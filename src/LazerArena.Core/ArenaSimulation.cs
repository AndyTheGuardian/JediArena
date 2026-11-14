using System.Numerics;

namespace LazerArena.Core;

/// <summary>
/// Deterministische Spiellogik von Lazer Arena mit festem Zeitschritt (SO-3) und Zufall mit Startwert (SO-4).
/// Eingaben: <see cref="SetSwordTarget"/> und <see cref="Fire"/>; Ausgabe: Zustand und <see cref="Events"/>.
/// </summary>
public sealed class ArenaSimulation
{
    private readonly IRandomSource _random;
    private readonly List<Bolt> _bolts = [];
    private readonly List<GameEvent> _events = [];
    private readonly List<GameEvent> _inputEvents = [];   // Ereignisse aus Eingaben, erscheinen im naechsten Schritt
    private readonly Shield[] _shields;
    private float _phaseTimer;
    private float _accumulator;

    public ArenaSimulation(GameConfig? config = null, GameOptions? options = null, int seed = 1)
        : this(config, options, new SeededRandom(seed)) { }

    public ArenaSimulation(GameConfig? config, GameOptions? options, IRandomSource random)
    {
        Config = config ?? GameConfig.Default;
        Options = options ?? GameOptions.Default;
        _random = random;
        Players = [new Player(PlayerId.One, Config), new Player(PlayerId.Two, Config)];
        _shields =
        [
            new Shield(PlayerId.One, Players[0].Position, Config, Options.ShieldShape),
            new Shield(PlayerId.Two, Players[1].Position, Config, Options.ShieldShape),
        ];
        Drone = new Drone(Config);
        Storm = new LaserStorm(Config);
        StartRound();
    }

    public GameConfig Config { get; }
    public GameOptions Options { get; }
    public IReadOnlyList<Player> Players { get; }
    public Drone Drone { get; }
    public LaserStorm Storm { get; }
    public IReadOnlyList<Bolt> Bolts => _bolts;
    public MatchPhase Phase { get; private set; }
    public float PhaseTimeRemaining => _phaseTimer;
    public float RoundTime { get; private set; }
    public bool Paused { get; private set; }
    public PlayerId? Winner { get; private set; }
    public PlayerId? LastHitPlayer { get; private set; }

    /// <summary>Ereignisse des letzten <see cref="Step"/> bzw. <see cref="Update"/> einschliesslich der Eingaben davor.</summary>
    public IReadOnlyList<GameEvent> Events => _events;

    public Player PlayerFor(PlayerId id) => Players[(int)id];
    public Shield ShieldFor(PlayerId id) => _shields[(int)id];

    /// <summary>BL-7: +10 % alle 20 s, hoechstens 200 %.</summary>
    public float SpeedFactor =>
        MathF.Min(Config.MaxSpeedFactor, 1 + Config.SpeedStepFactor * MathF.Floor(RoundTime / Config.SpeedStepInterval));

    // ------------------------------------------------------------------ Eingaben

    public void SetSwordTarget(PlayerId id, float angleDegrees) => PlayerFor(id).SetTargetAngle(angleDegrees);

    public void Pause() => Paused = true;   // SO-1
    public void Resume() => Paused = false;

    /// <summary>
    /// SP-3: Die Drohne feuert im Schwertwinkel des Schuetzen. Gesperrt waehrend Countdown (RS-4), aktivem Gewitter
    /// (LG-6), Abklingzeit und bei erreichtem Blitzlimit (SP-4). Im Modus Zurueckwerfen ersetzt das Tippen bei
    /// angedocktem Blitz den Schuss (AD-7).
    /// </summary>
    public bool Fire(PlayerId id)
    {
        var player = PlayerFor(id);
        if (Paused || Phase != MatchPhase.Playing || Storm.Phase == StormPhase.Active || player.Cooldown > 0)
            return false;

        var docked = _bolts.FirstOrDefault(b => b.State == BoltState.Docked && b.DockedAt == id);
        bool throwBack = Options.DockedBolt == DockedBoltBehavior.ThrowBack && docked is not null;
        int ownBolts = OwnedBoltCount(id) - (throwBack && docked!.Owner == id.AsOwner() ? 1 : 0);
        if (ownBolts >= Config.MaxBoltsPerPlayer)
            return false;

        if (throwBack)
        {
            docked!.State = BoltState.Gone;
            _bolts.Add(Bolt.FromPoint(id.AsOwner(), player.SwordTip, player.SwordDirection));
            _inputEvents.Add(new GameEvent(GameEventType.BoltThrownBack, id));
        }
        else
        {
            _bolts.Add(Bolt.FromDrone(id.AsOwner(), player.SwordDirection));
            _inputEvents.Add(new GameEvent(GameEventType.BoltFired, id));
        }
        player.Cooldown = Config.ShotCooldown;
        return true;
    }

