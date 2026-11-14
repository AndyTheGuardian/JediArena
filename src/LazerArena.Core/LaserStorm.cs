namespace LazerArena.Core;

/// <summary>
/// Lasergewitter (LG-1 … LG-3): Ankuendigung, dann 3 s neutrale Blitze. Je Richtung ist hoechstens ein
/// Gewitter-Blitz unterwegs; ist eine Richtung frei, folgt der naechste nach kurzer zufaelliger Pause.
/// </summary>
public sealed class LaserStorm(GameConfig config)
{
    private float _timer;
    private readonly float?[] _refireTimers = new float?[2];   // [0] = nach oben, [1] = nach unten

    public StormPhase Phase { get; private set; } = StormPhase.Idle;

    /// <summary>Restzeit der aktuellen Phase (Idle: bis zur Ankuendigung).</summary>
    public float TimeRemaining => _timer;

    public void Reset(IRandomSource random)
    {
        Phase = StormPhase.Idle;
        _timer = random.Range(config.StormIntervalMin, config.StormIntervalMax);
    }

    /// <summary>
    /// Schreitet fort und meldet Phasenwechsel sowie faellige Schuesse.
    /// <paramref name="hasActiveBolt"/> beantwortet, ob in Richtung „oben“ (true) bzw. „unten“ noch ein
    /// Gewitter-Blitz unterwegs oder angedockt ist.
    /// </summary>
    public void Update(float dt, IRandomSource random, Action<GameEventType> onPhaseChange,
        Func<bool, bool> hasActiveBolt, Action<bool, float> fireNeutral)
    {
        _timer -= dt;
        switch (Phase)
        {
            case StormPhase.Idle when _timer <= 0:
                Phase = StormPhase.Warning;
                _timer += config.StormWarning;
                onPhaseChange(GameEventType.StormWarning);
                break;
            case StormPhase.Warning when _timer <= 0:
                Phase = StormPhase.Active;
                _timer += config.StormDuration;
                _refireTimers[0] = random.Range(0, config.StormRefireMax);
                _refireTimers[1] = random.Range(0, config.StormRefireMax);
                onPhaseChange(GameEventType.StormStarted);
                break;
            case StormPhase.Active when _timer <= 0:
                Phase = StormPhase.Idle;
                _timer += random.Range(config.StormIntervalMin, config.StormIntervalMax);
                onPhaseChange(GameEventType.StormEnded);
                break;
            case StormPhase.Active:
                FireDueShots(dt, random, hasActiveBolt, fireNeutral);
                break;
        }
    }

    private void FireDueShots(float dt, IRandomSource random, Func<bool, bool> hasActiveBolt, Action<bool, float> fireNeutral)
    {
        for (int i = 0; i < 2; i++)
        {
            bool up = i == 0;
            if (hasActiveBolt(up))
            {
                _refireTimers[i] = null;
                continue;
            }
            _refireTimers[i] ??= random.Range(config.StormRefireMin, config.StormRefireMax);
            _refireTimers[i] -= dt;
            if (_refireTimers[i] > 0) continue;
            fireNeutral(up, random.Range(-config.StormMaxAngle, config.StormMaxAngle));
            _refireTimers[i] = null;
        }
    }
}
