using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementPath
{
    public Vector3[] Points;
    public float PathLength;
    
    private static List<Vector3> BuildSurfacePath(Vector3[] corners, float stepSize = 0.5f, float navmeshSampleRadius = 1.0f)
    {
        var result = new List<Vector3>();

        for (var i = 0; i < corners.Length - 1; i++)
        {
            Vector3 a = corners[i];
            Vector3 b = corners[i + 1];

            var dist = Vector3.Distance(a, b);
            var steps = Mathf.Max(1, Mathf.CeilToInt(dist / stepSize));

            for (var s = 0; s <= steps; s++)
            {
                var t = s / (float)steps;
                var point = Vector3.Lerp(a, b, t);

                if (NavMesh.SamplePosition(point, out var hit, navmeshSampleRadius, NavMesh.AllAreas))
                    result.Add(hit.position);
            }
        }

        return result;
    }

    private static void RemoveCollinearPoints(List<Vector3> path, float angleThreshold = 3f)
    {
        for (var i = path.Count - 2; i > 0; i--)
        {
            var a = (path[i] - path[i - 1]).normalized;
            var b = (path[i + 1] - path[i]).normalized;

            if (Vector3.Angle(a, b) < angleThreshold)
                path.RemoveAt(i);
        }
    }

    private static List<Vector3> ChaikinSmooth(List<Vector3> input, int iterations)
    {
        var path = new List<Vector3>(input);

        for (var it = 0; it < iterations; it++)
        {
            var newPath = new List<Vector3> { path[0] };

            for (var i = 0; i < path.Count - 1; i++)
            {
                var p0 = path[i];
                var p1 = path[i + 1];

                newPath.Add(Vector3.Lerp(p0, p1, 0.25f));
                newPath.Add(Vector3.Lerp(p0, p1, 0.75f));
            }

            newPath.Add(path[^1]);
            path = newPath;
        }

        return path;
    }

    private static void ProjectPathToNavMesh(List<Vector3> path, float radius = 1f)
    {
        for (var i = 0; i < path.Count; i++)
        {
            if (NavMesh.SamplePosition(path[i], out var hit, radius, NavMesh.AllAreas))
                path[i] = hit.position;
        }
    }

    private static void DrawPath(List<Vector3> path, Color color, float duration)
    {
        for (var i = 0; i < path.Count - 1; i++) 
            Debug.DrawLine(path[i], path[i + 1], color, duration);
    }

    public MovementPath(Vector3[] corners, float stepSize = 0.5f)
    {
        var surfacePath = BuildSurfacePath(corners, stepSize, 5f);
        RemoveCollinearPoints(surfacePath);
        DrawPath(surfacePath, Color.magenta, 25);
        surfacePath = ChaikinSmooth(surfacePath, 2);
        DrawPath(surfacePath, Color.green, 25);
        ProjectPathToNavMesh(surfacePath);
        DrawPath(surfacePath, Color.red, 25);
        
        Points = surfacePath.ToArray();
        for (var i = 0; i < Points.Length - 1; i++)
            PathLength += Vector3.Distance(Points[i], Points[i+1]);
        
        surfacePath.Clear();
    }
}