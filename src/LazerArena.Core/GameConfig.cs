namespace LazerArena.Core;

/// <summary>
/// Alle Zahlenwerte des Regelwerks an einer Stelle. Laengen in Feldbreiten (Feldbreite = 1),
/// Zeiten in Sekunden, Winkel in Grad. Die Kommentare nennen die Regel-ID aus REGELWERK.md.
/// </summary>
public sealed record GameConfig
{
    public static GameConfig Default { get; } = new();

    // Spielfeld (SF)
    public float FieldWidth { get; init; } = 1f;
    public float FieldAspect { get; init; } = 0.53f;              // Breite / Hoehe
    public float FieldHeight => FieldWidth / FieldAspect;
    public float Player1YRatio { get; init; } = 0.935f;           // SF-1, Anteil der Feldhoehe
    public float Player2YRatio { get; init; } = 0.065f;
    public float DroneYRatio { get; init; } = 0.5f;               // SF-2

    // Drohne (SF-2)
    public float DroneRadius { get; init; } = 0.045f;
    public float DroneSpeedMin { get; init; } = 0.15f;            // Annahme A3
    public float DroneSpeedMax { get; init; } = 0.45f;            // Annahme A3
    public float DroneChangeMin { get; init; } = 1f;
    public float DroneChangeMax { get; init; } = 3f;

    // Spieler und Schwert (SP)
    public float PlayerBodyRadius { get; init; } = 0.05f;
    public float SwordHandOffset { get; init; } = 0.06f;
    public float SwordLength { get; init; } = 0.22f;              // SP-2
    public float SwordMaxAngle { get; init; } = 60f;              // SP-2
    public float SwordLimitedTurnRate { get; init; } = 240f;      // SP-2, OP-4
    public float ShotCooldown { get; init; } = 0.8f;              // SP-4
    public int MaxBoltsPerPlayer { get; init; } = 2;              // SP-4

    // Blitze (BL)
    public float BoltSpeedFieldHeights { get; init; } = 1.2f;     // BL-1
    public float SpeedStepInterval { get; init; } = 20f;          // BL-7
    public float SpeedStepFactor { get; init; } = 0.1f;           // BL-7
    public float MaxSpeedFactor { get; init; } = 2f;              // BL-7
    public float CollisionSampleStep { get; init; } = 0.005f;

    // Andocken (AD)
    public float DockRadius { get; init; } = 0.08f;               // AD-1
    public float DockHoldMin { get; init; } = 1f;                 // AD-3
    public float DockHoldMax { get; init; } = 2f;                 // AD-3

    // Schild (SF-3, SC)
    public int ShieldSegments { get; init; } = 16;
    public int ShieldRows { get; init; } = 2;
    public float ShieldRowHeightRatio { get; init; } = 0.022f;    // Anteil der Feldhoehe
    public float[] StraightShieldRowsP1 { get; init; } = [0.83f, 0.855f]; // vordere, hintere Reihe (Anteil Feldhoehe)
    public float CurvedArcDegrees { get; init; } = 30f;
    public float[] CurvedRadii { get; init; } = [0.90f, 0.84f];
    public float CurvedCenterBehind { get; init; } = 0.72f;
    public float ShieldRegrowInterval { get; init; } = 5f;        // SC-1

    // Lasergewitter (LG)
    public float StormIntervalMin { get; init; } = 15f;           // LG-1
    public float StormIntervalMax { get; init; } = 30f;
    public float StormWarning { get; init; } = 1f;                // LG-2
    public float StormDuration { get; init; } = 3f;               // LG-3
    public float StormRefireMin { get; init; } = 0.05f;           // LG-3: Pause vor dem naechsten Blitz je Richtung
    public float StormRefireMax { get; init; } = 0.35f;           // LG-3
    public float StormMaxAngle { get; init; } = 60f;              // LG-3

    // Runden (RS)
    public int PointsToWin { get; init; } = 3;                    // RS-3
    public float CountdownSeconds { get; init; } = 3f;            // RS-4
    public float RoundEndPause { get; init; } = 2f;               // Annahme A4

    // Simulation (SO-3)
    public float TickSeconds { get; init; } = 1f / 60f;
}
