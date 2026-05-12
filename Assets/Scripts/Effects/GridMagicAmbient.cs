using System.Collections.Generic;
using UnityEngine;

public class GridMagicAmbient : MonoBehaviour
{
    [Header("Area")]
    [SerializeField] private bool autoFitToChildRenderers = true;
    [SerializeField] private Vector2 manualAreaSize = new Vector2(40f, 24f);
    [SerializeField] private Vector2 areaPadding = new Vector2(2f, 1f);

    [Header("Floating Motes")]
    [SerializeField] private int moteCount = 45;
    [SerializeField] private float minMoteSize = 0.05f;
    [SerializeField] private float maxMoteSize = 0.18f;
    [SerializeField] private float minRiseSpeed = 0.08f;
    [SerializeField] private float maxRiseSpeed = 0.35f;
    [SerializeField] private float horizontalDrift = 0.12f;
    [SerializeField] private float twinkleSpeed = 1.4f;

    [Header("Light Trails")]
    [SerializeField] private int trailCount = 10;
    [SerializeField] private int trailSegments = 10;
    [SerializeField] private float trailLength = 1.8f;
    [SerializeField] private float minTrailSpeed = 0.4f;
    [SerializeField] private float maxTrailSpeed = 0.9f;

    [Header("Look")]
    [SerializeField] private string sortingLayerName = "Background";
    [SerializeField] private int sortingOrder = 2;
    [SerializeField] private Color coreColor = new Color(0.55f, 0.9f, 1f, 0.65f);
    [SerializeField] private Color accentColor = new Color(0.9f, 0.55f, 1f, 0.55f);
    [SerializeField] private Color sparkleColor = new Color(1f, 0.92f, 0.7f, 0.8f);

    private readonly List<Mote> motes = new List<Mote>();
    private readonly List<Trail> trails = new List<Trail>();

    private Transform generatedRoot;
    private Sprite softSprite;
    private Vector2 areaSize;
    private Vector3 localCenter;
    private bool initialized;

    private class Mote
    {
        public Transform transform;
        public SpriteRenderer renderer;
        public Vector2 velocity;
        public float baseSize;
        public float alphaMin;
        public float alphaMax;
        public float twinkleOffset;
    }

    private class Trail
    {
        public Vector2 headPosition;
        public Vector2 direction;
        public float speed;
        public float alpha;
        public float phaseOffset;
        public float spacing;
        public readonly List<Transform> segments = new List<Transform>();
        public readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>();
    }

    private void OnEnable()
    {
        InitializeIfNeeded();
    }

    private void Start()
    {
        InitializeIfNeeded();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        InitializeIfNeeded();

        float dt = Time.deltaTime;
        UpdateMotes(dt);
        UpdateTrails(dt);
    }

    private void OnDisable()
    {
        CleanupGeneratedObjects();
        initialized = false;
    }

    private void OnDestroy()
    {
        CleanupGeneratedObjects();

        if (softSprite != null)
        {
            Destroy(softSprite.texture);
            Destroy(softSprite);
        }
    }

    [ContextMenu("Rebuild Ambient FX")]
    private void RebuildAmbientFx()
    {
        CleanupGeneratedObjects();
        initialized = false;
        InitializeIfNeeded();
    }

    private void InitializeIfNeeded()
    {
        if (initialized)
        {
            return;
        }

        if (softSprite == null)
        {
            softSprite = CreateSoftSprite();
        }

        ResolveArea();
        CreateGeneratedRoot();
        SpawnMotes();
        SpawnTrails();
        initialized = true;
    }

    private void ResolveArea()
    {
        areaSize = manualAreaSize;
        localCenter = Vector3.zero;

        if (!autoFitToChildRenderers)
        {
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Bounds bounds = default;
        bool hasBounds = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            if (renderer == null || renderer.transform == transform || !renderer.enabled)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (!hasBounds)
        {
            return;
        }

        areaSize = new Vector2(bounds.size.x + areaPadding.x * 2f, bounds.size.y + areaPadding.y * 2f);
        localCenter = transform.InverseTransformPoint(bounds.center);
    }

    private void CreateGeneratedRoot()
    {
        generatedRoot = new GameObject("MagicAmbient_Runtime").transform;
        generatedRoot.SetParent(transform, false);
        generatedRoot.localPosition = localCenter;
        generatedRoot.localRotation = Quaternion.identity;
        generatedRoot.localScale = Vector3.one;
    }

    private void SpawnMotes()
    {
        float halfWidth = areaSize.x * 0.5f;
        float halfHeight = areaSize.y * 0.5f;

        for (int i = 0; i < moteCount; i++)
        {
            GameObject moteObject = new GameObject("Mote_" + i);
            moteObject.transform.SetParent(generatedRoot, false);

            SpriteRenderer sr = moteObject.AddComponent<SpriteRenderer>();
            sr.sprite = softSprite;
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;

            Color baseColor = PickColor(i % 3);
            float startAlpha = Random.Range(0.15f, baseColor.a);
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, startAlpha);

            float size = Random.Range(minMoteSize, maxMoteSize);
            moteObject.transform.localScale = new Vector3(size, size, 1f);
            moteObject.transform.localPosition = new Vector3(
                Random.Range(-halfWidth, halfWidth),
                Random.Range(-halfHeight, halfHeight),
                0f);

            motes.Add(new Mote
            {
                transform = moteObject.transform,
                renderer = sr,
                velocity = new Vector2(
                    Random.Range(-horizontalDrift, horizontalDrift),
                    Random.Range(minRiseSpeed, maxRiseSpeed)),
                baseSize = size,
                alphaMin = Mathf.Clamp01(startAlpha * 0.45f),
                alphaMax = Mathf.Clamp01(baseColor.a),
                twinkleOffset = Random.Range(0f, Mathf.PI * 2f)
            });
        }
    }

