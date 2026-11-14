using System.Numerics;

namespace LazerArena.Core.Ai;

/// <summary>KI-2: Reaktionszeit, Zielgenauigkeit (Streuung in Grad) und Schussabstand je Stufe.</summary>
public sealed record AiProfile(float ReactionSeconds, float AimSpreadDegrees, float ShotIntervalSeconds)
{
    public static AiProfile For(AiLevel level) => level switch
    {
        AiLevel.Easy => new AiProfile(0.400f, 14f, 2.2f),
        AiLevel.Medium => new AiProfile(0.250f, 7f, 1.5f),
        AiLevel.Hard => new AiProfile(0.120f, 2.5f, 1.0f),
        _ => throw new ArgumentOutOfRangeException(nameof(level)),
    };
}

/// <summary>
/// KI-Gegner. Er bedient die Simulation ausschliesslich ueber dieselben Eingaben wie ein Mensch
/// (<see cref="ArenaSimulation.SetSwordTarget"/>, <see cref="ArenaSimulation.Fire"/>) und unterliegt damit
/// denselben Regeln (KI-3). Er entscheidet nur alle <see cref="AiProfile.ReactionSeconds"/> neu.
/// </summary>
public sealed class AiController(PlayerId id, AiProfile profile, IRandomSource random)
{
    private const float AimToleranceDegrees = 4f;
    private float _decisionTimer;
    private float _shotTimer;
    private float _aim;
    private bool _wantsToShoot;
    private bool _wantsToThrowBack;

    public AiController(PlayerId id, AiLevel level, IRandomSource random) : this(id, AiProfile.For(level), random) { }

    public PlayerId Id { get; } = id;
    public AiProfile Profile { get; } = profile;

    public void Update(ArenaSimulation sim, float dt)
    {
        _shotTimer -= dt;
        _decisionTimer -= dt;
        if (_decisionTimer <= 0)
        {
            _decisionTimer += Profile.ReactionSeconds;
            Decide(sim);
        }

        sim.SetSwordTarget(Id, _aim);
        var me = sim.PlayerFor(Id);
        bool aligned = MathF.Abs(me.SwordAngle - _aim) <= AimToleranceDegrees;
        if (_wantsToThrowBack && aligned && sim.Fire(Id))
            _wantsToThrowBack = false;
        else if (_wantsToShoot && aligned && _shotTimer <= 0 && sim.Fire(Id))
            _shotTimer = Profile.ShotIntervalSeconds * random.Range(0.8f, 1.2f);
    }

    private void Decide(ArenaSimulation sim)
    {
        var me = sim.PlayerFor(Id);
        var opponent = sim.PlayerFor(Id.Opponent());
        float spread = random.Range(-Profile.AimSpreadDegrees, Profile.AimSpreadDegrees);

        bool holdsBolt = sim.Bolts.Any(b => b.State == BoltState.Docked && b.DockedAt == Id);
        _wantsToThrowBack = holdsBolt && sim.Options.DockedBolt == DockedBoltBehavior.ThrowBack;
        if (_wantsToThrowBack)
        {
            _aim = me.AngleFor(opponent.Position - me.SwordTip) + spread;
            return;
        }

        var interception = FindInterceptionPoint(sim, me);
        if (interception is { } point)
        {
            _aim = me.AngleFor(point - me.Position) + spread;
            _wantsToShoot = false;
            return;
        }

        _aim = me.AngleFor(opponent.Position - sim.Drone.Position) + spread;
        _wantsToShoot = true;
    }

    /// <summary>Wo schneidet der gefaehrlichste anfliegende Blitz den Kreis, den die Schwertspitze ueberstreicht?</summary>
    private static Vector2? FindInterceptionPoint(ArenaSimulation sim, Player me)
    {
        var config = sim.Config;
        float tipRadius = config.SwordHandOffset + config.SwordLength;
        Vector2? best = null;
        float bestRemaining = float.MaxValue;

        foreach (var bolt in sim.Bolts.Where(b => b.State == BoltState.Growing && b.Target == me.Id))
        {
            var origin = bolt.Origin(sim.Drone);
            float maxLength = config.FieldHeight * 2;
            for (float s = bolt.Length; s < maxLength; s += 0.01f)
            {
                var p = Bolt.PointAt(origin, bolt.Direction, s, config.FieldWidth, sim.Options.Walls);
                if (p is null) break;
                if (Vector2.Distance(p.Value, me.Position) > tipRadius) continue;

                float remaining = s - bolt.Length;
                if (remaining < bestRemaining)
                {
                    bestRemaining = remaining;
                    best = p;
                }
                break;
            }
        }
        return best;
    }
}
