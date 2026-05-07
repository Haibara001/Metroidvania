using UnityEngine;

public class WitchThrowing : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private bool destroyOnHit = true;

    private float damage;
    private Vector2 direction;
    private bool hitOnce;

    public void Setup(Vector2 dir, float dmg, LayerMask layer)
    {
        direction = dir.normalized;
        damage = dmg;
        hitLayer = layer;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & hitLayer) == 0)
        {
            return;
        }

        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable == null)
        {
            damageable = other.GetComponentInParent<IDamageable>();
        }

        if (damageable == null)
        {
            return;
        }

        bool hit = damageable.TakeDamage(damage);

        if (hit && destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}
