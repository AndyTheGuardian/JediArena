using LazerArena.Presentation.Rendering;
using LazerArena.Presentation.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace LazerArena.Game;

/// <summary>
/// MonoGame-Rahmen: zeichnet die logische Pixelflaeche ganzzahlig vergroessert und mittig auf den Bildschirm
/// und reicht Beruehrungen in logischen Koordinaten an den aktiven Bildschirm weiter.
/// </summary>
public sealed class LazerArenaGame : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly IPlatformServices _platform;
    private readonly PointerInput _pointers = new();
    private readonly SoundPlayer _sound = new();
    private AppContext _app = null!;
    private IScreen _screen = null!;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D? _texture;
    private PixelCanvas? _canvas;
    private uint[] _buffer = [];
    private KeyboardState _previousKeys;
    private int _frame;
    private Rectangle _destination;
    private RenderTarget2D? _glowTarget;
    private Texture2D? _scanlines;
    private Texture2D? _vignette;
    private (int, int, int) _scanlineKey;
    private int _vignetteKey;

    public LazerArenaGame(IPlatformServices platform, int windowWidth = 540, int windowHeight = 1170, bool fullScreen = false)
    {
        _platform = platform;
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = windowWidth,
            PreferredBackBufferHeight = windowHeight,
            SupportedOrientations = DisplayOrientation.Portrait | DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight,
            IsFullScreen = fullScreen,   // Android: Statusleiste ausblenden, sonst ueberdeckt sie die Anzeige
        };
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        TouchPanel.EnableMouseTouchPoint = true;
        Deactivated += (_, _) => _screen?.OnBackgrounded();   // SO-1
    }

    public UserSettings Settings => _app.Settings;

    protected override void Initialize()
    {
        _app = new AppContext(_platform, new JsonSettingsStore(_platform.SettingsPath));
        _screen = new TitleScreen(_app);
        _sound.Warmup();
        base.Initialize();
    }

    protected override void LoadContent() => _spriteBatch = new SpriteBatch(GraphicsDevice);

    protected override void Update(GameTime gameTime)
    {
        float dt = MathF.Min((float)gameTime.ElapsedGameTime.TotalSeconds, 0.1f);
        _app.ScreenWidth = GraphicsDevice.Viewport.Width;
        _app.ScreenHeight = GraphicsDevice.Viewport.Height - _platform.Cutout.Reserved;   // SO-7
        UpdateDestination(_screen.Layout);

        var keys = ReadKeyboard();
        var pointers = _pointers.Read(new Vector2(_destination.X, _destination.Y), _destination.Width / (float)_screen.Layout.Width);
        _screen = _screen.Update(dt, pointers, gameTime.TotalGameTime.TotalSeconds, keys);
        foreach (var cue in _screen.DrainCues())
            _sound.Play(cue, _app.Settings, _app.Text.Language);
        _sound.TickVoices(dt, _app.Text.Language);
        _sound.Update(_screen.Audio, IsActive, _app.Settings);
        base.Update(gameTime);
    }

    private KeyboardCommands ReadKeyboard()
    {
        var k = Keyboard.GetState();
        bool Pressed(Keys key) => k.IsKeyDown(key) && !_previousKeys.IsKeyDown(key);
        float Axis(Keys left, Keys right) => (k.IsKeyDown(right) ? 1 : 0) - (k.IsKeyDown(left) ? 1 : 0);
        var result = new KeyboardCommands(
            Axis(Keys.Left, Keys.Right), Pressed(Keys.Space) || Pressed(Keys.Up),
            -Axis(Keys.A, Keys.D), Pressed(Keys.W),
            Pressed(Keys.Escape) || Pressed(Keys.Back));
        if (Pressed(Keys.F12)) SaveScreenshot();
        _previousKeys = k;
        return result;
    }

    protected override void Draw(GameTime gameTime)
    {
        var layout = _screen.Layout;
        if (_canvas is null || _canvas.Width != layout.Width || _canvas.Height != layout.Height)
        {
            _canvas = new PixelCanvas(layout.Width, layout.Height);
            _texture?.Dispose();
            _texture = new Texture2D(GraphicsDevice, layout.Width, layout.Height, false, SurfaceFormat.Color);
            _buffer = new uint[layout.Width * layout.Height];
        }
        _screen.Render(_canvas, _frame++);
        _canvas.CopyTo(_buffer);
        _texture!.SetData(_buffer);

        bool tv = _app.Settings.TvEffect;   // OP-12
        if (tv) PrepareTvEffect(layout);    // vor dem Bildschirm-Puffer, da ein Zielwechsel ihn leeren kann

        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_texture, _destination, Color.White);
        _spriteBatch.End();
        if (tv) DrawTvEffect();
        base.Draw(gameTime);
    }

    /// <summary>
    /// Leuchten vorbereiten: Bild verkleinert und weich gefiltert in ein kleines Zielbild zeichnen. Hochskaliert und
    /// additiv darueber gelegt ergibt das einen weichen Schein um helle Stellen (wie Phosphor einer Bildroehre).
    /// </summary>
    private void PrepareTvEffect(LazerArena.Presentation.Layout.ScreenLayout layout)
    {
        var tv = _app.Settings.Advanced;   // OP-15: Feineinstellungen
        // V1.1: Zeilen und Leuchten im Raster des klassischen Stils, damit der Effekt in beiden Stilen gleich grob aussieht
        int rows = TvEffect.ScanlineRows(layout.Height, _app.Settings.Style);
        float coarse = TvEffect.CoarseFactor(_app.Settings.Style);
        int gw = Math.Max(1, (int)(layout.Width / (tv.GlowDownscale * coarse))), gh = Math.Max(1, (int)(layout.Height / (tv.GlowDownscale * coarse)));
        if (_glowTarget is null || _glowTarget.Width != gw || _glowTarget.Height != gh)
        {
            _glowTarget?.Dispose();
            _glowTarget = new RenderTarget2D(GraphicsDevice, gw, gh);
        }
        var scanKey = (rows, tv.TvScanlineDarkness, tv.ScanlineRowParts);
        if (_scanlines is null || _scanlineKey != scanKey)
        {
            _scanlines?.Dispose();
            _scanlines = new Texture2D(GraphicsDevice, 1, rows * tv.ScanlineRowParts);
            _scanlines.SetData(TvEffect.ScanlineTexture(rows, tv.TvScanlineDarkness / 100f, tv.ScanlineRowParts));
            _scanlineKey = scanKey;
        }
        if (_vignette is null || _vignetteKey != tv.TvVignette)
        {
            _vignette?.Dispose();
            _vignette = new Texture2D(GraphicsDevice, 64, 64);
            _vignette.SetData(TvEffect.VignetteTexture(64, tv.TvVignette / 100f));
            _vignetteKey = tv.TvVignette;
        }
        GraphicsDevice.SetRenderTarget(_glowTarget);
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        _spriteBatch.Draw(_texture!, new Rectangle(0, 0, gw, gh), Color.White);
        _spriteBatch.End();
        GraphicsDevice.SetRenderTarget(null);
    }

    private void DrawTvEffect()
    {
        var tv = _app.Settings.Advanced;
        float spread = tv.TvGlowSpread / 100f;
        int spreadX = (int)(_destination.Width * spread), spreadY = (int)(_destination.Height * spread / 2);
        var glowArea = new Rectangle(_destination.X - spreadX, _destination.Y - spreadY,
            _destination.Width + 2 * spreadX, _destination.Height + 2 * spreadY);
        _spriteBatch.Begin(blendState: BlendState.Additive, samplerState: SamplerState.LinearClamp);
        _spriteBatch.Draw(_glowTarget!, glowArea, Color.White * (tv.TvGlow / 100f));
        _spriteBatch.End();

        _spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_scanlines!, _destination, Color.White);   // dunkle Linien im Zeilenraster des klassischen Stils
        _spriteBatch.End();

        _spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.LinearClamp);
        _spriteBatch.Draw(_vignette!, _destination, Color.White);
        _spriteBatch.End();
    }

    private void UpdateDestination(LazerArena.Presentation.Layout.ScreenLayout layout)
    {
        int reserved = _platform.Cutout.Reserved;   // SO-7: Aussparung nicht mittig: Bild beginnt darunter
        int vw = GraphicsDevice.Viewport.Width, vh = GraphicsDevice.Viewport.Height - reserved;
        // bildschirmfuellend einpassen; das logische Bild hat bereits das Seitenverhaeltnis des Geraets
        float scale = MathF.Min(vw / (float)layout.Width, vh / (float)layout.Height);
        int w = (int)(layout.Width * scale), h = (int)(layout.Height * scale);
        _destination = new Rectangle((vw - w) / 2, reserved + (vh - h) / 2, w, h);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sound.Dispose();
            _glowTarget?.Dispose();
            _scanlines?.Dispose();
            _vignette?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void SaveScreenshot()
    {
        if (_canvas is null) return;
        Directory.CreateDirectory(_platform.ScreenshotDirectory);
        string path = Path.Combine(_platform.ScreenshotDirectory, $"lazer-arena-{DateTime.Now:yyyyMMdd-HHmmss}.png");
        PngWriter.Write(_canvas, path, scale: 2);
    }
}
