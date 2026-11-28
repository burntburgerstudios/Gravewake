using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    enum Gun { Ready, Reloading, Clearing, Jammed }

    public Transform sprite;
    public float fireRate = 7f;
    public int magazine = 15;
    public float reloadTime = 1.6f;
    public float jamClearTime = 1.2f;
    public float baseSpread = 3f;
    public float hurtSpread = 9f;
    public float hitSpread = 8f;
    public float jamChance = 0.01f;
    public float hurtJamChance = 0.08f;
    public float muzzleDistance = 0.6f;
    public float shake = 0.08f;
    public float noiseRadius = 25f;
    // Initial state check: a trigger already held when the level loads still fires.
    [SerializeField] InputAction fire =
        new InputAction("Fire", InputActionType.Button, "<Keyboard>/space") { wantsInitialStateCheck = true };
    [SerializeField] InputAction reload = new InputAction("Reload", InputActionType.Button, "<Keyboard>/r");

    Sprite square;
    Health health;
    GUIStyle style;
    Gun state = Gun.Ready;
    int ammo;
    float nextShot;
    float busyUntil;
    float seenHit;

    void Awake()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        square = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    void OnEnable()
    {
        fire.Enable();
        reload.Enable();
    }

    void OnDisable()
    {
        fire.Disable();
        reload.Disable();
    }

    void Start()
    {
        health = GetComponent<Health>();
        ammo = magazine;
        seenHit = health.LastHit;
    }

    void Update()
    {
        if (health.LastHit != seenHit)
        {
            seenHit = health.LastHit;
            if (state == Gun.Reloading || state == Gun.Clearing) busyUntil += 0.5f;
        }

        if (state == Gun.Reloading && Time.time >= busyUntil)
        {
            ammo = magazine;
            state = Gun.Ready;
        }
        if (state == Gun.Clearing && Time.time >= busyUntil)
            state = Gun.Ready;

        if (reload.WasPressedThisFrame())
        {
            if (state == Gun.Jammed) Begin(Gun.Clearing, jamClearTime);
            else if (state == Gun.Ready && ammo < magazine) Begin(Gun.Reloading, reloadTime);
        }

        if (state != Gun.Ready || !fire.IsPressed() || Time.time < nextShot) return;

        if (ammo <= 0)
        {
            Begin(Gun.Reloading, reloadTime);
            return;
        }

        nextShot = Time.time + 1f / fireRate;
        Shoot();
    }

    void Begin(Gun next, float time)
    {
        state = next;
        busyUntil = Time.time + time * (1f + health.Damaged);
    }

    void Shoot()
    {
        if (Random.value < jamChance + hurtJamChance * health.Damaged)
        {
            state = Gun.Jammed;
            return;
        }

        ammo--;
        LevelManager.PlayShot();

        float spread = baseSpread
                     + hurtSpread * health.Damaged
                     + hitSpread * Mathf.Clamp01(1f - health.SinceHit / 0.8f);

        Vector3 pos = sprite.position + sprite.up * muzzleDistance;
        Quaternion aim = sprite.rotation * Quaternion.Euler(0f, 0f, Random.Range(-spread, spread));

        Make(pos, aim, new Vector3(0.1f, 0.3f, 1f), Color.white).AddComponent<Bullet>();
        Destroy(Make(pos, sprite.rotation, new Vector3(0.45f, 0.45f, 1f), Color.yellow), 0.05f);

        CameraShake.Shake(shake);
        ZombieEnemy.MakeNoise(transform.position, noiseRadius);
    }

    GameObject Make(Vector3 pos, Quaternion rot, Vector3 scale, Color color)
    {
        GameObject go = new GameObject("Shot");
        go.transform.SetPositionAndRotation(pos, rot);
        go.transform.localScale = scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = square;
        sr.color = color;
        sr.sortingOrder = 10;
        return go;
    }

    void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = 26;
            style.alignment = TextAnchor.LowerRight;
        }

        string text = ammo + " / " + magazine;
        if (state == Gun.Reloading) text = "RELOADING";
        if (state == Gun.Clearing) text = "CLEARING JAM";
        if (state == Gun.Jammed) text = "JAMMED - PRESS R";

        style.normal.textColor = state == Gun.Jammed ? Color.red : Color.white;
        GUI.color = Color.white;
        GUI.Label(new Rect(Screen.width - 320, Screen.height - 60, 300, 40), text, style);
    }
}