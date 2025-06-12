using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public SpaceshipMover player;
    private GUIStyle labelStyle;
    private GUIStyle shadowStyle;

    void Start()
    {
        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 32;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = new Color(1f, 0.95f, 0.2f);
        labelStyle.alignment = TextAnchor.UpperLeft;

        shadowStyle = new GUIStyle(labelStyle);
        shadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.7f);
    }

    void Update()
    {
        if (player == null || !player.gameObject.activeSelf)
        {
            player = FindObjectOfType<SpaceshipMover>();
        }
    }

    void OnGUI()
    {
        if (player == null) return;
        string scoreText = $"Score: {player.score}\nHigh Score: {player.highScore}";
        Rect rect = new Rect(30, 20, 400, 80);
        GUI.Label(new Rect(rect.x+2, rect.y+2, rect.width, rect.height), scoreText, shadowStyle);
        GUI.Label(rect, scoreText, labelStyle);
    }
}