    /// <summary>Nur fuer Tests: Blitz direkt einsetzen.</summary>
    internal void AddBolt(Bolt bolt) => _bolts.Add(bolt);

    /// <summary>SP-4: lebende eigene Blitze, auch die am gegnerischen Schwert angedockten.</summary>
    public int OwnedBoltCount(PlayerId id) => _bolts.Count(b => b.State != BoltState.Gone && b.Owner == id.AsOwner());

    // ------------------------------------------------------------------ Zeit

    /// <summary>Echtzeit-Fortschritt: zerlegt <paramref name="elapsedSeconds"/> in feste Zeitschritte (SO-3).</summary>
    public void Update(float elapsedSeconds)
    {
        BeginEvents();
        _accumulator += elapsedSeconds;
        const float tolerance = 1e-5f;   // Rundungsfehler beim Aufsummieren duerfen keinen Schritt verschlucken
        while (_accumulator >= Config.TickSeconds - tolerance)
        {
            _accumulator -= Config.TickSeconds;
            StepInternal();
        }
    }

    /// <summary>Genau ein fester Zeitschritt.</summary>
    public void Step()
    {
        BeginEvents();
        StepInternal();
    }

    private void BeginEvents()
    {
        _events.Clear();
        _events.AddRange(_inputEvents);
        _inputEvents.Clear();
    }

    private void StepInternal()
    {
        if (Paused || Phase == MatchPhase.MatchOver)
            return;

        float dt = Config.TickSeconds;
        foreach (var p in Players)
            p.UpdateSword(dt, Options.SwordTurning);

        switch (Phase)
        {
            case MatchPhase.Countdown:
                int before = (int)MathF.Ceiling(_phaseTimer);
                _phaseTimer -= dt;
                int after = (int)MathF.Ceiling(_phaseTimer);
                if (after != before && after == 2) _events.Add(new GameEvent(GameEventType.CountdownTwo));   // RS-4: Anzeige „2“
                if (after != before && after == 1) _events.Add(new GameEvent(GameEventType.CountdownOne));   // Anzeige „1“
                if (_phaseTimer <= 0)
                {
                    Phase = MatchPhase.Playing;
                    _events.Add(new GameEvent(GameEventType.RoundStarted));
                }
                break;
            case MatchPhase.Playing:
                StepPlaying(dt);
                break;
            case MatchPhase.RoundOver:
                _phaseTimer -= dt;
                if (_phaseTimer <= 0)
                    StartRound();
                break;
        }
    }

    private void StepPlaying(float dt)
    {
        RoundTime += dt;
        foreach (var p in Players)
            p.Cooldown = MathF.Max(0, p.Cooldown - dt);

        Drone.Update(dt, _random);
        Storm.Update(dt, _random, type => _events.Add(new GameEvent(type)), HasActiveNeutral, FireNeutral);

        foreach (var bolt in _bolts.ToList())
        {
            if (Phase != MatchPhase.Playing) break;
            if (bolt.State == BoltState.Growing) GrowBolt(bolt, dt);
            else if (bolt.State == BoltState.Docked) HoldBolt(bolt, dt);
        }
        _bolts.RemoveAll(b => b.State == BoltState.Gone);
        if (Phase != MatchPhase.Playing)
            return;

        foreach (var shield in _shields)
        {
            if (shield.UpdateRegrowth(dt, IsCrossedByBolt) is not null)
                _events.Add(new GameEvent(GameEventType.ShieldSegmentRegrown, shield.Owner));
        }
    }

    /// <summary>LG-3: Ist in dieser Richtung noch ein Gewitter-Blitz unterwegs oder angedockt?</summary>
    private bool HasActiveNeutral(bool up) => _bolts.Any(b =>
        b.Owner == BoltOwner.Neutral && b.State != BoltState.Gone && (b.Target == PlayerId.Two) == up);

    /// <summary>LG-3/LG-4: neutraler Blitz nach oben (zu Spieler 2) oder unten (zu Spieler 1).</summary>
    private void FireNeutral(bool up, float angleDegrees)
    {
        float a = Angles.ToRadians(angleDegrees);
        var dir = new Vector2(MathF.Sin(a), up ? -MathF.Cos(a) : MathF.Cos(a));
        _bolts.Add(Bolt.FromDrone(BoltOwner.Neutral, dir));
    }

    private void GrowBolt(Bolt bolt, float dt)
    {
        float growth = Config.BoltSpeedFieldHeights * Config.FieldHeight * SpeedFactor * dt;
        float end = bolt.Length + growth;
        var origin = bolt.Origin(Drone);
        var target = PlayerFor(bolt.Target);

        for (float s = MathF.Min(bolt.Length + Config.CollisionSampleStep, end); ; s = MathF.Min(s + Config.CollisionSampleStep, end))
        {
            bolt.Length = s;
            var head = Bolt.PointAt(origin, bolt.Direction, s, Config.FieldWidth, Options.Walls);
            if (head is null || head.Value.Y < 0 || head.Value.Y > Config.FieldHeight)
            {
                bolt.State = BoltState.Gone;   // BL-3 (Erloeschen) bzw. Feld verlassen
                return;
            }
            if (TryDock(bolt, target, head.Value) || TryHitShield(bolt, head.Value) || TryHitPlayer(bolt, target, head.Value))
                return;
            if (s >= end)
                return;
        }
    }

