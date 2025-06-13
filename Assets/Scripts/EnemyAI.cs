using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SearchAlgorithmType
{
    Greedy,
    AStar
}


public class EnemyAI : MonoBehaviour
{

    public SearchAlgorithmType algorithmType;
    public PlanetNode currentPlanet;
    private PlanetNode targetPlanet;
    public float speed = 60f;

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

                    // Checa colisão defensiva com o player
                    var player = FindObjectOfType<SpaceshipMover>();
                    if (player != null && player.currentPlanet == currentPlanet && !player.isDead)
                    {
                        if (player.currentBullets > 0)
                        {
                            player.currentBullets--;
                            player.KillEnemy(this);
                        }
                        else
                        {
                            player.Die();
                        }
                    }
                }
            }
        }
    }

    void CalculatePathToTarget()
    {
        path.Clear();
        if (currentPlanet == null || targetPlanet == null) return;

        PriorityQueue<PlanetNode> frontier = new PriorityQueue<PlanetNode>();
        frontier.Enqueue(currentPlanet, 0);

        Dictionary<PlanetNode, PlanetNode> cameFrom = new Dictionary<PlanetNode, PlanetNode>();
        Dictionary<PlanetNode, float> costSoFar = new Dictionary<PlanetNode, float>();

        cameFrom[currentPlanet] = null;
        costSoFar[currentPlanet] = 0;

        while (!frontier.IsEmpty)
        {
            PlanetNode current = frontier.Dequeue();

            if (current == targetPlanet)
                break;

            foreach (PlanetNode neighbor in current.neighbors)
            {
                float newCost = costSoFar[current] + Vector2.Distance(current.position, neighbor.position);

                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;

                    float priority;
                    if (algorithmType == SearchAlgorithmType.Greedy)
                    {
                        priority = Heuristic(neighbor, targetPlanet);
                    }
                    else
                    {
                        priority = newCost + Heuristic(neighbor, targetPlanet);
                    }

                    frontier.Enqueue(neighbor, priority);
                    cameFrom[neighbor] = current;
                }
            }
        }

        // A reconstrução do caminho continua exatamente a mesma...
        PlanetNode node = targetPlanet;
        Stack<PlanetNode> reversePath = new Stack<PlanetNode>();
        if (cameFrom.ContainsKey(node))
        {
            while (node != currentPlanet)
            {
                reversePath.Push(node);
                node = cameFrom[node];
                if (node == null) break;
            }
        }
        path.Clear();
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

    public void KillSelf()
    {
        Destroy(gameObject);
    }
}