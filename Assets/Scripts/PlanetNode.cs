using System.Collections.Generic;
using UnityEngine;
public class PlanetNode
{
    public int id;
    public Vector2 position;
    public GameObject planetObject;

    [System.NonSerialized]
    public List<PlanetNode> neighbors;

    public PlanetNode(int id, Vector2 position, GameObject planetObject)
{
    this.id = id;
    this.position = position;
    this.planetObject = planetObject;
    this.neighbors = new List<PlanetNode>();
}

    public void Connect(PlanetNode other)
    {
        if (other != null && other != this && !neighbors.Contains(other))
        {
            neighbors.Add(other);
            other.neighbors.Add(this);
        }
    }

    public bool IsValid()
    {
        return planetObject != null && float.IsFinite(position.x) && float.IsFinite(position.y);
    }

    public void DisconnectAll()
    {
        if (neighbors == null) return;

        foreach (var neighbor in neighbors)
        {
            if (neighbor != null && neighbor.neighbors != null)
            {
                neighbor.neighbors.Remove(this);
            }
        }
        neighbors.Clear();
    }
    public bool IsConnectedTo(PlanetNode other)
    {
        return neighbors != null && neighbors.Contains(other);
    }


}