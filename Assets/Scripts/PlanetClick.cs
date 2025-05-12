using UnityEngine;

public class PlanetClick : MonoBehaviour
{
    public PlanetNode node;

    void OnMouseDown()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null || !col.enabled) return;

        GameObject ship = GameObject.Find("PlayerSpaceship");
        if (ship == null) return;

        SpaceshipMover mover = ship.GetComponent<SpaceshipMover>();
        if (mover != null && node != null)
        {
            mover.TravelTo(node);
        }
    }

    public void SetClicable(bool state)
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = state;
        }

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.color = state ? Color.white : Color.gray;
        }
    }
}
