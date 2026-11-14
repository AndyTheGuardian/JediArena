using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using LazerArena.Game;

namespace LazerArena.Android;

[Activity(
    Label = "@string/app_name",
    MainLauncher = true,
    Icon = "@mipmap/icon",
    AlwaysRetainTaskState = true,
    LaunchMode = LaunchMode.SingleInstance,
    ScreenOrientation = ScreenOrientation.FullUser,   // Hoch- und Querformat
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity
{
    private LazerArenaGame? _game;

    protected override void OnCreate(Bundle? bundle)
    {
        base.OnCreate(bundle);
        _game = new LazerArenaGame(new AndroidPlatform(this), fullScreen: true);
        var view = (View)_game.Services.GetService(typeof(View))!;
        SetContentView(view);
        _game.Run();
    }
}

internal sealed class AndroidPlatform(Activity activity) : IPlatformServices
{
    private static string Base => System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);

    public string SettingsPath => Path.Combine(Base, "settings.json");
    public string ScreenshotDirectory => activity.GetExternalFilesDir(null)?.AbsolutePath ?? Base;
    public void Quit() => activity.FinishAndRemoveTask();
}
