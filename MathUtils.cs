using UnityEngine;

public static class MathUtils
{
    public static float Remap(float value, Vector2 fromRange, Vector2 toRange)
    {
        float t = Mathf.InverseLerp(fromRange[0], fromRange[1], value);
        return Mathf.Lerp(toRange[0], toRange[1], t);
    }

    public static float Wedge(Vector2 a, Vector2 b)
    {
        a.Normalize();
        b.Normalize();
        return (a.x*b.y) - (a.y*b.x);
    }
}
