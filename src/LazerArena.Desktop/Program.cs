using LazerArena.Game;

// Desktop-Variante zum schnellen Ausprobieren (Maus = Touch, F12 = Screenshot).
// Optional: --landscape fuer ein Querformat-Fenster.
bool landscape = args.Contains("--landscape");
using var game = new LazerArenaGame(new DesktopPlatform(), landscape ? 1170 : 540, landscape ? 540 : 1170);
game.Run();

internal sealed class DesktopPlatform : IPlatformServices
{
    private static readonly string Base =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LazerArena");

    public string SettingsPath => Path.Combine(Base, "settings.json");
    public string ScreenshotDirectory => Path.Combine(Base, "screenshots");
    public void Quit() => Environment.Exit(0);
}
