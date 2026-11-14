using System.Numerics;

namespace LazerArena.Core;

public sealed class ShieldSegment(int row, int index, RectF bounds)
{
    /// <summary>0 = vordere Reihe (zur Spielfeldmitte), 1 = hintere Reihe.</summary>
    public int Row { get; } = row;
    public int Index { get; } = index;
    public RectF Bounds { get; } = bounds;
    public bool Alive { get; internal set; } = true;
}

/// <summary>
/// Energieschild aus 2 x 16 Segmenten, gerade oder gebogen (SF-3, SC-1, SC-2). Die Trefferflaechen der Segmente
/// schliessen lueckenlos aneinander an; sichtbare Fugen sind reine Darstellung.
/// </summary>
public sealed class Shield
{
    private readonly GameConfig _config;
    private float _regrowTimer;

    public Shield(PlayerId owner, Vector2 playerPosition, GameConfig config, ShieldShape shape)
    {
        Owner = owner;
        _config = config;
        Segments = shape == ShieldShape.Straight
            ? BuildStraight(owner, config)
            : BuildCurved(owner, playerPosition, config);
    }

    public PlayerId Owner { get; }
    public IReadOnlyList<ShieldSegment> Segments { get; }

    public ShieldSegment? SegmentAt(Vector2 point) =>
        Segments.FirstOrDefault(s => s.Alive && s.Bounds.Contains(point));

    internal void Restore()
    {
        foreach (var s in Segments) s.Alive = true;
        _regrowTimer = 0;
    }

    /// <summary>
    /// SC-1: Solange Segmente fehlen, waechst alle 5 s eines nach – vordere Reihe zuerst, darin das der Mitte naechste.
    /// SC-2: Segmente, durch die gerade ein Blitz verlaeuft, werden uebersprungen.
    /// </summary>
    internal ShieldSegment? UpdateRegrowth(float dt, Func<RectF, bool> isBlocked)
    {
        if (Segments.All(s => s.Alive))
        {
            _regrowTimer = 0;
            return null;
        }

        _regrowTimer += dt;
        if (_regrowTimer < _config.ShieldRegrowInterval)
            return null;

        float middle = (_config.ShieldSegments - 1) / 2f;
        var candidate = Segments
            .Where(s => !s.Alive && !isBlocked(s.Bounds))
            .OrderBy(s => s.Row)
            .ThenBy(s => MathF.Abs(s.Index - middle))
            .ThenBy(s => s.Index)
            .FirstOrDefault();
        if (candidate is null)
            return null;

        candidate.Alive = true;
        _regrowTimer -= _config.ShieldRegrowInterval;
        return candidate;
    }

    private static List<ShieldSegment> BuildStraight(PlayerId owner, GameConfig c)
    {
        var result = new List<ShieldSegment>();
        float segW = c.FieldWidth / c.ShieldSegments;
        float rowH = c.ShieldRowHeightRatio * c.FieldHeight;
        for (int row = 0; row < c.ShieldRows; row++)
        {
            float topP1 = c.StraightShieldRowsP1[row] * c.FieldHeight;
            float top = owner == PlayerId.One ? topP1 : c.FieldHeight - topP1 - rowH;
            for (int i = 0; i < c.ShieldSegments; i++)
                result.Add(new ShieldSegment(row, i,
                    new RectF(i * segW, top, (i + 1) * segW, top + rowH)));
        }
        return result;
    }

    private static List<ShieldSegment> BuildCurved(PlayerId owner, Vector2 player, GameConfig c)
    {
        var result = new List<ShieldSegment>();
        float forward = owner == PlayerId.One ? -1f : 1f;
        var center = player - new Vector2(0, forward * c.CurvedCenterBehind * c.FieldWidth);
        float rowH = c.ShieldRowHeightRatio * c.FieldHeight;
        float arc = Angles.ToRadians(c.CurvedArcDegrees);
        for (int row = 0; row < c.ShieldRows; row++)
        {
            float r = c.CurvedRadii[row] * c.FieldWidth;
            float half = r * 2 * arc / c.ShieldSegments / 2;
            for (int i = 0; i < c.ShieldSegments; i++)
            {
                float a = -arc + (i + 0.5f) * 2 * arc / c.ShieldSegments;
                var p = center + new Vector2(r * MathF.Sin(a), forward * r * MathF.Cos(a));
                result.Add(new ShieldSegment(row, i,
                    new RectF(p.X - half, p.Y - rowH / 2, p.X + half, p.Y + rowH / 2)));
            }
        }
        return result;
    }
}
