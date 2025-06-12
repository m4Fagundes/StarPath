using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipMover : MonoBehaviour
{
    public PlanetNode currentPlanet;
    public PlanetNode previousPlanet;
    public PlanetNode targetPlanet;
    public float travelSpeed = 30f;
    public int planetVisitCount = 0;

    private bool isMoving = false;

    public Vector2 TravelDirection =>
        (currentPlanet != null && previousPlanet != null)
            ? (currentPlanet.position - previousPlanet.position).normalized
            : Vector2.right;

    public Vector2 IntendedTravelDirection { get; private set; } = Vector2.zero;

    public float maxFuel = 100f;
    public float currentFuel = 100f;

    public int maxBullets = 3;
    public int currentBullets = 1;
    public bool isDead = false;

    public int score = 0;
    public int highScore = 0;

    // Retorna o custo de gasolina para um planeta com base na cor
    public int GetFuelCost(PlanetNode planet)
    {
        if (planet == null || planet.planetObject == null) return 0;
        var renderer = planet.planetObject.GetComponent<SpriteRenderer>();
        if (renderer == null || renderer.sprite == null) return 0;
        string spriteName = renderer.sprite.name.ToLower();
        if (spriteName.Contains("verde")) return 3;
        if (spriteName.Contains("amarelo")) return 5;
        if (spriteName.Contains("vermelho")) return 8;
        return 5; // padrão caso não encontre
    }

    public bool HasEnoughFuel(PlanetNode destination)
    {
        return currentFuel >= GetFuelCost(destination);
    }

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void AddScore(int amount)
    {
        score += amount;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
    }

    public void TravelTo(PlanetNode destination)
    {
        if (isMoving || currentPlanet == null || destination == null || currentPlanet.neighbors == null)
        {
            Debug.LogWarning("Tentativa de viagem com referência nula.");
            return;
        }

        if (!currentPlanet.neighbors.Contains(destination))
        {
            Debug.LogWarning("Destino não é vizinho do planeta atual.");
            return;
        }

        int fuelCost = GetFuelCost(destination);
        if (currentFuel < fuelCost || currentFuel <= 0)
        {
            Debug.LogWarning("Sem gasolina suficiente para viajar!");
            currentFuel = Mathf.Max(0, currentFuel); // nunca negativo
            return;
        }

        // Score por planeta
        int scoreToAdd = 1;
        if (destination != null && destination.planetObject != null)
        {
            var renderer = destination.planetObject.GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.sprite != null)
            {
                string spriteName = renderer.sprite.name.ToLower();
                if (spriteName.Contains("verde")) scoreToAdd = 1;
                else if (spriteName.Contains("amarelo")) scoreToAdd = 2;
                else if (spriteName.Contains("vermelho")) scoreToAdd = 3;
            }
        }
        AddScore(scoreToAdd);

        IntendedTravelDirection = (destination.position - currentPlanet.position).normalized;
        currentFuel -= fuelCost;
        if (currentFuel < 0) currentFuel = 0;
        StartCoroutine(MoveToPlanet(destination));
    }

    // Chame este método para adicionar uma bala (ex: powerup)
    public void AddBullet(int amount = 1)
    {
        currentBullets = Mathf.Min(currentBullets + amount, maxBullets);
    }

    // Chame este método quando o jogador "morrer"
    public void Die()
    {
        isDead = true;
        Debug.Log("O jogador foi destruído!");
        // Mostra tela de game over
        var goUI = FindObjectOfType<GameOverUI>();
        if (goUI == null)
        {
            var goObj = new GameObject("GameOverUI");
            goUI = goObj.AddComponent<GameOverUI>();
        }
        goUI.Show();
        gameObject.SetActive(false);
    }

    // Chame este método quando um inimigo for morto
    public void KillEnemy(EnemyAI enemy)
    {
        if (enemy != null)
        {
            Destroy(enemy.gameObject);
            Debug.Log("Inimigo destruído!");
            AddScore(30);
            var generator = Object.FindFirstObjectByType<SpaceGraphGenerator>();
            if (generator != null)
            {
                generator.TrySpawnEnemy();
            }
        }
    }

    // Verifica se há inimigos no mesmo planeta após chegar
    void CheckCombatOnPlanet()
    {
        if (isDead) return;
        EnemyAI[] enemies = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        var toKill = new List<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            if (enemy.currentPlanet == currentPlanet)
            {
                toKill.Add(enemy);
            }
        }
        foreach (EnemyAI enemy in toKill)
        {
            if (isDead) break;
            if (currentBullets > 0)
            {
                currentBullets--;
                KillEnemy(enemy);
            }
            else
            {
                Die();
                break;
            }
        }
    }

    void OnArriveAtPlanet(PlanetNode planet)
    {
        currentPlanet = planet;
        var generator = Object.FindFirstObjectByType<SpaceGraphGenerator>();
        if (generator != null)
        {
            generator.RegisterPlanetVisit(planet);
        }
        CheckCombatOnPlanet();
    }

    IEnumerator MoveToPlanet(PlanetNode destination)
    {
        isMoving = true;
        targetPlanet = destination;

        Vector3 start = transform.position;
        Vector3 end = destination.position;
        float journeyLength = Vector3.Distance(start, end);
        float startTime = Time.time;

        Vector3 direction = (end - start).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        SpaceGraphGenerator generator = Object.FindFirstObjectByType<SpaceGraphGenerator>();
        Vector3 lastGenerationPos = transform.position;

        while (Vector3.Distance(transform.position, end) > 0.05f)
        {
            float distCovered = (Time.time - startTime) * travelSpeed;
            float fraction = distCovered / journeyLength;
            transform.position = Vector3.Lerp(start, end, fraction);

            if (generator != null && Vector3.Distance(transform.position, lastGenerationPos) > generator.generationThreshold)
            {
                generator.GenerateNewArea(transform.position);
                lastGenerationPos = transform.position;
            }

            yield return null;
        }

        transform.position = end;
        previousPlanet = currentPlanet;
        currentPlanet = destination;
        isMoving = false;
        planetVisitCount++;

        OnArriveAtPlanet(currentPlanet);

        EnableNeighbors(currentPlanet);


        EnemyAI[] enemies = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in enemies)
        {
            enemy.UpdateTargetPlanet(currentPlanet);
        }

        if (generator != null)
        {
            generator.GenerateNewArea(destination.position);
        }
    }

    void EnableNeighbors(PlanetNode current)
    {
        if (current == null || current.neighbors == null) return;

        PlanetClick[] allPlanets = Object.FindObjectsByType<PlanetClick>(FindObjectsSortMode.None);

        foreach (PlanetClick pc in allPlanets)
        {
            if (pc == null || pc.node == null) continue;

            bool isNeighbor = current.neighbors.Contains(pc.node);
            pc.SetClicable(isNeighbor);
        }
    }
}
