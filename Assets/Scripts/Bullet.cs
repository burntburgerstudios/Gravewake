using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 25f;
    public float damage = 25f;
    public float knockback = 5f;
    public float range = 14f;

    float traveled;

    void Update()
    {
        float step = speed * Time.deltaTime;
        Vector3 start = transform.position;
        Vector3 end = start + transform.up * step;

        foreach (RaycastHit2D hit in Physics2D.LinecastAll(start, end))
        {
            Health h = hit.collider.GetComponentInParent<Health>();

            if (h != null)
            {
                if (h.isPlayer) continue;
                h.Damage(damage, transform.up * knockback);
                LevelManager.PlayHit();
                LevelManager.PlayZombieHurt();
                Destroy(gameObject);
                return;
            }

            if (!hit.collider.isTrigger)
            {
                Destroy(gameObject); // wall
                return;
            }
        }

        transform.position = end;
        traveled += step;

        Vector3 view = Camera.main.WorldToViewportPoint(transform.position);
        bool offscreen = view.x < 0f || view.x > 1f || view.y < 0f || view.y > 1f;
        if (traveled > range || offscreen)
            Destroy(gameObject);
    }
}