    /// <summary>AD-1, AD-2, AD-4: Andocken an der Schwertspitze des Ziels, hoechstens ein Blitz je Schwert.</summary>
    private bool TryDock(Bolt bolt, Player target, Vector2 head)
    {
        if (Vector2.Distance(head, target.SwordTip) > Config.DockRadius)
            return false;
        if (_bolts.Any(b => b.State == BoltState.Docked && b.DockedAt == target.Id))
            return false;

        bolt.State = BoltState.Docked;
        bolt.DockedAt = target.Id;
        bolt.HoldRemaining = _random.Range(Config.DockHoldMin, Config.DockHoldMax);   // AD-3
        _events.Add(new GameEvent(GameEventType.BoltDocked, target.Id));
        return true;
    }

    /// <summary>BL-4: Schildsegment wird zerstoert, der Blitz erlischt.</summary>
    private bool TryHitShield(Bolt bolt, Vector2 head)
    {
        var segment = ShieldFor(bolt.Target).SegmentAt(head);
        if (segment is null)
            return false;

        segment.Alive = false;
        bolt.State = BoltState.Gone;
        _events.Add(new GameEvent(GameEventType.ShieldSegmentDestroyed, bolt.Target));
        return true;
    }

    /// <summary>BL-5, RS-1: Treffer beendet die Runde; der Gegner des Getroffenen punktet (auch bei neutralem Blitz).</summary>
    private bool TryHitPlayer(Bolt bolt, Player target, Vector2 head)
    {
        if (Vector2.Distance(head, target.Position) > Config.PlayerBodyRadius)
            return false;

        bolt.State = BoltState.Gone;
        var scorer = PlayerFor(target.Id.Opponent());
        scorer.Score++;
        LastHitPlayer = target.Id;
        _events.Add(new GameEvent(GameEventType.PlayerHit, target.Id));

        if (scorer.Score >= Config.PointsToWin)   // RS-3
        {
            Phase = MatchPhase.MatchOver;
            Winner = scorer.Id;
            _events.Add(new GameEvent(GameEventType.MatchWon, scorer.Id));
        }
        else
        {
            Phase = MatchPhase.RoundOver;
            _phaseTimer = Config.RoundEndPause;
        }
        return true;
    }

    /// <summary>AD-3: Haltezeit 1–2 s, danach erlischt der Blitz.</summary>
    private void HoldBolt(Bolt bolt, float dt)
    {
        bolt.HoldRemaining -= dt;
        if (bolt.HoldRemaining > 0)
            return;
        bolt.State = BoltState.Gone;
        _events.Add(new GameEvent(GameEventType.BoltFizzled, bolt.DockedAt));
    }

    /// <summary>SC-2: Verlaeuft ein Blitz durch das Rechteck?</summary>
    private bool IsCrossedByBolt(RectF rect)
    {
        foreach (var bolt in _bolts.Where(b => b.State != BoltState.Gone))
        {
            var origin = bolt.Origin(Drone);
            if (bolt.State == BoltState.Docked)
            {
                var tip = PlayerFor(bolt.DockedAt!.Value).SwordTip;
                if (SegmentCrosses(origin, tip, rect)) return true;
                continue;
            }
            for (float s = 0; s <= bolt.Length; s += Config.CollisionSampleStep)
            {
                var p = Bolt.PointAt(origin, bolt.Direction, s, Config.FieldWidth, Options.Walls);
                if (p is { } point && rect.Contains(point)) return true;
            }
        }
        return false;
    }

    private bool SegmentCrosses(Vector2 a, Vector2 b, RectF rect)
    {
        float length = Vector2.Distance(a, b);
        for (float s = 0; s <= length; s += Config.CollisionSampleStep)
            if (rect.Contains(Vector2.Lerp(a, b, s / length))) return true;
        return false;
    }

    // ------------------------------------------------------------------ Runden

    /// <summary>RS-2, RS-4: alles zuruecksetzen, Countdown starten. Punkte bleiben erhalten.</summary>
    private void StartRound()
    {
        _bolts.Clear();
        foreach (var p in Players) p.ResetForRound();
        foreach (var s in _shields) s.Restore();
        Drone.Reset(_random);
        Storm.Reset(_random);
        RoundTime = 0;
        Phase = MatchPhase.Countdown;
        _phaseTimer = Config.CountdownSeconds;
        _inputEvents.Add(new GameEvent(GameEventType.CountdownStarted));   // erscheint im naechsten Schritt (auch bei der allerersten Runde)
    }
}
