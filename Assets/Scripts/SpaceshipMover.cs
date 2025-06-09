using System.Collections;
using UnityEngine;

public class SpaceshipMover : MonoBehaviour
{
    public PlanetNode currentPlanet;
    public PlanetNode previousPlanet;
    public PlanetNode targetPlanet;
    public float travelSpeed = 40f;
    public int planetVisitCount = 0;

    private bool isMoving = false;
    
    public bool IsMoving => isMoving;

    public Vector2 TravelDirection =>
        (currentPlanet != null && previousPlanet != null)
            ? (currentPlanet.position - previousPlanet.position).normalized
            : Vector2.right;

    public Vector2 IntendedTravelDirection { get; private set; } = Vector2.zero;

    public void TravelTo(PlanetNode destination)
    {
        if (isMoving || currentPlanet == null || destination == null || !currentPlanet.IsConnectedTo(destination))
        {
            if (currentPlanet != null && !currentPlanet.IsConnectedTo(destination))
            {
                 Debug.LogWarning("Destino não é mais um vizinho válido. O mapa pode ter sido atualizado.");
            }
            return;
        }

        IntendedTravelDirection = (destination.position - currentPlanet.position).normalized;
        StartCoroutine(MoveToPlanet(destination));
    }

    void OnArriveAtPlanet(PlanetNode planet)
    {
        currentPlanet = planet;
        var generator = FindFirstObjectByType<SpaceGraphGenerator>();
        if (generator != null)
        {
            generator.RegisterPlanetVisit(planet);
        }
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

        while (Vector3.Distance(transform.position, end) > 0.05f)
        {
            float distCovered = (Time.time - startTime) * travelSpeed;
            float fraction = distCovered / journeyLength;
            transform.position = Vector3.Lerp(start, end, fraction);
            yield return null;
        }

        
        transform.position = end;
        previousPlanet = currentPlanet;
        currentPlanet = destination;
        isMoving = false;

        OnArriveAtPlanet(currentPlanet);

        var generator = FindFirstObjectByType<SpaceGraphGenerator>();
        if (generator != null)
        {
            generator.GenerateNewArea(destination.position);
        }

        EnableNeighbors(currentPlanet);

        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in enemies)
        {
            enemy.UpdateTargetPlanet(currentPlanet);
        }
    }

    public void EnableNeighbors(PlanetNode current)
    {
        if (current == null || current.neighbors == null) return;

        PlanetClick[] allPlanets = FindObjectsByType<PlanetClick>(FindObjectsSortMode.None);

        foreach (PlanetClick pc in allPlanets)
        {
            if (pc == null || pc.node == null) continue;

            bool isNeighbor = current.neighbors.Contains(pc.node);
            pc.SetClicable(isNeighbor);
        }
    }
}