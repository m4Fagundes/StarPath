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
    
    // Propriedade pública para que outros scripts possam saber se a nave está se movendo.
    public bool IsMoving => isMoving;

    public Vector2 TravelDirection =>
        (currentPlanet != null && previousPlanet != null)
            ? (currentPlanet.position - previousPlanet.position).normalized
            : Vector2.right;

    public Vector2 IntendedTravelDirection { get; private set; } = Vector2.zero;

    public void TravelTo(PlanetNode destination)
    {
        // Checa se a viagem é válida.
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
        var generator = FindObjectOfType<SpaceGraphGenerator>();
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

        // Loop principal do movimento da nave.
        while (Vector3.Distance(transform.position, end) > 0.05f)
        {
            float distCovered = (Time.time - startTime) * travelSpeed;
            float fraction = distCovered / journeyLength;
            transform.position = Vector3.Lerp(start, end, fraction);
            yield return null;
        }

        // --- Sequência de eventos de chegada (ORDEM CORRIGIDA) ---
        
        // 1. Atualiza o estado da nave.
        transform.position = end;
        previousPlanet = currentPlanet;
        currentPlanet = destination;
        isMoving = false;

        // 2. Notifica outros sistemas sobre a chegada.
        OnArriveAtPlanet(currentPlanet);

        // 3. Gera o novo mapa PRIMEIRO, com a nave já parada e segura.
        var generator = FindObjectOfType<SpaceGraphGenerator>();
        if (generator != null)
        {
            generator.GenerateNewArea(destination.position);
        }

        // 4. AGORA, com o mapa atualizado, habilita os vizinhos corretos para o clique.
        EnableNeighbors(currentPlanet);

        // 5. Atualiza o alvo dos inimigos para o novo planeta.
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            enemy.UpdateTargetPlanet(currentPlanet);
        }
    }

    // Função agora é PÚBLICA para ser acessada pelo Gerador do Grafo.
    public void EnableNeighbors(PlanetNode current)
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