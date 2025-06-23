using UnityEngine;

public class CircleObject
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float Radius { get; set; }
    public float Mass { get; set; }
    public bool IsStriker { get; set; }
    public CoinType Type { get; set; }

    public CircleObject(Vector2 position, float radius, float mass, bool isStriker, CoinType type = CoinType.None)
    {
        Position = position;
        Velocity = Vector2.zero;
        Radius = radius;
        Mass = mass;
        IsStriker = isStriker;
        Type = type;
    }
} 