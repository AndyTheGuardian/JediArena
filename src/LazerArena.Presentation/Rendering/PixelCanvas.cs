using System.Numerics;

namespace LazerArena.Presentation.Rendering;

/// <summary>RGB-Farbe mit Kanaelen 0..255 als Gleitkomma (fuer additives Leuchten).</summary>
public readonly record struct Rgb(float R, float G, float B)
{
    public static Rgb Black => new(0, 0, 0);
    public static Rgb FromBytes(int r, int g, int b) => new(r, g, b);
    public Rgb Scale(float f) => new(R * f, G * f, B * f);
    public Rgb Add(int amount) => new(MathF.Min(255, R + amount), MathF.Min(255, G + amount), MathF.Min(255, B + amount));
    public static Rgb Lerp(Rgb a, Rgb b, float t) => new(a.R + (b.R - a.R) * t, a.G + (b.G - a.G) * t, a.B + (b.B - a.B) * t);
}

/// <summary>
/// Pixelflaeche in logischer Aufloesung mit einfachen, pixelgenauen Zeichenfunktionen (ohne Kantenglaettung)
/// und additivem Leuchten. Plattformunabhaengig, damit die Darstellung testbar ist.
/// </summary>
public sealed class PixelCanvas
{
    private readonly Rgb[] _pixels;

    public PixelCanvas(int width, int height)
    {
        Width = width;
        Height = height;
        _pixels = new Rgb[width * height];
    }

    public int Width { get; }
    public int Height { get; }

    public Rgb this[int x, int y]
    {
        get => _pixels[y * Width + x];
        set => _pixels[y * Width + x] = value;
    }

    public void Set(int x, int y, Rgb color)
    {
        if ((uint)x < (uint)Width && (uint)y < (uint)Height) _pixels[y * Width + x] = color;
    }

