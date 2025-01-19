namespace Cherris;

public static class ExtensionMethods
{
    public static void Swap<T>(this List<T> list, int index1, int index2)
    {
        (list[index2], list[index1]) = (list[index1], list[index2]);
    }

    public static Vector2 Normalized(this Vector2 vector)
    {
        float length = MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y);

        if (length == 0)
        {
            return new(0, 0);
        }

        return new(vector.X / length, vector.Y / length);
    }

    public static float DistanceTo(this Vector2 vector, Vector2 other)
    {
        // Calculate the Euclidean distance between the two vectors
        return MathF.Sqrt(MathF.Pow(other.X - vector.X, 2) + MathF.Pow(other.Y - vector.Y, 2));
    }

    public static Vector2 MoveToward(this Vector2 current, Vector2 target, float maxDistanceDelta)
    {
        Vector2 direction = target - current;
        float distance = direction.Length();

        if (distance <= maxDistanceDelta || distance == 0)
        {
            return target;
        }

        return current + direction.Normalized() * maxDistanceDelta;
    }
}