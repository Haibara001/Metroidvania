using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]
[RequireComponent(typeof(Water))]
public class WaterTrigger : MonoBehaviour
{
    [Header("Buoyancy")]
    [Range(0f, 1f)] public float BuoyancyForce = 0.3f;
    [Range(0f, 1f)] public float WaterDrag = 0.15f;

    private Water _water;
    private EdgeCollider2D _edgeCollider;
    private readonly Dictionary<Rigidbody2D, Vector2> _previousVelocities = new Dictionary<Rigidbody2D, Vector2>();

    private void Awake()
    {
        _water = GetComponent<Water>();
        _edgeCollider = GetComponent<EdgeCollider2D>();
        _edgeCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null || _water == null) return;

        _water.Splash(other.transform.position, rb.velocity.y);

        if (!_previousVelocities.ContainsKey(rb))
        {
            _previousVelocities[rb] = rb.velocity;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        // 施加浮力
        rb.velocity = new Vector2(
            rb.velocity.x * (1f - WaterDrag),
            rb.velocity.y + BuoyancyForce
        );

        // 速度变化显著时产生飞溅
        if (_water != null && _previousVelocities.TryGetValue(rb, out Vector2 prevVel))
        {
            float deltaY = Mathf.Abs(rb.velocity.y - prevVel.y);
            if (deltaY > _water.SplashVelocityThreshold)
            {
                _water.Splash(other.transform.position, deltaY);
            }
        }

        _previousVelocities[rb] = rb.velocity;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        _previousVelocities.Remove(rb);
    }
}
