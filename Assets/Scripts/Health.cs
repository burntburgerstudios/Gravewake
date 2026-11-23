using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float playerMaxHealth = 50f;

    [HideInInspector] public bool isPlayer;

    float hp;
    float flash;
    float stunUntil;
    float lastHit = -99f;
    bool dead;
    Rigidbody2D rb;
    SpriteRenderer render;
    Texture2D pixel;

    public bool Stunned => Time.time < stunUntil;
    public float Damaged => Mathf.Clamp01(1f - hp / maxHealth);
    public float LastHit => lastHit;
    public float SinceHit => Time.time - lastHit;

    void Awake()
    {
        isPlayer = GetComponent<PlayerMovement>() != null;
        if (isPlayer) maxHealth = playerMaxHealth;
        hp = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        render = GetComponentInChildren<SpriteRenderer>();

        pixel = new Texture2D(1, 1);
        pixel.SetPixel(0, 0, Color.white);
        pixel.Apply();
    }

    public void Damage(float amount, Vector2 push)
    {
        if (dead) return;

        hp -= amount;
        flash = 1f;
        lastHit = Time.time;

        if (rb != null)
        {
            rb.linearVelocity = push;
            stunUntil = Time.time + 0.15f;
        }

        if (isPlayer) CameraShake.Shake(0.3f);
        if (hp <= 0f) Die();
    }

    void Die()
    {
        dead = true;

        if (isPlayer)
        {
            SceneManager.LoadScene(LevelManager.MenuScene);
        }
        else
        {
            CameraShake.Shake(0.2f);
            Destroy(gameObject);
        }
    }

    void Update()
    {
        flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime * 6f);
        if (render != null) render.color = Color.Lerp(Color.white, Color.red, flash);
    }

    void OnGUI()
    {
        if (isPlayer)
        {
            float alpha = Mathf.Clamp01(1f - hp / maxHealth) * 0.6f + flash * 0.25f;
            if (alpha > 0f)
                Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(1f, 0f, 0f, alpha));

            DrawBar(new Rect(20, 20, 220, 22));
        }
        else if (hp < maxHealth)
        {
            Vector3 p = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.9f);
            DrawBar(new Rect(p.x - 25f, Screen.height - p.y, 50f, 6f));
        }
    }

    void DrawBar(Rect r)
    {
        Fill(r, new Color(0f, 0f, 0f, 0.6f));

        float t = Mathf.Clamp01(hp / maxHealth);
        Fill(new Rect(r.x + 1f, r.y + 1f, (r.width - 2f) * t, r.height - 2f), Color.Lerp(Color.red, Color.green, t));
    }

    void Fill(Rect r, Color c)
    {
        GUI.color = c;
        GUI.DrawTexture(r, pixel);
    }
}