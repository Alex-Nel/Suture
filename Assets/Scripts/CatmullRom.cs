using UnityEngine;
using System.Collections.Generic;

public static class CatmullRom
{
    public static List<Vector3> GenerateSpline(Vector3[] points, int subdivisions)
    {
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 p0 = i == 0 ? points[i] : points[i - 1];
            Vector3 p1 = points[i];
            Vector3 p2 = points[i + 1];
            Vector3 p3 = (i + 2 < points.Length) ? points[i + 2] : p2;

            for (int j = 0; j <= subdivisions; j++)
            {
                float t = j / (float)subdivisions;
                result.Add(CatmullRomPosition(t, p0, p1, p2, p3));
            }
        }
        return result;
    }

    private static Vector3 CatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }
}

