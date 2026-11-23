using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ZombieEnemy : MonoBehaviour
{
    enum State { Wander, Investigate, Chase }

    public SpriteRenderer render;
    public Sprite[] frames;
    public bool spritesFaceLeft = true;
    public float wanderSpeed = 1f;
    public float investigateSpeed = 2f;
    public float chaseSpeed = 3f;
    public float turnSpeed = 200f;
    public float sightRange = 8f;
    public float viewAngle = 90f;
    public float closeRange = 1.5f;
    public float loseTime = 3f;
    public float wanderRadius = 5f;
    public float attackRange = 1.2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    public float attackPush = 4f;
    public float backstab = 1.5f;
    public float flankChance = 0.6f;
    public float flankRadius = 4f;
    public float alertRadius = 10f;
    public float lookAhead = 1.2f;

    static List<ZombieEnemy> all = new List<ZombieEnemy>();

    Rigidbody2D rb;
    Health health;
    PlayerMovement playerMove;
    Transform player;
    State state = State.Wander;
    Vector2 target;
    Vector2 aim; 
    bool flanker;
    float avoidSign = 1f;
    float angle;
    float loseTimer;
    float waitTimer;
    float nextAttack;
    float thinkTimer;

    Vector2 Facing => new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

    void OnEnable() { all.Add(this); }
    void OnDisable() { all.Remove(this); }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (!TryGetComponent(out health)) health = gameObject.AddComponent<Health>();
        if (render == null) render = GetComponentInChildren<SpriteRenderer>();

        playerMove = FindAnyObjectByType<PlayerMovement>();
        if (playerMove != null) player = playerMove.transform;

        flanker = Random.value < flankChance;
        angle = Random.Range(0f, 360f);
        PickWanderPoint();
        Think();
        UpdateSprite();
    }

    public static void MakeNoise(Vector2 pos, float radius)
    {
        foreach (ZombieEnemy z in all)
        {
            if (Vector2.Distance(z.transform.position, pos) <= radius)
                z.Hear(pos);
        }
    }

    void Hear(Vector2 pos)
    {
        if (state == State.Chase) return;
        state = State.Investigate;
        target = pos;
        waitTimer = 2f;
    }

    void Update()
    {
        if (CanSee())
        {
            if (state != State.Chase) MakeNoise(player.position, alertRadius);
            state = State.Chase;
            target = ChasePoint();
            loseTimer = 0f;
        }
        else if (state == State.Chase)
        {
            loseTimer += Time.deltaTime;
            if (loseTimer >= loseTime) PickWanderPoint();
        }
        else if (IsIdle())
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f) PickWanderPoint();
        }

        thinkTimer -= Time.deltaTime;
        if (thinkTimer <= 0f)
        {
            thinkTimer = 0.1f;
            Think();
        }

        Attack();
        Turn();
        UpdateSprite();
    }

    void FixedUpdate()
    {
        if (health.Stunned) return;

        if (aim == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float speed = wanderSpeed;
        if (state == State.Investigate) speed = investigateSpeed;
        if (state == State.Chase) speed = chaseSpeed;

        rb.linearVelocity = Facing * speed * Align(aim);
    }

    Vector2 ChasePoint()
    {
        Vector2 p = player.position;
        if (!flanker) return p;

        Vector2 fromPlayer = (Vector2)transform.position - p;
        Vector2 back = -(Vector2)playerMove.sprite.up;
        float off = Vector2.SignedAngle(back, fromPlayer);

        if (fromPlayer.magnitude < 2.5f || Mathf.Abs(off) < 30f) return p;

        float step = Mathf.Min(Mathf.Abs(off), 45f);
        Vector2 dir = Rotate(fromPlayer.normalized, -Mathf.Sign(off) * step);
        return p + dir * flankRadius;
    }

    void Think()
    {
        Vector2 to = target - rb.position;
        aim = to.magnitude < 0.3f ? Vector2.zero : Steer(to.normalized);
    }

    Vector2 Steer(Vector2 want)
    {
        if (!Blocked(want)) return want;

        for (int a = 30; a <= 150; a += 30)
        {
            Vector2 first = Rotate(want, a * avoidSign);
            if (!Blocked(first)) return first;

            Vector2 other = Rotate(want, -a * avoidSign);
            if (!Blocked(other))
            {
                avoidSign = -avoidSign;
                return other;
            }
        }
        return want;
    }

    bool Blocked(Vector2 dir)
    {
        foreach (RaycastHit2D h in Physics2D.CircleCastAll(rb.position, 0.3f, dir, lookAhead))
        {
            if (IsWall(h.collider)) return true;
        }
        return false;
    }

    bool WallBetween(Vector2 point)
    {
        foreach (RaycastHit2D h in Physics2D.LinecastAll(rb.position, point))
        {
            if (IsWall(h.collider)) return true;
        }
        return false;
    }

    static bool IsWall(Collider2D c)
    {
        return !c.isTrigger && c.GetComponentInParent<Health>() == null;
    }

    static Vector2 Rotate(Vector2 v, float degrees)
    {
        return Quaternion.Euler(0f, 0f, degrees) * v;
    }

    void Attack()
    {
        if (player == null || Time.time < nextAttack) return;

        Vector2 to = player.position - transform.position;
        if (to.magnitude > attackRange || WallBetween(player.position)) return;

        nextAttack = Time.time + attackCooldown;

        bool fromBehind = Vector2.Dot(playerMove.sprite.up, -to.normalized) < -0.3f;
        float damage = attackDamage * (fromBehind ? backstab : 1f);
        player.GetComponent<Health>().Damage(damage, to.normalized * attackPush);
        LevelManager.PlayPlayerHurt();
    }

    float Align(Vector2 dir)
    {
        return Mathf.Max(0f, Vector2.Dot(Facing, dir.normalized));
    }

    bool CanSee()
    {
        if (player == null) return false;

        Vector2 to = player.position - transform.position;
        float dist = to.magnitude;

        bool inView = dist <= closeRange
                   || (dist <= sightRange && Vector2.Angle(Facing, to) <= viewAngle / 2f);

        return inView && !WallBetween(player.position);
    }

    bool IsIdle()
    {
        if (aim == Vector2.zero) return true;
        return Align(aim) > 0.5f && rb.linearVelocity.sqrMagnitude < 0.01f;
    }

    void PickWanderPoint()
    {
        state = State.Wander;
        target = (Vector2)transform.position + Random.insideUnitCircle * wanderRadius;
        waitTimer = Random.Range(1f, 3f);
    }

    void Turn()
    {
        if (aim == Vector2.zero) return;

        float want = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
        angle = Mathf.MoveTowardsAngle(angle, want, turnSpeed * Time.deltaTime);
    }

    void UpdateSprite()
    {
        if (render == null || frames.Length == 0) return;

        Vector2 f = Facing;
        float fromDown = Vector2.Angle(Vector2.down, f);
        int i = Mathf.RoundToInt(fromDown / 180f * (frames.Length - 1));

        render.sprite = frames[i];
        render.flipX = spritesFaceLeft ? f.x > 0f : f.x < 0f;
    }
}