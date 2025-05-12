using System.Collections;
using UnityEngine;

public class SpaceshipMover : MonoBehaviour
{
    public PlanetNode currentPlanet;
    public PlanetNode previousPlanet;
    public PlanetNode targetPlanet;
    public float travelSpeed = 20f;

    private bool isMoving = false;

    public Vector2 TravelDirection =>
        (currentPlanet != null && previousPlanet != null)
            ? (currentPlanet.position - previousPlanet.position).normalized
            : Vector2.right;

    public Vector2 IntendedTravelDirection { get; private set; } = Vector2.zero;

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

        IntendedTravelDirection = (destination.position - currentPlanet.position).normalized;

        StartCoroutine(MoveToPlanet(destination));
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

        SpaceGraphGenerator generator = FindObjectOfType<SpaceGraphGenerator>();
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

        EnableNeighbors(currentPlanet);

        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            enemy.UpdateTargetPlanet(currentPlanet);
        }

        FindObjectOfType<SpaceGraphGenerator>().GenerateNewArea(destination.position);

    }


    void EnableNeighbors(PlanetNode current)
    {
        if (current == null || current.neighbors == null) return;

        PlanetClick[] allPlanets = FindObjectsOfType<PlanetClick>();

        foreach (PlanetClick pc in allPlanets)
        {
            if (pc == null || pc.node == null) continue;

            bool isNeighbor = current.neighbors.Contains(pc.node);
            pc.SetClicable(isNeighbor);
        }
    }
}
