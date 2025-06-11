using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner
{
    private SpaceGraphGenerator graphGenerator;
    private GameObject enemyPrefab;
    private GameObject spaceship;

    public EnemySpawner(SpaceGraphGenerator graphGenerator)
    {
        this.graphGenerator = graphGenerator;
        this.enemyPrefab = graphGenerator.enemyPrefab;
        this.spaceship = GameObject.Find("PlayerSpaceship");
    }


    public void SpawnEnemyAtDistance()
    {
        if (spaceship == null || enemyPrefab == null || graphGenerator == null)
        {
            Debug.LogError("EnemySpawner: Referências não atribuídas corretamente.");
            return;
        }

        var mover = spaceship.GetComponent<SpaceshipMover>();
        if (mover == null || mover.currentPlanet == null)
        {
            Debug.LogError("EnemySpawner: SpaceshipMover ou planeta atual não encontrado.");
            return;
        }

        PlanetNode playerPlanet = mover.currentPlanet;
        List<PlanetNode> candidates = GetPlanetsAtDistance(playerPlanet, 2);

        if (candidates.Count == 0)
        {
            Debug.LogWarning("EnemySpawner: Nenhum planeta encontrado a 2 ou 3 de distância.");
            return;
        }

        PlanetNode spawnPlanet = candidates[Random.Range(0, candidates.Count)];
        GameObject enemy = Object.Instantiate(enemyPrefab, spawnPlanet.position, Quaternion.identity);
        var ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.target = spaceship.transform;
            ai.currentPlanet = spawnPlanet;
        }
    }

  
    private List<PlanetNode> GetPlanetsAtDistance(PlanetNode start, int minDist, int maxDist)
    {
        List<PlanetNode> result = new List<PlanetNode>();
        Queue<(PlanetNode node, int depth)> queue = new Queue<(PlanetNode, int)>();
        HashSet<PlanetNode> visited = new HashSet<PlanetNode>();

        queue.Enqueue((start, 0));
        visited.Add(start);

        while (queue.Count > 0)
        {
            var (node, depth) = queue.Dequeue();

            if (depth >= minDist && depth <= maxDist)
                result.Add(node);

            if (depth < maxDist)
            {
                foreach (var neighbor in node.neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, depth + 1));
                    }
                }
            }
        }

        result.Remove(start);
        return result;
    }
}