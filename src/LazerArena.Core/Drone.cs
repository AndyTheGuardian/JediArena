using System.Numerics;

namespace LazerArena.Core;

/// <summary>Sentinel-Drohne: bewegt sich waagerecht, wechselt zufaellig Richtung und Tempo (SF-2).</summary>
public sealed class Drone(GameConfig config)
{
    private float _changeTimer;

    public float X { get; private set; } = config.FieldWidth / 2;
    public float Y { get; } = config.DroneYRatio * config.FieldHeight;
    public float VelocityX { get; private set; }
    public Vector2 Position => new(X, Y);

    public void Reset(IRandomSource random)
    {
        X = config.FieldWidth / 2;
        PickNewMovement(random);
    }

    public void Update(float dt, IRandomSource random)
    {
        _changeTimer -= dt;
        if (_changeTimer <= 0)
            PickNewMovement(random);

        X += VelocityX * dt;
        float min = config.DroneRadius, max = config.FieldWidth - config.DroneRadius;
        if (X < min) { X = 2 * min - X; VelocityX = MathF.Abs(VelocityX); }
        if (X > max) { X = 2 * max - X; VelocityX = -MathF.Abs(VelocityX); }
    }

    private void PickNewMovement(IRandomSource random)
    {
        float speed = random.Range(config.DroneSpeedMin, config.DroneSpeedMax);
        VelocityX = random.Chance(0.5f) ? speed : -speed;
        _changeTimer = random.Range(config.DroneChangeMin, config.DroneChangeMax);
    }
}