    public void AddLight(int x, int y, Rgb color, float intensity)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height || intensity <= 0.002f) return;
        ref var p = ref _pixels[y * Width + x];
        p = new Rgb(MathF.Min(255, p.R + color.R * intensity), MathF.Min(255, p.G + color.G * intensity),
            MathF.Min(255, p.B + color.B * intensity));
    }

    public void Clear(Rgb color) => Array.Fill(_pixels, color);

    public void CopyFrom(PixelCanvas other) => Array.Copy(other._pixels, _pixels, _pixels.Length);

    public void FillRect(int x0, int y0, int x1, int y1, Rgb color)
    {
        for (int y = Math.Max(0, y0); y <= Math.Min(Height - 1, y1); y++)
            for (int x = Math.Max(0, x0); x <= Math.Min(Width - 1, x1); x++)
                _pixels[y * Width + x] = color;
    }

    public void RectOutline(int x0, int y0, int x1, int y1, Rgb color)
    {
        FillRect(x0, y0, x1, y0, color);
        FillRect(x0, y1, x1, y1, color);
        FillRect(x0, y0, x0, y1, color);
        FillRect(x1, y0, x1, y1, color);
    }

    /// <summary>Rechteck mit abgeschraegten Ecken (Knoepfe im modernen Stil).</summary>
    public void RoundedRect(int x0, int y0, int x1, int y1, int radius, Rgb? fill, Rgb? outline)
    {
        for (int y = y0; y <= y1; y++)
        {
            int inset = CornerInset(y - y0, y1 - y, radius);
            if (fill is { } f) FillRect(x0 + inset, y, x1 - inset, y, f);
            if (outline is { } o)
            {
                Set(x0 + inset, y, o);
                Set(x1 - inset, y, o);
                if (y == y0 || y == y1) FillRect(x0 + inset, y, x1 - inset, y, o);
                else
                {
                    int prev = CornerInset(y - 1 - y0, y1 - y + 1, radius);
                    if (prev > inset) { FillRect(x0 + inset, y, x0 + prev, y, o); FillRect(x1 - prev, y, x1 - inset, y, o); }
                }
            }
        }
    }

    private static int CornerInset(int fromTop, int fromBottom, int radius)
    {
        int d = Math.Min(fromTop, fromBottom);
        if (d >= radius) return 0;
        float dy = radius - d - 0.5f;
        return (int)MathF.Round(radius - MathF.Sqrt(MathF.Max(0, radius * radius - dy * dy)));
    }

    /// <summary>Linie; Breite 1 = Bresenham, sonst Abstandstest zur Strecke.</summary>
    public void Line(float x0, float y0, float x1, float y1, Rgb color, float width = 1)
    {
        if (width <= 1)
        {
            Bresenham((int)MathF.Round(x0), (int)MathF.Round(y0), (int)MathF.Round(x1), (int)MathF.Round(y1), color);
            return;
        }
        float half = width / 2;
        ForEachNear(x0, y0, x1, y1, half, (x, y, d) => { if (d <= half) Set(x, y, color); });
    }

    public void Polyline(IReadOnlyList<Vector2> points, Rgb color, float width = 1)
    {
        for (int i = 1; i < points.Count; i++)
            Line(points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y, color, width);
    }

    private void Bresenham(int x0, int y0, int x1, int y1, Rgb color)
    {
        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;
        while (true)
        {
            Set(x0, y0, color);
            if (x0 == x1 && y0 == y1) return;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    public void FillEllipse(float cx, float cy, float rx, float ry, Rgb color)
    {
        for (int y = (int)MathF.Floor(cy - ry); y <= (int)MathF.Ceiling(cy + ry); y++)
            for (int x = (int)MathF.Floor(cx - rx); x <= (int)MathF.Ceiling(cx + rx); x++)
            {
                float nx = (x - cx) / rx, ny = (y - cy) / ry;
                if (nx * nx + ny * ny <= 1) Set(x, y, color);
            }
    }

    public void EllipseOutline(float cx, float cy, float rx, float ry, Rgb color, float thickness = 1)
    {
        for (int y = (int)MathF.Floor(cy - ry - 1); y <= (int)MathF.Ceiling(cy + ry + 1); y++)
            for (int x = (int)MathF.Floor(cx - rx - 1); x <= (int)MathF.Ceiling(cx + rx + 1); x++)
            {
                float nx = (x - cx) / rx, ny = (y - cy) / ry;
                float d = MathF.Sqrt(nx * nx + ny * ny);
                if (MathF.Abs(d - 1) * MathF.Min(rx, ry) <= thickness / 2) Set(x, y, color);
            }
    }

    /// <summary>Beleuchtete Kugel/Ellipse in Draufsicht (Licht von links oben).</summary>
    public void ShadedEllipse(float cx, float cy, float rx, float ry, Rgb color)
    {
        const float lx = -0.5f, ly = -0.6f;
        for (int y = (int)(cy - ry); y <= (int)(cy + ry) + 1; y++)
            for (int x = (int)(cx - rx); x <= (int)(cx + rx) + 1; x++)
            {
                float nx = (x - cx) / rx, ny = (y - cy) / ry;
                float dd = nx * nx + ny * ny;
                if (dd > 1) continue;
                float nz = MathF.Sqrt(1 - dd);
                float lum = 0.35f + 0.65f * MathF.Max(0, -(nx * lx + ny * ly) * 0.8f + nz * 0.6f);
                float spec = nx < -0.1f && ny < -0.1f ? MathF.Max(0, nz - 0.85f) * 4 : 0;
                Set(x, y, new Rgb(MathF.Min(255, color.R * lum + 127 * spec), MathF.Min(255, color.G * lum + 127 * spec),
                    MathF.Min(255, color.B * lum + 127 * spec)));
            }
    }

    // ------------------------------------------------------------------ SNES-Bausteine

    /// <summary>Transparentes Ueberblenden (wie die Farbmathematik des SNES).</summary>
    public void Blend(int x, int y, Rgb color, float alpha)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height || alpha <= 0) return;
        ref var p = ref _pixels[y * Width + x];
        p = new Rgb(p.R + (color.R - p.R) * alpha, p.G + (color.G - p.G) * alpha, p.B + (color.B - p.B) * alpha);
    }

    public void BlendRect(int x0, int y0, int x1, int y1, Rgb color, float alpha)
    {
        for (int y = Math.Max(0, y0); y <= Math.Min(Height - 1, y1); y++)
            for (int x = Math.Max(0, x0); x <= Math.Min(Width - 1, x1); x++)
                Blend(x, y, color, alpha);
    }

    /// <summary>
    /// Breiter, transparenter Linienzug. Jedes Pixel wird nur einmal ueberblendet (auch an den Knicken),
    /// damit die Transparenz gleichmaessig bleibt.
    /// </summary>
    public void BlendPolyline(IReadOnlyList<Vector2> points, Rgb color, float width, float alpha)
    {
        float half = width / 2;
        int minX = Math.Max(0, (int)(points.Min(p => p.X) - half - 1)), maxX = Math.Min(Width - 1, (int)(points.Max(p => p.X) + half + 1));
        int minY = Math.Max(0, (int)(points.Min(p => p.Y) - half - 1)), maxY = Math.Min(Height - 1, (int)(points.Max(p => p.Y) + half + 1));
        if (minX > maxX || minY > maxY) return;
        int bw = maxX - minX + 1;
        var covered = new bool[bw * (maxY - minY + 1)];
        for (int i = 1; i < points.Count; i++)
            ForEachNear(points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y, half, (x, y, d) =>
            {
                if (d > half) return;
                int k = (y - minY) * bw + (x - minX);
                if ((uint)k < (uint)covered.Length) covered[k] = true;
            });
        for (int k = 0; k < covered.Length; k++)
            if (covered[k]) Blend(minX + k % bw, minY + k / bw, color, alpha);
    }

    /// <summary>Kugel/Ellipse mit harten Schattierungsstufen (ramp: dunkel..hell) und Glanzpunkt.</summary>
    public void PosterizedEllipse(float cx, float cy, float rx, float ry, IReadOnlyList<Rgb> ramp)
    {
        for (int y = (int)(cy - ry); y <= (int)(cy + ry) + 1; y++)
            for (int x = (int)(cx - rx); x <= (int)(cx + rx) + 1; x++)
            {
                float nx = (x - cx) / rx, ny = (y - cy) / ry, dd = nx * nx + ny * ny;
                if (dd > 1) continue;
                float nz = MathF.Sqrt(1 - dd);
                float lum = MathF.Max(0, (-0.5f * nx - 0.6f * ny) * 0.8f + nz * 0.6f);
                int idx = Math.Min(ramp.Count - 1, (int)(lum * ramp.Count));
                if (nx < -0.25f && ny < -0.3f && dd < 0.35f && dd > 0.12f) idx = ramp.Count - 1;
                Set(x, y, ramp[idx]);
            }
    }

    /// <summary>Senkrechter Verlauf in wenigen Stufen (wie zeilenweise Farbwechsel beim SNES, ohne Dithering).</summary>
    public void BandedGradient(int x0, int y0, int x1, int y1, Rgb top, Rgb bottom, int bands)
    {
        for (int y = y0; y <= y1; y++)
        {
            float t = (y - y0) / (float)Math.Max(1, y1 - y0);
            float q = MathF.Floor(t * bands) / Math.Max(1, bands - 1);
            FillRect(x0, y, x1, y, Rgb.Lerp(top, bottom, MathF.Min(1, q)));
        }
    }

    /// <summary>Alle Pixel einer Zeile abdunkeln (Bildzeilen eines Roehrenfernsehers).</summary>
    public void DarkenRow(int y, float factor)
    {
        if ((uint)y >= (uint)Height) return;
        for (int x = 0; x < Width; x++)
        {
            ref var p = ref _pixels[y * Width + x];
            p = p.Scale(factor);
        }
    }

    /// <summary>Gefuelltes Vieleck (Scanline, gerade-ungerade-Regel).</summary>
    public void FillPolygon(IReadOnlyList<Vector2> points, Rgb color)
    {
        int minY = (int)MathF.Floor(points.Min(p => p.Y)), maxY = (int)MathF.Ceiling(points.Max(p => p.Y));
        var crossings = new List<float>();
        for (int y = minY; y <= maxY; y++)
        {
            float sy = y + 0.5f;
            crossings.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                var a = points[i];
                var b = points[(i + 1) % points.Count];
                if ((a.Y <= sy && b.Y > sy) || (b.Y <= sy && a.Y > sy))
                    crossings.Add(a.X + (sy - a.Y) / (b.Y - a.Y) * (b.X - a.X));
            }
            crossings.Sort();
            for (int k = 0; k + 1 < crossings.Count; k += 2)
                FillRect((int)MathF.Ceiling(crossings[k] - 0.5f), y, (int)MathF.Floor(crossings[k + 1] - 0.5f), y, color);
        }
    }

    /// <summary>Fuenfzackiger Stern.</summary>
    public void FillStar(float cx, float cy, float outer, float inner, Rgb color, float rotation = 0)
    {
        var points = new List<Vector2>();
        for (int i = 0; i < 10; i++)
        {
            float r = i % 2 == 0 ? outer : inner;
            float a = rotation - MathF.PI / 2 + i * MathF.PI / 5;
            points.Add(new Vector2(cx + r * MathF.Cos(a), cy + r * MathF.Sin(a)));
        }
        FillPolygon(points, color);
    }

    /// <summary>Additives Leuchten um eine Strecke (Gauss-Abfall mit Radius <paramref name="radius"/>).</summary>
    public void GlowLine(float x0, float y0, float x1, float y1, Rgb color, float coreWidth, float radius, float strength)
    {
        float reach = coreWidth / 2 + radius * 2;
        ForEachNear(x0, y0, x1, y1, reach, (x, y, d) =>
        {
            float outside = MathF.Max(0, d - coreWidth / 2);
            AddLight(x, y, color, strength * 0.55f * MathF.Exp(-(outside * outside) / (radius * radius)));
        });
    }

    public void GlowPolyline(IReadOnlyList<Vector2> points, Rgb color, float coreWidth, float radius, float strength)
    {
        for (int i = 1; i < points.Count; i++)
            GlowLine(points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y, color, coreWidth, radius, strength);
    }

    public void GlowCircle(float cx, float cy, float r, Rgb color, float radius, float strength) =>
        GlowLine(cx, cy, cx, cy, color, 2 * r, radius, strength);

    public void GlowRect(int x0, int y0, int x1, int y1, Rgb color, float radius, float strength)
    {
        int reach = (int)MathF.Ceiling(radius * 2);
        for (int y = y0 - reach; y <= y1 + reach; y++)
            for (int x = x0 - reach; x <= x1 + reach; x++)
            {
                float dx = MathF.Max(0, MathF.Max(x0 - x, x - x1));
                float dy = MathF.Max(0, MathF.Max(y0 - y, y - y1));
                float d = MathF.Sqrt(dx * dx + dy * dy);
                AddLight(x, y, color, strength * 0.55f * MathF.Exp(-(d * d) / (radius * radius)));
            }
    }

    /// <summary>Leuchtender Ring (z. B. Drohne im Gewitter).</summary>
    public void GlowRing(float cx, float cy, float r, Rgb color, float radius, float strength)
    {
        int reach = (int)MathF.Ceiling(r + radius * 2);
        for (int y = (int)cy - reach; y <= (int)cy + reach; y++)
            for (int x = (int)cx - reach; x <= (int)cx + reach; x++)
            {
                float d = MathF.Abs(MathF.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) - r);
                AddLight(x, y, color, strength * 0.55f * MathF.Exp(-(d * d) / (radius * radius)));
            }
    }

    private void ForEachNear(float x0, float y0, float x1, float y1, float reach, Action<int, int, float> action)
    {
        int minX = (int)MathF.Floor(MathF.Min(x0, x1) - reach), maxX = (int)MathF.Ceiling(MathF.Max(x0, x1) + reach);
        int minY = (int)MathF.Floor(MathF.Min(y0, y1) - reach), maxY = (int)MathF.Ceiling(MathF.Max(y0, y1) + reach);
        minX = Math.Max(0, minX); minY = Math.Max(0, minY);
        maxX = Math.Min(Width - 1, maxX); maxY = Math.Min(Height - 1, maxY);
        float vx = x1 - x0, vy = y1 - y0, len2 = vx * vx + vy * vy;
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                float t = len2 > 0 ? Math.Clamp(((x - x0) * vx + (y - y0) * vy) / len2, 0, 1) : 0;
                float px = x0 + vx * t - x, py = y0 + vy * t - y;
                action(x, y, MathF.Sqrt(px * px + py * py));
            }
    }

    /// <summary>Kopiert eine andere Flaeche hinein, optional um 180° gedreht (Anzeigen fuer Spieler 2).</summary>
    public void Blit(PixelCanvas source, int dx, int dy, bool rotate180 = false)
    {
        for (int y = 0; y < source.Height; y++)
            for (int x = 0; x < source.Width; x++)
            {
                var c = rotate180 ? source[source.Width - 1 - x, source.Height - 1 - y] : source[x, y];
                Set(dx + x, dy + y, c);
            }
    }

    /// <summary>Ausgabe als 32-Bit-RGBA (fuer Textur oder PNG).</summary>
    public void CopyTo(Span<uint> rgba)
    {
        for (int i = 0; i < _pixels.Length; i++)
        {
            var p = _pixels[i];
            rgba[i] = 0xFF000000u | ((uint)Math.Clamp(p.B, 0, 255) << 16) | ((uint)Math.Clamp(p.G, 0, 255) << 8) | (uint)Math.Clamp(p.R, 0, 255);
        }
    }

    public int CountColors()
    {
        var set = new HashSet<(int, int, int)>();
        foreach (var p in _pixels) set.Add(((int)p.R, (int)p.G, (int)p.B));
        return set.Count;
    }

    internal Span<Rgb> Pixels => _pixels;
}
