using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class PathHelper : MonoBehaviour 
{
    [SerializeField] LineRenderer line;
    [SerializeField] List<Transform> waypoints = new();

    public List<Vector2> Path => waypoints.Select(wp => (Vector2)wp.position).ToList();
    public Vector2 FirstPoint => waypoints.FirstOrDefault()?.position ?? Vector2.zero;
    public int WaypointCount => waypoints.Count;

    private void Awake()
    {
        UpdateWaypoints();
        DrawPath();
    }

    private void OnValidate()
    {
        UpdateWaypoints();
        DrawPath();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DrawPath(); 
        }
#endif
    }

    public void UpdateWaypoints()
    {
        waypoints.Clear();
        foreach (Transform child in transform)
        {
            waypoints.Add(child);
        }
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
