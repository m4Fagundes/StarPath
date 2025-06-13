using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Shop_Menager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBuyBullet()
    {
        var player = FindObjectOfType<SpaceshipMover>();
        if(player != null)
        {
            player.currentBullets++;
        }
    }

    public void OnBuyGas()
    {
        var player = FindObjectOfType<SpaceshipMover>();
        if (player != null)
        {
            player.currentFuel++;
        }
    }
}
