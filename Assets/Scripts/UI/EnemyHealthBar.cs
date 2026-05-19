using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private float barWidth = 1.2f;
    [SerializeField] private float barHeight = 0.14f;
    [SerializeField] private float borderPadding = 0.03f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.35f, 0f);

    [Header("Sorting")]
    [SerializeField] private string sortingLayerName = "Player";
    [SerializeField] private int sortingOrder = 20;

    [Header("Colors")]
    [SerializeField] private Color bgColor = new Color(0.1f, 0.05f, 0.15f, 0.85f);
    [SerializeField] private Color fillColor = new Color(0.82f, 0.2f, 0.3f, 1f);
    [SerializeField] private Color borderColor = new Color(0.35f, 0.2f, 0.5f, 0.95f);

    private Enemy enemy;
    private Renderer targetRenderer;
    private Transform rootTransform;
    private Transform fillTransform;
    private SpriteRenderer fillRenderer;
    private float lastHp = -1f;
    private Sprite barSprite;
    private float innerBarWidth;
    private float innerBarHeight;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        targetRenderer = GetComponentInChildren<Renderer>();
        CreateBar();
    }

    private void LateUpdate()
    {
        if (enemy == null || rootTransform == null)
        {
            return;
        }

        if (enemy.IsDead())
        {
            rootTransform.gameObject.SetActive(false);
            return;
        }

        if (!rootTransform.gameObject.activeSelf)
        {
            rootTransform.gameObject.SetActive(true);
        }

        rootTransform.position = GetBarWorldPosition();

        float hp = Mathf.Clamp01(enemy.HealthPercent);
        if (Mathf.Abs(hp - lastHp) > 0.001f)
        {
            lastHp = hp;
            fillTransform.localScale = new Vector3(innerBarWidth * hp, innerBarHeight, 1f);
            fillTransform.localPosition = new Vector3(-(innerBarWidth * (1f - hp)) * 0.5f, 0f, 0f);

            if (fillRenderer != null)
            {
                fillRenderer.enabled = hp > 0f;
            }
        }
    }

    private void OnDestroy()
    {
        if (rootTransform != null)
        {
            Destroy(rootTransform.gameObject);
        }

        if (barSprite != null)
        {
            Destroy(barSprite.texture);
            Destroy(barSprite);
        }
    }

    private void CreateBar()
    {
        barSprite = CreateSolidSprite();
        innerBarWidth = barWidth - borderPadding * 2f;
        innerBarHeight = barHeight - borderPadding * 2f;

        GameObject root = new GameObject("HPBar");
        rootTransform = root.transform;
        rootTransform.SetParent(transform, false);
        rootTransform.localPosition = Vector3.zero;

        CreateSegment("Border", rootTransform, barWidth, barHeight, borderColor, sortingOrder);
        CreateSegment("Bg", rootTransform, innerBarWidth, innerBarHeight, bgColor, sortingOrder + 1);

        GameObject fill = CreateSegment("Fill", rootTransform, innerBarWidth, innerBarHeight, fillColor, sortingOrder + 2);
        fillTransform = fill.transform;
        fillRenderer = fill.GetComponent<SpriteRenderer>();
    }

    private GameObject CreateSegment(string objectName, Transform parent, float width, float height, Color color, int order)
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = new Vector3(width, height, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = barSprite;
        sr.color = color;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = order;

        return go;
    }

    private Vector3 GetBarWorldPosition()
    {
        if (targetRenderer != null)
        {
            Bounds bounds = targetRenderer.bounds;
            return new Vector3(bounds.center.x, bounds.max.y, transform.position.z) + offset;
        }

        return transform.position + offset;
    }

    private Sprite CreateSolidSprite()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        texture.filterMode = FilterMode.Point;

        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}