    private void SpawnTrails()
    {
        float halfWidth = areaSize.x * 0.5f;
        float halfHeight = areaSize.y * 0.5f;

        for (int i = 0; i < trailCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
            Trail trail = new Trail
            {
                headPosition = new Vector2(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight)),
                direction = direction,
                speed = Random.Range(minTrailSpeed, maxTrailSpeed),
                alpha = Random.Range(0.25f, 0.65f),
                phaseOffset = Random.Range(0f, Mathf.PI * 2f),
                spacing = trailLength / Mathf.Max(1, trailSegments)
            };

            Color baseColor = Random.value > 0.5f ? accentColor : coreColor;

            for (int segmentIndex = 0; segmentIndex < trailSegments; segmentIndex++)
            {
                GameObject segmentObject = new GameObject("Trail_" + i + "_" + segmentIndex);
                segmentObject.transform.SetParent(generatedRoot, false);

                SpriteRenderer sr = segmentObject.AddComponent<SpriteRenderer>();
                sr.sprite = softSprite;
                sr.sortingLayerName = sortingLayerName;
                sr.sortingOrder = sortingOrder + 1;
                sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);

                trail.segments.Add(segmentObject.transform);
                trail.renderers.Add(sr);
            }

            trails.Add(trail);
        }
    }

    private void UpdateMotes(float dt)
    {
        float halfWidth = areaSize.x * 0.5f;
        float halfHeight = areaSize.y * 0.5f;
        float time = Time.time * twinkleSpeed;

        for (int i = 0; i < motes.Count; i++)
        {
            Mote mote = motes[i];
            Vector3 pos = mote.transform.localPosition;
            pos.x += mote.velocity.x * dt;
            pos.y += mote.velocity.y * dt;

            if (pos.y > halfHeight)
            {
                pos.y = -halfHeight;
            }

            if (pos.x > halfWidth)
            {
                pos.x = -halfWidth;
            }
            else if (pos.x < -halfWidth)
            {
                pos.x = halfWidth;
            }

            mote.transform.localPosition = pos;

            float pulse = 0.5f + Mathf.Sin(time + mote.twinkleOffset) * 0.5f;
            float alpha = Mathf.Lerp(mote.alphaMin, mote.alphaMax, pulse);
            float scale = mote.baseSize * Mathf.Lerp(0.82f, 1.22f, pulse);

            mote.transform.localScale = new Vector3(scale, scale, 1f);

            Color color = mote.renderer.color;
            color.a = alpha;
            mote.renderer.color = color;
        }
    }

    private void UpdateTrails(float dt)
    {
        float halfWidth = areaSize.x * 0.5f;
        float halfHeight = areaSize.y * 0.5f;

        for (int i = 0; i < trails.Count; i++)
        {
            Trail trail = trails[i];
            trail.headPosition += trail.direction * trail.speed * dt;

            if (trail.headPosition.x > halfWidth + trailLength) trail.headPosition.x = -halfWidth - trailLength;
            if (trail.headPosition.x < -halfWidth - trailLength) trail.headPosition.x = halfWidth + trailLength;
            if (trail.headPosition.y > halfHeight + trailLength) trail.headPosition.y = -halfHeight - trailLength;
            if (trail.headPosition.y < -halfHeight - trailLength) trail.headPosition.y = halfHeight + trailLength;

            float pulse = 0.5f + Mathf.Sin(Time.time * 1.2f + trail.phaseOffset) * 0.5f;
            float trailAlpha = trail.alpha * Mathf.Lerp(0.65f, 1f, pulse);

            for (int segmentIndex = 0; segmentIndex < trail.segments.Count; segmentIndex++)
            {
                float ratio = (float)segmentIndex / Mathf.Max(1, trail.segments.Count - 1);
                Vector2 segmentPos = trail.headPosition - trail.direction * trail.spacing * segmentIndex;
                float segmentScale = Mathf.Lerp(0.16f, 0.035f, ratio);
                float alpha = trailAlpha * (1f - ratio);

                trail.segments[segmentIndex].localPosition = new Vector3(segmentPos.x, segmentPos.y, 0f);
                trail.segments[segmentIndex].localScale = new Vector3(segmentScale, segmentScale, 1f);

                Color color = trail.renderers[segmentIndex].color;
                color.a = alpha;
                trail.renderers[segmentIndex].color = color;
            }
        }
    }

    private Color PickColor(int index)
    {
        switch (index)
        {
            case 0:
                return coreColor;
            case 1:
                return accentColor;
            default:
                return sparkleColor;
        }
    }

    private Sprite CreateSoftSprite()
    {
        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[size * size];
        float center = (size - 1) * 0.5f;
        float radius = center - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float normalized = Mathf.Clamp01(distance / radius);
                float alpha = Mathf.Pow(1f - normalized, 2.8f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private void CleanupGeneratedObjects()
    {
        motes.Clear();
        trails.Clear();

        if (generatedRoot == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(generatedRoot.gameObject);
        }
        else
        {
            DestroyImmediate(generatedRoot.gameObject);
        }

        generatedRoot = null;
    }
}
