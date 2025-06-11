using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpaceGraphGenerator : MonoBehaviour
{
    Animator animator;

    [Header("Graph Settings")]
    public int initialPlanetCount = 60;
    private HashSet<int> visitedPlanetIds = new HashSet<int>();
    public int maxPlanets = 1000;
    public float connectionDistance = 80f; 
    public Vector2 spawnArea = new Vector2(100, 100);
    public float generationThreshold = 10f;
    private int planetVisitCount = 0;

    [Header("Planet Settings")]
    public GameObject planetPrefab;
    public Sprite[] planetSprites;
    public float planetSpacing = 150f;
    public int maxConnectionsPerPlanet = 3;

    [Header("Spaceship Settings")]
    public GameObject spaceshipPrefab;
    public float spaceshipSpeed = 10f;

    private List<PlanetNode> planets = new List<PlanetNode>();
    private GameObject spaceship;
    private Vector2 lastGenerationPosition;

    [Header("Enemy Settings")]
    public GameObject enemyPrefab;

    void Start()
    {
        animator = GetComponent<Animator>();
        SetSpawnAreaToCameraView();
        LoadPlanetSprites();
        GenerateInitialGraph();
        CreateSpaceship();
        if (planets.Count > 1) {
            SpawnEnemyAtPlanet(planets[Random.Range(1, planets.Count)]);
        }
    }

    void SetSpawnAreaToCameraView()
    {
        Camera can = Camera.main;
        float height = 2f * can.orthographicSize;
        float width = height * can.aspect;
        spawnArea = new Vector2(width, height);
    }
    
    public void RegisterPlanetVisit(PlanetNode planet)
    {
        if (!visitedPlanetIds.Contains(planet.id))
        {
            visitedPlanetIds.Add(planet.id);
            planetVisitCount++;

            if (planetVisitCount >= 3)
            {
                planetVisitCount = 0;
                SpawnEnemyAtPlanet(planets[Random.Range(0, planets.Count)]);
            }
        }
    }

    void LoadPlanetSprites()
    {
        planetSprites = new Sprite[3];
        planetSprites[0] = Resources.Load<Sprite>("PlanetaVerde");
        planetSprites[1] = Resources.Load<Sprite>("PlanetaAmarelo");
        planetSprites[2] = Resources.Load<Sprite>("PlanetaVermelho");
    }

    void GenerateInitialGraph()
    {
        planets.Clear();
        lastGenerationPosition = Vector2.zero;

        int attempts = 0;
        int created = 0;
        int maxAttempts = initialPlanetCount * 20;

        float initialSpawnMultiplier = 2.5f;
        float spawnAreaX = spawnArea.x * initialSpawnMultiplier;
        float spawnAreaY = spawnArea.y * initialSpawnMultiplier;

        while (created < initialPlanetCount && attempts < maxAttempts)
        {
            Vector2 randomPos = new Vector2(
                Random.Range(-spawnAreaX / 2, spawnAreaX / 2),
                Random.Range(-spawnAreaY / 2, spawnAreaY / 2)
            );

            if (IsFarEnough(randomPos))
            {
                CreatePlanet(randomPos);
                created++;
            }
            attempts++;
        }

        Debug.Log($"Geração inicial concluída. Planetas criados: {created} (Tentativa era de {initialPlanetCount})");

        ValidateGraph();
        ConnectPlanets();
    }

    public void GenerateNewArea(Vector2 centerPosition)
    {
        if (planets.Count >= maxPlanets) return;

        lastGenerationPosition = centerPosition;

        SpaceshipMover mover = spaceship.GetComponent<SpaceshipMover>();
        Vector2 travelDir = (mover != null) ? mover.IntendedTravelDirection : Vector2.zero;

        if (travelDir == Vector2.zero)
            travelDir = Random.insideUnitCircle.normalized;

        int planetsToGenerate = 25;
        float generationRadius = spawnArea.x * 0.8f;

        int tries = 0;
        int created = 0;

        while (created < planetsToGenerate && tries < planetsToGenerate * 3)
        {
            float angleOffset = Random.Range(-30f, 30f);
            float baseAngle = Mathf.Atan2(travelDir.y, travelDir.x) * Mathf.Rad2Deg;
            float angle = (baseAngle + angleOffset) * Mathf.Deg2Rad;

            float distance = Random.Range(planetSpacing * 1.5f, generationRadius);
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
            Vector2 spawnPos = centerPosition + offset;

            if (IsFarEnough(spawnPos))
            {
                CreatePlanet(spawnPos);
                created++;
            }
            tries++;
        }

        ValidateGraph();
        ConnectPlanets();
    }

    bool IsFarEnough(Vector2 position)
    {
        foreach (var planet in planets)
        {
            if (planet != null && Vector2.Distance(planet.position, position) < planetSpacing)
                return false;
        }
        return true;
    }

    void CreatePlanet(Vector2 position)
    {
        GameObject planetObj = Instantiate(planetPrefab, position, Quaternion.identity, this.transform);
        planetObj.name = $"Planet_{planets.Count}";

        int typeIndex = Random.Range(0, planetSprites.Length);
        var renderer = planetObj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sprite = planetSprites[typeIndex];
        }

        Animator planetAnimator = planetObj.GetComponent<Animator>();
        if (planetAnimator != null)
        {
            switch (typeIndex)
            {
                case 0: planetAnimator.Play("GreenPlanet"); break;
                case 1: planetAnimator.Play("YellowPlanet"); break;
                case 2: planetAnimator.Play("RedPlanet"); break;
            }
        }

        PlanetNode node = new PlanetNode(planets.Count, position, planetObj);
        planetObj.AddComponent<PlanetClick>().node = node;

        if (!planetObj.TryGetComponent(out Collider2D col))
        {
            planetObj.AddComponent<CircleCollider2D>().radius = 1.2f;
        }

        planets.Add(node);
    }

    void CreateSpaceship()
    {
        if (planets.Count == 0) return;

        PlanetNode startNode = planets[0];
        spaceship = Instantiate(spaceshipPrefab, startNode.position, Quaternion.identity);
        spaceship.name = "PlayerSpaceship";

        var mover = spaceship.AddComponent<SpaceshipMover>();
        mover.currentPlanet = startNode;

        Camera.main.GetComponent<CameraFollow>().target = spaceship.transform;
    }

    void ConnectPlanets()
    {
        for (int i = 0; i < planets.Count; i++)
        {
            PlanetNode current = planets[i];
            if (current == null || !current.IsValid()) continue;
            
            if (current.neighbors.Count >= maxConnectionsPerPlanet) continue;

            var potentialConnections = new List<(PlanetNode node, float distance)>();

            for (int j = 0; j < planets.Count; j++)
            {
                if (i == j) continue;
                PlanetNode other = planets[j];
                if (other == null || !other.IsValid()) continue;
                
                if (other.neighbors.Count < maxConnectionsPerPlanet)
                {
                    float distance = Vector2.Distance(current.position, other.position);
                    if (float.IsFinite(distance) && distance <= connectionDistance)
                    {
                        potentialConnections.Add((other, distance));
                    }
                }
            }
            
            potentialConnections = potentialConnections.OrderBy(conn => conn.distance).ToList();

            int connectionsMade = current.neighbors.Count;
            foreach (var connection in potentialConnections)
            {
                if (connectionsMade >= maxConnectionsPerPlanet) break;

                PlanetNode otherNode = connection.node;
                
                if (!current.IsConnectedTo(otherNode))
                {
                    current.Connect(otherNode);
                    DrawConnection(current, otherNode);
                    connectionsMade++;
                }
            }
        }
    }

    void DrawConnection(PlanetNode a, PlanetNode b)
    {
        if (a == null || b == null || !a.IsValid() || !b.IsValid()) return;

        GameObject connectionObj = new GameObject("Connection");
        connectionObj.transform.SetParent(transform);

        LineRenderer line = connectionObj.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, a.position);
        line.SetPosition(1, b.position);
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.material = new Material(Shader.Find("Sprites/Default"))
        {
            color = new Color(1f, 1f, 1f, 0.3f)
        };
        line.sortingOrder = -1;
    }

    void ValidateGraph()
    {
        for (int i = planets.Count - 1; i >= 0; i--)
        {
            if (planets[i] == null || !planets[i].IsValid())
            {
                Debug.LogWarning($"Removing invalid planet at index {i}");
                if (planets[i] != null && planets[i].planetObject != null)
                {
                    Destroy(planets[i].planetObject);
                }
                planets.RemoveAt(i);
            }
        }
    }

    void Update()
    {
        if (spaceship == null) return;

        SpaceshipMover mover = spaceship.GetComponent<SpaceshipMover>();
        if (mover == null) return;
        
        if (!mover.IsMoving && Vector2.Distance(spaceship.transform.position, lastGenerationPosition) > generationThreshold)
        {
            GenerateNewArea(spaceship.transform.position);
            mover.EnableNeighbors(mover.currentPlanet);
        }
    }
    
    void SpawnEnemyAtPlanet(PlanetNode targetPlanet)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab não atribuído!");
            return;
        }

        if (spaceship == null)
        {
            Debug.LogError("Spaceship ainda não foi criado!");
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab, targetPlanet.position, Quaternion.identity);
        var ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.target = spaceship.transform;
            ai.currentPlanet = targetPlanet;
        }
        else
        {
            Debug.LogError("Enemy instanciado, mas sem componente EnemyAI.");
        }
    }

    IEnumerator SpawnEnemiesOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);

            PlanetNode randomPlanet = null;
            if (planets.Count > 1)
                randomPlanet = planets[Random.Range(1, planets.Count)];

            if(randomPlanet != null)
                SpawnEnemyAtPlanet(randomPlanet);
        }
    }
}