using System.Collections.Generic;
using UnityEngine;

public class PathHelper : MonoBehaviour 
{
    public LineRenderer line;

    public List<Transform> waypoints = new();

    private void Awake()
    {
        // Collect all child transforms as waypoints
        waypoints.Clear();
        foreach (Transform child in transform)
        {
            waypoints.Add(child);
        }

        DrawPath();
    }

    private void DrawPath()
    {
        if (line == null || waypoints.Count < 2)
            return;

        line.positionCount = waypoints.Count;
        for (int i = 0; i < waypoints.Count; i++)
        {
            line.SetPosition(i, waypoints[i].position);
        }
    }

    public Vector3 GetWaypoint(int index)
    {
        if (index >= 0 && index < waypoints.Count)
            return waypoints[index].position;
        return Vector3.zero;
    }

    public Vector2 GetFirstPoint() => waypoints[0].position;

    public List<Vector2> GetPathPoints()
    {
        List<Vector2> pathPoints = new();

        foreach (Transform point in transform)
        {
            pathPoints.Add(point.position);
        }

        return pathPoints;
    }

    public int GetWaypointCount() => waypoints.Count;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        // Auto-update the list only in editor
        waypoints.Clear();
        foreach (Transform child in transform)
        {
            waypoints.Add(child);
        }

        if (waypoints.Count == 0) return;

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] && waypoints[i + 1])
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (i == 0)
                Gizmos.color = Color.blue; // Start point
            else if (i == waypoints.Count - 1)
                Gizmos.color = Color.red; // End point
            else
                Gizmos.color = Color.white;

            Gizmos.DrawSphere(waypoints[i].position, 0.3f);
        }
    }
}
