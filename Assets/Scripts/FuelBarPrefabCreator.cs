using UnityEngine;
using UnityEngine.UI;

public class FuelBarPrefabCreator : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Create FuelBar Prefab")]
    public void CreateFuelBarPrefab()
    {
        // Cria Canvas
        GameObject canvasObj = new GameObject("FuelBarCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Cria fundo da barra
        GameObject bgObj = new GameObject("FuelBarBG", typeof(Image));
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImage = bgObj.GetComponent<Image>();
        bgImage.color = new Color(0,0,0,0.5f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 1f);
        bgRect.anchorMax = new Vector2(0.5f, 1f);
        bgRect.pivot = new Vector2(0.5f, 1f);
        bgRect.anchoredPosition = new Vector2(0, -20);
        bgRect.sizeDelta = new Vector2(200, 24);

        // Cria barra de preenchimento
        GameObject fillObj = new GameObject("FuelBarFill", typeof(Image));
        fillObj.transform.SetParent(bgObj.transform, false);
        Image fillImage = fillObj.GetComponent<Image>();
        fillImage.color = Color.yellow;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // Adiciona FuelBar script
        var fuelBar = bgObj.AddComponent<FuelBar>();
        fuelBar.fillImage = fillImage;
        fuelBar.player = FindObjectOfType<SpaceshipMover>();

#if UNITY_EDITOR
        // Salva como prefab
        string localPath = "Assets/Prefabs/FuelBar.prefab";
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(canvasObj, localPath);
        Debug.Log("FuelBar prefab salvo em: " + localPath);
#endif
    }
#endif
}
