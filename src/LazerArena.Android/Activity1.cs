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
    private AndroidPlatform _platform = null!;

    protected override void OnCreate(Bundle? bundle)
    {
        base.OnCreate(bundle);
        // SO-7: auch im Streifen der Kamera-Aussparung zeichnen (sonst bleibt oben ein schwarzer Rand)
        if (Window?.Attributes is { } attributes)
        {
            attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
            Window.Attributes = attributes;
        }
        _platform = new AndroidPlatform(this);
        _game = new LazerArenaGame(_platform, fullScreen: true);
        var view = (View)_game.Services.GetService(typeof(View))!;
        SetContentView(view);
        view.ViewTreeObserver!.GlobalLayout += (_, _) => _platform.UpdateCutout(view);
        HideSystemBars();
        _game.Run();
    }

    protected override void OnResume()
    {
        base.OnResume();
        HideSystemBars();
    }

    /// <summary>Nach Dialogen, Benachrichtigungen oder dem Wischen vom Rand die Leisten wieder ausblenden.</summary>
    public override void OnWindowFocusChanged(bool hasFocus)
    {
        base.OnWindowFocusChanged(hasFocus);
        if (hasFocus) HideSystemBars();
    }

    /// <summary>
    /// Vollbild ohne Status- und Navigationsleiste („immersiv“): Die Leisten erscheinen nur kurz, wenn man vom Rand
    /// wischt, und verdecken sonst keine Knoepfe. Ab Android 11 ueber den WindowInsetsController, darunter ueber die
    /// aelteren System-UI-Flags.
    /// </summary>
    private void HideSystemBars()
    {
        var window = Window;
        if (window is null) return;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
        {
            window.SetDecorFitsSystemWindows(false);
            if (window.InsetsController is { } controller)
            {
                controller.Hide(WindowInsets.Type.SystemBars());
                controller.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
            }
            return;
        }
#pragma warning disable CA1422 // Android 10: nur die aelteren Flags stehen zur Verfuegung
        window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.ImmersiveSticky | SystemUiFlags.HideNavigation
            | SystemUiFlags.Fullscreen | SystemUiFlags.LayoutStable | SystemUiFlags.LayoutHideNavigation | SystemUiFlags.LayoutFullscreen);
#pragma warning restore CA1422
    }
}

internal sealed class AndroidPlatform(Activity activity) : IPlatformServices
{
    private static string Base => System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);

    public string SettingsPath => Path.Combine(Base, "settings.json");
    public string ScreenshotDirectory => activity.GetExternalFilesDir(null)?.AbsolutePath ?? Base;

    private Cutout _cutout;
    public Cutout Cutout { get { lock (this) return _cutout; } }

    /// <summary>Liest die Kamera-Aussparung nach jedem Layout (Drehen, Ein-/Ausblenden der Leisten) auf dem UI-Thread.</summary>
    public void UpdateCutout(View view)
    {
        var cutout = view.RootWindowInsets?.DisplayCutout;
        int top = cutout?.SafeInsetTop ?? 0;
        var rects = cutout?.BoundingRects?.Where(r => r.Top <= 0).Select(r => (r.Left, r.Right)).ToList() ?? [];
        var value = new Cutout(top, Cutout.IsCentered(view.Width, rects));
        lock (this) _cutout = value;
    }
    public void Quit() => activity.FinishAndRemoveTask();
}
