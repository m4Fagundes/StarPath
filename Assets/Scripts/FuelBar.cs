using UnityEngine;
using UnityEngine.UI;

public class FuelBar : MonoBehaviour
{
    public SpaceshipMover player;
    public Image fillImage;

    void Update()
    {
        if (player != null && fillImage != null)
        {
            fillImage.fillAmount = player.currentFuel / player.maxFuel;
        }
    }
}
