using System.Numerics;
using LazerArena.Core;
using LazerArena.Presentation.Layout;

namespace LazerArena.Presentation.Rendering;

/// <summary>Rechnet Spielfeldkoordinaten (Breite 1, Hoehe 1/Seitenverhaeltnis) in logische Pixel um.</summary>
public sealed class FieldMapper(IntRect field, GameConfig config)
{
    public IntRect Field { get; } = field;

    public float ScaleX => Field.Width / config.FieldWidth;
    public float ScaleY => Field.Height / config.FieldHeight;

    public Vector2 ToScreen(Vector2 p) => new(Field.X + p.X * ScaleX, Field.Y + p.Y * ScaleY);

    public (int X0, int Y0, int X1, int Y1) ToScreen(RectF r) =>
        ((int)MathF.Round(Field.X + r.Left * ScaleX), (int)MathF.Round(Field.Y + r.Top * ScaleY),
         (int)MathF.Round(Field.X + r.Right * ScaleX) - 1, (int)MathF.Round(Field.Y + r.Bottom * ScaleY) - 1);
}

public static class Zigzag
{
    /// <summary>Zickzack-Linie von a nach b mit wechselndem seitlichem Ausschlag (BL-8).</summary>
    public static List<Vector2> Between(Vector2 a, Vector2 b, float step, float amplitude, int seed)
    {
        var random = new Random(seed);
        var d = b - a;
        float length = d.Length();
        var result = new List<Vector2> { a };
        if (length < 0.001f) { result.Add(b); return result; }
        var normal = new Vector2(-d.Y, d.X) / length;
        int n = Math.Max(2, (int)(length / step));
        for (int k = 1; k < n; k++)
        {
            float offset = amplitude * (k % 2 == 1 ? 1 : -1) * (0.5f + (float)random.NextDouble() * 0.5f);
            result.Add(a + d * (k / (float)n) + normal * offset);
        }
        result.Add(b);
        return result;
    }

    /// <summary>
    /// Unregelmaessiger Blitz (BL-8): ungleich lange Abschnitte, wechselnd starke Ausschlaege, meist abwechselnd,
    /// ab und zu ein scharfer Knick. Mit neuem Startwert je Bild zuckt der Blitz sichtbar.
    /// </summary>
    public static List<Vector2> Irregular(Vector2 a, Vector2 b, float step, float amplitude, int seed)
    {
        var random = new Random(seed);
        var d = b - a;
        float length = d.Length();
        var result = new List<Vector2> { a };
        if (length < 0.001f) { result.Add(b); return result; }
        var normal = new Vector2(-d.Y, d.X) / length;
        float pos = 0, side = random.Next(2) == 0 ? -1 : 1;
        while (true)
        {
            pos += step * (0.5f + (float)random.NextDouble());   // 0,5 .. 1,5 Abschnittslaenge
            if (pos >= length - step * 0.4f) break;
            side = random.NextDouble() < 0.8 ? -side : side;     // meist abwechselnd, manchmal zweimal dieselbe Seite
            float strength = 0.3f + (float)random.NextDouble() * 1.1f;
            if (random.NextDouble() < 0.12) strength *= 2f;      // gelegentlicher scharfer Knick
            result.Add(a + d * (pos / length) + normal * side * amplitude * strength);
        }
        result.Add(b);
        return result;
    }

    /// <summary>Kurze Seitenaeste, die von zufaelligen Punkten des Blitzes abzweigen.</summary>
    public static List<List<Vector2>> Branches(IReadOnlyList<Vector2> bolt, int count, float length, int seed)
    {
        var random = new Random(seed ^ 0x51ED);
        var branches = new List<List<Vector2>>();
        if (bolt.Count < 4) return branches;
        for (int i = 0; i < count; i++)
        {
            int k = random.Next(1, bolt.Count - 2);
            var dir = Vector2.Normalize(bolt[k + 1] - bolt[k - 1]);
            float angle = (random.NextDouble() < 0.5 ? -1 : 1) * (0.5f + (float)random.NextDouble() * 0.6f);
            var turned = new Vector2(dir.X * MathF.Cos(angle) - dir.Y * MathF.Sin(angle), dir.X * MathF.Sin(angle) + dir.Y * MathF.Cos(angle));
            float len = length * (0.5f + (float)random.NextDouble() * 0.7f);
            branches.Add(Irregular(bolt[k], bolt[k] + turned * len, len / 3, len * 0.15f, seed + i * 131));
        }
        return branches;
    }

    /// <summary>Zickzack entlang eines Pfads mit Knicken.</summary>
    /// <param name="irregular">OP-14: unregelmaessig zuckend (Standard) oder gleichmaessiges Zickzack wie in den ersten Versionen.</param>
    public static List<Vector2> Along(IReadOnlyList<Vector2> path, float step, float amplitude, int seed, bool irregular = true)
    {
        var result = new List<Vector2>();
        for (int i = 1; i < path.Count; i++)
        {
            var part = irregular
                ? Irregular(path[i - 1], path[i], step, amplitude, seed + i * 7919)
                : Between(path[i - 1], path[i], step, amplitude, seed + i * 7919);
            if (result.Count > 0) part.RemoveAt(0);
            result.AddRange(part);
        }
        return result;
    }
}
