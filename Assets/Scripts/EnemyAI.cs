using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public PlanetNode currentPlanet;
    private PlanetNode targetPlanet;
    public float speed = 1000f;

    private PlanetNode nextNode;

    private Queue<PlanetNode> path = new Queue<PlanetNode>();

    public Transform target;

    void Start()
    {
        StartCoroutine(UpdatePathRoutine());

        if (target != null)
        {
            SpaceshipMover mover = target.GetComponent<SpaceshipMover>();
            if (mover != null)
            {
                targetPlanet = mover.currentPlanet;
                CalculatePathToTarget();
            }
        }
    }

    void Update()
    {
        if (path != null && (nextNode != null || path.Count > 0))
        {
            if (nextNode == null && path.Count > 0)
            {
                nextNode = path.Dequeue();
            }

            if (nextNode != null)
            {
                Vector2 direction = (nextNode.position - (Vector2)transform.position).normalized;
                transform.position = Vector2.MoveTowards(transform.position, nextNode.position, speed * Time.deltaTime);

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle - 0f); 

                if (Vector2.Distance(transform.position, nextNode.position) < 0.05f)
                {
                    currentPlanet = nextNode;
                    nextNode = null;
                }
            }
        }
    }

    void CalculatePathToTarget()
    {
        path.Clear();
        HashSet<PlanetNode> visited = new HashSet<PlanetNode>();
        PriorityQueue<PlanetNode> frontier = new PriorityQueue<PlanetNode>();
        Dictionary<PlanetNode, PlanetNode> cameFrom = new Dictionary<PlanetNode, PlanetNode>();

        frontier.Enqueue(currentPlanet, Heuristic(currentPlanet, targetPlanet));
        visited.Add(currentPlanet);

        while (!frontier.IsEmpty)
        {
            PlanetNode current = frontier.Dequeue();

            if (current == targetPlanet)
                break;

            foreach (PlanetNode neighbor in current.neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    float priority = Heuristic(neighbor, targetPlanet);
                    frontier.Enqueue(neighbor, priority);
                }
            }
        }

        PlanetNode node = targetPlanet;
        Stack<PlanetNode> reversePath = new Stack<PlanetNode>();

        while (node != currentPlanet && cameFrom.ContainsKey(node))
        {
            reversePath.Push(node);
            node = cameFrom[node];
        }

        while (reversePath.Count > 0)
        {
            path.Enqueue(reversePath.Pop());
        }
    }

    float Heuristic(PlanetNode a, PlanetNode b)
    {
        return Vector2.Distance(a.position, b.position);
    }

    public void UpdateTargetPlanet(PlanetNode newTarget)
    {
        targetPlanet = newTarget;
        CalculatePathToTarget();
    }
    IEnumerator UpdatePathRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            if (targetPlanet != null)
            {
                UpdateTargetPlanet(targetPlanet);
            }
        }
    }
}