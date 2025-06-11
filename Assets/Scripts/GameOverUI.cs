using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    private bool show = false;
    
    public void Show()
    {
        show = true;
        Time.timeScale = 0f;
    }

    void OnGUI()
    {
        if (!show) return;
        int width = 300, height = 120;
        Rect rect = new Rect((Screen.width-width)/2, (Screen.height-height)/2, width, height);
        GUI.Box(rect, "GAME OVER");
        if (GUI.Button(new Rect(rect.x+50, rect.y+50, 200, 40), "Reiniciar"))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
