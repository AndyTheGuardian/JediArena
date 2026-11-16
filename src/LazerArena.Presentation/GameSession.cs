using LazerArena.Core;
using LazerArena.Core.Ai;
using LazerArena.Presentation.Input;
using LazerArena.Presentation.Settings;

namespace LazerArena.Presentation;

/// <summary>Ein laufendes Match: Simulation, optional KI-Gegner (Spieler 2) und Umsetzung der Eingabebefehle.</summary>
public sealed class GameSession
{
    private readonly AiController? _ai;

    public GameSession(UserSettings settings, bool twoPlayers, int seed, GameConfig? config = null)
    {
        TwoPlayers = twoPlayers;
        Simulation = new ArenaSimulation(config ?? GameConfig.Default, settings.ToGameOptions(), seed);
        if (!twoPlayers)
            _ai = new AiController(PlayerId.Two, settings.AiLevel, new SeededRandom(seed ^ 0x5EED));
        AiLevel = settings.AiLevel;
        Bolts = settings.Bolts;
        Look = settings.Advanced;
    }

    public ArenaSimulation Simulation { get; }
    public bool TwoPlayers { get; }
    public AiLevel AiLevel { get; }

    /// <summary>OP-14: Blitzform des Grafikstils, mit dem die Partie gestartet wurde.</summary>
    public BoltShape Bolts { get; }

    /// <summary>OP-15: Feineinstellungen der Darstellung (Leuchten).</summary>
    public AdvancedSettings Look { get; }

    /// <summary>Gesammelte Ereignisse seit dem letzten Abholen (fuer Ton).</summary>
    public List<GameEvent> PendingEvents { get; } = [];

    public void Apply(IEnumerable<InputCommand> commands)
    {
        foreach (var command in commands)
        {
            switch (command.Type)
            {
                case CommandType.Pause:
                    TogglePause();
                    break;
                case CommandType.RotateSword when !Simulation.Paused:
                    var player = Simulation.PlayerFor(command.Player);
                    Simulation.SetSwordTarget(command.Player, player.TargetAngle + command.DeltaDegrees);
                    break;
                case CommandType.Fire when !Simulation.Paused:
                    if (!TwoPlayers && command.Player == PlayerId.Two) break;
                    Simulation.Fire(command.Player);
                    break;
            }
        }
    }

    public void Update(float elapsedSeconds)
    {
        if (!Simulation.Paused)
            _ai?.Update(Simulation, elapsedSeconds);
        Simulation.Update(elapsedSeconds);
        PendingEvents.AddRange(Simulation.Events);
    }

    public void TogglePause()
    {
        if (Simulation.Paused) Simulation.Resume();
        else Simulation.Pause();
    }

    /// <summary>SO-1: Wechsel der App in den Hintergrund pausiert.</summary>
    public void OnAppBackgrounded() => Simulation.Pause();
}
