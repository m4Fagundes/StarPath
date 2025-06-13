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
            if(player.coins > 3)
            {
                player.currentBullets++;
                player.coins -= 3;
                PlayerPrefs.SetInt("Coins", player.coins);
                PlayerPrefs.Save();
                PlayerPrefs.SetInt("Bullets", player.currentBullets);
                PlayerPrefs.Save();
            }         
        }
    }

    public void OnBuyGas()
    {
        var player = FindObjectOfType<SpaceshipMover>();
        if (player != null)
        {
            if (player.coins >= 5)
            {
                player.currentFuel++;
                player.coins -= 5;
                PlayerPrefs.SetInt("Coins", player.coins);
                PlayerPrefs.Save();
            }
        }              
    }
}
