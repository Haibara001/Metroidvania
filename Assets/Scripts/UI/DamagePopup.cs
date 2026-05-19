using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float offsetY = 0.8f;
    [SerializeField] private float randomOffsetX = 0.3f;

    private TextMeshPro textMesh;
    private MeshRenderer meshRenderer;
    private float timer;
    private Color startColor;

    public static DamagePopup Create(Vector3 position, float damage, float fontSize = 4f, Color? color = null, TMP_FontAsset font = null, string sortingLayer = "Player", int sortingOrder = 50, float offsetY = 0.8f, float randomOffsetX = 0.3f)
    {
        GameObject popupObj = new GameObject("DamagePopup");
        popupObj.transform.position = position + new Vector3(Random.Range(-randomOffsetX, randomOffsetX), offsetY, 0f);

        DamagePopup popup = popupObj.AddComponent<DamagePopup>();
        popup.Setup(damage, fontSize, color ?? Color.white, font, sortingLayer, sortingOrder);

        return popup;
    }

    private void Setup(float damage, float fontSize, Color color, TMP_FontAsset font, string sortingLayer, int sortingOrder)
    {
        textMesh = gameObject.AddComponent<TextMeshPro>();

        if (font != null)
        {
            textMesh.font = font;
        }
        else if (TMP_Settings.defaultFontAsset != null)
        {
            textMesh.font = TMP_Settings.defaultFontAsset;
        }

        textMesh.text = Mathf.RoundToInt(damage).ToString();
        textMesh.fontSize = fontSize;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = color;
        startColor = textMesh.color;

        textMesh.outlineWidth = 0.3f;
        textMesh.outlineColor = Color.black;

        meshRenderer = textMesh.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingLayerName = sortingLayer;
            meshRenderer.sortingOrder = sortingOrder;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        if (timer >= lifetime - fadeDuration)
        {
            float fadeProgress = (timer - (lifetime - fadeDuration)) / fadeDuration;
            textMesh.color = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0f), fadeProgress);
        }

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
