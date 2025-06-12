using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    private bool show = false;
    private GUIStyle boxStyle;
    private GUIStyle buttonStyle;
    private GUIStyle labelStyle;
    private GUIStyle shadowStyle;

    public void Show()
    {
        show = true;
        Time.timeScale = 0f;
    }

    void Start()
    {
        InitStyles();
    }

    private void OnGUI()
    {
        if (!show) return;

        if (boxStyle == null || buttonStyle == null || labelStyle == null || shadowStyle == null)
            InitStyles();

        // Aumentei o tamanho do retângulo (550x300)
        int width = 550, height = 300;
        Rect rect = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);

        // Caixa de fundo com estilo
        GUI.Box(rect, GUIContent.none, boxStyle);

        // Ajustei as posições dos textos
        GUI.Label(new Rect(rect.x + 2, rect.y + 38, rect.width, 80), "GAME OVER", shadowStyle);
        GUI.Label(new Rect(rect.x, rect.y + 35, rect.width, 80), "GAME OVER", labelStyle);

        // Botão "Reiniciar" - posicionado mais abaixo
        Rect buttonRect = new Rect(rect.x + (rect.width - 200) / 2, rect.y + 180, 200, 60);
        if (GUI.Button(buttonRect, "🔄 Reiniciar", buttonStyle))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void InitStyles()
    {
        // Fundo aumentado (550x300)
        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeBeautifulGradientBox(550, 300, 
            new Color(0.05f, 0.05f, 0.1f, 0.9f),
            new Color(0.02f, 0.02f, 0.05f, 0.95f),
            new Color(0.3f, 0.3f, 0.5f, 0.2f));
        boxStyle.border = new RectOffset(32, 32, 32, 32);
        boxStyle.padding = new RectOffset(20, 20, 20, 20);
        boxStyle.margin = new RectOffset(10, 10, 10, 10);

        // Botão estilizado
        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 22;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.alignment = TextAnchor.MiddleCenter;
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.textColor = Color.yellow;
        buttonStyle.normal.background = MakeRoundedButtonTex(200, 60, new Color(0.2f, 0.6f, 1f, 1f), 30f);
        buttonStyle.hover.background = MakeRoundedButtonTex(200, 60, new Color(0.3f, 0.7f, 1f, 1f), 30f);
        buttonStyle.active.background = MakeRoundedButtonTex(200, 60, new Color(0.1f, 0.5f, 0.9f, 1f), 30f);
        buttonStyle.border = new RectOffset(20, 20, 20, 20);
        buttonStyle.padding = new RectOffset(10, 10, 10, 10);

        // Texto principal aumentado
        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 60;  // Aumentei o tamanho da fonte
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = new Color(1f, 0.2f, 0.2f);

        // Sombra
        shadowStyle = new GUIStyle(labelStyle);
        shadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.6f);
    }

    private Texture2D MakeBeautifulGradientBox(int width, int height, Color topColor, Color bottomColor, Color glowColor)
    {
        Texture2D tex = new Texture2D(width, height);
        
        for (int y = 0; y < height; y++)
        {
            float t = (float)y / height;
            Color baseColor = Color.Lerp(topColor, bottomColor, t);
            
            for (int x = 0; x < width; x++)
            {
                float centerDist = Vector2.Distance(new Vector2(x, y), new Vector2(width/2, height/2));
                float glowFactor = Mathf.Clamp01(1 - (centerDist / (width/2))) * 0.5f;
                
                Color finalColor = baseColor + glowColor * glowFactor;
                finalColor.a = Mathf.Lerp(topColor.a, bottomColor.a, t);
                tex.SetPixel(x, y, finalColor);
            }
        }
        
        float borderRadius = 40f;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float distToEdge = Mathf.Min(
                    Mathf.Min(x, width - x),
                    Mathf.Min(y, height - y)
                );
                
                if (distToEdge < borderRadius)
                {
                    float edgeFactor = distToEdge / borderRadius;
                    Color current = tex.GetPixel(x, y);
                    tex.SetPixel(x, y, new Color(current.r, current.g, current.b, current.a * edgeFactor));
                }
            }
        }
        
        tex.Apply();
        return tex;
    }

    private Texture2D MakeRoundedButtonTex(int width, int height, Color color, float borderRadius)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] clearColors = new Color[width * height];
        for (int i = 0; i < clearColors.Length; i++)
            clearColors[i] = Color.clear;
        tex.SetPixels(clearColors);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool inRoundedRect = true;
                
                if (x < borderRadius && y < borderRadius)
                {
                    float dx = borderRadius - x;
                    float dy = borderRadius - y;
                    if (Mathf.Sqrt(dx * dx + dy * dy) > borderRadius)
                        inRoundedRect = false;
                }
                else if (x < borderRadius && y > height - borderRadius)
                {
                    float dx = borderRadius - x;
                    float dy = y - (height - borderRadius);
                    if (Mathf.Sqrt(dx * dx + dy * dy) > borderRadius)
                        inRoundedRect = false;
                }
                else if (x > width - borderRadius && y < borderRadius)
                {
                    float dx = x - (width - borderRadius);
                    float dy = borderRadius - y;
                    if (Mathf.Sqrt(dx * dx + dy * dy) > borderRadius)
                        inRoundedRect = false;
                }
                else if (x > width - borderRadius && y > height - borderRadius)
                {
                    float dx = x - (width - borderRadius);
                    float dy = y - (height - borderRadius);
                    if (Mathf.Sqrt(dx * dx + dy * dy) > borderRadius)
                        inRoundedRect = false;
                }
                
                if (inRoundedRect)
                {
                    tex.SetPixel(x, y, color);
                }
            }
        }
        
        tex.Apply();
        return tex;
    }
}