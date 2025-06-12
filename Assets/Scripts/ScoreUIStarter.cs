using UnityEngine;

public class ScoreUIStarter : MonoBehaviour
{
    void Awake()
    {
        if (FindObjectOfType<ScoreUI>() == null)
        {
            var go = new GameObject("ScoreUI");
            go.AddComponent<ScoreUI>();
        }
    }
}
