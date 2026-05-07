using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UI_SnowEffect : MonoBehaviour
{
    [System.Serializable]
    private class Snowflake
    {
        public RectTransform rect;
        public Image image;
        public float speed;
        public float size;
        public float swayAmplitude;
        public float swayFrequency;
        public float swayOffset;
        public float rotationSpeed;
        public float baseX;
        public float y;
    }

    [Header("Count")]
    [SerializeField] private int snowflakeCount = 48;

    [Header("Movement")]
    [SerializeField] private Vector2 fallSpeedRange = new Vector2(40f, 120f);
    [SerializeField] private Vector2 sizeRange = new Vector2(6f, 18f);
    [SerializeField] private Vector2 swayAmplitudeRange = new Vector2(8f, 30f);
    [SerializeField] private Vector2 swayFrequencyRange = new Vector2(0.6f, 1.8f);
    [SerializeField] private Vector2 rotationSpeedRange = new Vector2(-40f, 40f);
    [SerializeField] private float spawnPadding = 24f;

    [Header("Visual")]
    [SerializeField] private Color snowTint = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private Vector2 alphaRange = new Vector2(0.35f, 0.9f);
    [SerializeField] private bool ignoreTimeScale = true;

    private static Sprite snowSprite;

    private readonly List<Snowflake> snowflakes = new List<Snowflake>();
    private RectTransform root;

    private void Awake()
    {
        root = GetComponent<RectTransform>();
        EnsureSnowSprite();
    }

    private void OnEnable()
    {
        RebuildSnowflakes();
    }

    private void OnDisable()
    {
        ClearSnowflakes();
    }

    private void OnValidate()
    {
        snowflakeCount = Mathf.Max(1, snowflakeCount);
        fallSpeedRange.x = Mathf.Max(1f, Mathf.Min(fallSpeedRange.x, fallSpeedRange.y));
        sizeRange.x = Mathf.Max(1f, Mathf.Min(sizeRange.x, sizeRange.y));
        alphaRange.x = Mathf.Clamp01(Mathf.Min(alphaRange.x, alphaRange.y));
        alphaRange.y = Mathf.Clamp01(Mathf.Max(alphaRange.x, alphaRange.y));
    }

    private void Update()
    {
        if (snowflakes.Count == 0)
        {
            return;
        }

        float deltaTime = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        float time = ignoreTimeScale ? Time.unscaledTime : Time.time;
        Rect rect = root.rect;

        float minX = rect.xMin - spawnPadding;
        float maxX = rect.xMax + spawnPadding;
        float minY = rect.yMin - spawnPadding;
        float maxY = rect.yMax + spawnPadding;

        for (int i = 0; i < snowflakes.Count; i++)
        {
            Snowflake flake = snowflakes[i];
            flake.y -= flake.speed * deltaTime;

            if (flake.y < minY - flake.size)
            {
                ResetSnowflake(flake, rect, true);
                flake.y = maxY + Random.Range(0f, rect.height * 0.35f);
            }

            float x = flake.baseX + Mathf.Sin(time * flake.swayFrequency + flake.swayOffset) * flake.swayAmplitude;
            if (x < minX)
            {
                flake.baseX = maxX;
                x = maxX;
            }
            else if (x > maxX)
            {
                flake.baseX = minX;
                x = minX;
            }

            flake.rect.anchoredPosition = new Vector2(x, flake.y);
            flake.rect.Rotate(0f, 0f, flake.rotationSpeed * deltaTime);
        }
    }

    [ContextMenu("Rebuild Snowflakes")]
    public void RebuildSnowflakes()
    {
        if (root == null)
        {
            root = GetComponent<RectTransform>();
        }

        ClearSnowflakes();
        EnsureSnowSprite();

        Rect rect = root.rect;
        for (int i = 0; i < snowflakeCount; i++)
        {
            Snowflake flake = CreateSnowflake(i);
            ResetSnowflake(flake, rect, false);
            snowflakes.Add(flake);
        }
    }

    private Snowflake CreateSnowflake(int index)
    {
        GameObject flakeObject = new GameObject($"Snowflake_{index:00}");
        flakeObject.transform.SetParent(transform, false);

        Image image = flakeObject.AddComponent<Image>();
        image.sprite = snowSprite;
        image.raycastTarget = false;
        image.preserveAspect = true;

        RectTransform flakeRect = image.rectTransform;
        flakeRect.anchorMin = new Vector2(0.5f, 0.5f);
        flakeRect.anchorMax = new Vector2(0.5f, 0.5f);
        flakeRect.pivot = new Vector2(0.5f, 0.5f);

        return new Snowflake
        {
            rect = flakeRect,
            image = image
        };
    }

    private void ResetSnowflake(Snowflake flake, Rect rect, bool respawnFromTop)
    {
        flake.size = Random.Range(sizeRange.x, sizeRange.y);
        flake.speed = Random.Range(fallSpeedRange.x, fallSpeedRange.y);
        flake.swayAmplitude = Random.Range(swayAmplitudeRange.x, swayAmplitudeRange.y);
        flake.swayFrequency = Random.Range(swayFrequencyRange.x, swayFrequencyRange.y);
        flake.swayOffset = Random.Range(0f, Mathf.PI * 2f);
        flake.rotationSpeed = Random.Range(rotationSpeedRange.x, rotationSpeedRange.y);
        flake.baseX = Random.Range(rect.xMin - spawnPadding, rect.xMax + spawnPadding);
        flake.y = respawnFromTop
            ? rect.yMax + spawnPadding
            : Random.Range(rect.yMin - spawnPadding, rect.yMax + spawnPadding);

        flake.rect.sizeDelta = new Vector2(flake.size, flake.size);
        flake.rect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        Color color = snowTint;
        color.a = Random.Range(alphaRange.x, alphaRange.y);
        flake.image.color = color;
        flake.rect.anchoredPosition = new Vector2(flake.baseX, flake.y);
    }

    private void ClearSnowflakes()
    {
        for (int i = 0; i < snowflakes.Count; i++)
        {
            if (snowflakes[i].rect != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(snowflakes[i].rect.gameObject);
                }
                else
                {
                    DestroyImmediate(snowflakes[i].rect.gameObject);
                }
            }
        }

        snowflakes.Clear();
    }

    private static void EnsureSnowSprite()
    {
        if (snowSprite != null)
        {
            return;
        }

        Rect rect = new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height);
        snowSprite = Sprite.Create(Texture2D.whiteTexture, rect, new Vector2(0.5f, 0.5f), 100f);
    }
}
