using UnityEngine;

public class BossPhaseAura : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Enemy boss;
    [SerializeField] private SpriteRenderer sourceRenderer;

    [Header("Phase Trigger")]
    [SerializeField] [Range(0.05f, 0.95f)] private float phaseTwoHealthPercent = 0.5f;

    [Header("Aura Visual")]
    [SerializeField] private Color auraColor = new Color(0.72f, 0.3f, 1f, 0.45f);
    [SerializeField] private Vector3 auraScale = new Vector3(1.12f, 1.12f, 1f);
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float pulseAlpha = 0.12f;
    [SerializeField] private int sortingOrderOffset = -1;

    private SpriteRenderer auraRenderer;
    private bool phaseTwoActive;

    private void Awake()
    {
        if (boss == null)
        {
            boss = GetComponent<Enemy>();
        }

        if (sourceRenderer == null)
        {
            sourceRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        CreateAuraRenderer();
        SetAuraEnabled(false);
    }

    private void LateUpdate()
    {
        if (boss == null || sourceRenderer == null || auraRenderer == null)
        {
            return;
        }

        bool shouldEnableAura = !boss.IsDead() && boss.HealthPercent <= phaseTwoHealthPercent;

        if (shouldEnableAura != phaseTwoActive)
        {
            phaseTwoActive = shouldEnableAura;
            SetAuraEnabled(phaseTwoActive);
        }

        if (!phaseTwoActive)
        {
            return;
        }

        SyncAuraRenderer();
    }

    private void CreateAuraRenderer()
    {
        if (sourceRenderer == null)
        {
            return;
        }

        GameObject auraObject = new GameObject("PhaseTwoAura");
        auraObject.transform.SetParent(sourceRenderer.transform, false);
        auraObject.transform.localPosition = Vector3.zero;
        auraObject.transform.localRotation = Quaternion.identity;
        auraObject.transform.localScale = auraScale;

        auraRenderer = auraObject.AddComponent<SpriteRenderer>();
        auraRenderer.enabled = false;
        auraRenderer.color = auraColor;
        auraRenderer.maskInteraction = sourceRenderer.maskInteraction;
        auraRenderer.drawMode = sourceRenderer.drawMode;
        auraRenderer.size = sourceRenderer.size;
        auraRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        auraRenderer.sortingOrder = sourceRenderer.sortingOrder + sortingOrderOffset;
    }

    private void SyncAuraRenderer()
    {
        auraRenderer.sprite = sourceRenderer.sprite;
        auraRenderer.flipX = sourceRenderer.flipX;
        auraRenderer.flipY = sourceRenderer.flipY;
        auraRenderer.drawMode = sourceRenderer.drawMode;
        auraRenderer.size = sourceRenderer.size;
        auraRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        auraRenderer.sortingOrder = sourceRenderer.sortingOrder + sortingOrderOffset;

        Color color = auraColor;
        color.a = Mathf.Clamp01(auraColor.a + Mathf.Sin(Time.time * pulseSpeed) * pulseAlpha);
        auraRenderer.color = color;
    }

    private void SetAuraEnabled(bool enabled)
    {
        if (auraRenderer != null)
        {
            auraRenderer.enabled = enabled;
        }
    }
}
