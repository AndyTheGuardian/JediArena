using LazerArena.Core;

namespace LazerArena.Presentation.Rendering;

/// <summary>Daten fuer den Siegerbildschirm (RS-5).</summary>
public sealed record VictoryInfo(string WinnerLabel, bool HumanWon, int WinnerScore, int LoserScore, bool ShowContinue)
{
    public static VictoryInfo From(GameSession session, Localization.Localizer t, bool showContinue)
    {
        var sim = session.Simulation;
        var winner = sim.Winner ?? PlayerId.One;
        string label = winner == PlayerId.One ? t["hud.p1"] : session.TwoPlayers ? t["hud.p2"] : t["hud.ai"];
        bool humanWon = session.TwoPlayers || winner == PlayerId.One;
        return new VictoryInfo(label, humanWon, sim.PlayerFor(winner).Score, sim.PlayerFor(winner.Opponent()).Score, showContinue);
    }
}
