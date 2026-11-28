using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public Transform sprite;
    public float maxSpeed = 5f;
    public float smoothing = 10f;
    public float turnSpeed = 540f;
    [SerializeField] InputAction move = DefaultMove();

    Rigidbody2D rb;
    Health health;
    Vector2 input;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (!TryGetComponent(out health)) health = gameObject.AddComponent<Health>();
    }

    static InputAction DefaultMove()
    {
        var action = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        action.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        action.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        action.AddBinding("<Gamepad>/leftStick");
        action.AddBinding("<Joystick>/stick");
        return action;
    }

    void OnEnable() => move.Enable();
    void OnDisable() => move.Disable();

    void Update()
    {
        input = Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1f);

        if (input != Vector2.zero)
        {
            float target = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg - 90f;
            float angle = Mathf.MoveTowardsAngle(sprite.eulerAngles.z, target, turnSpeed * Time.deltaTime);
            sprite.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void FixedUpdate()
    {
        // Hurt players limp: down to 55% speed near death.
        float speed = maxSpeed * Mathf.Lerp(1f, 0.55f, health.Damaged);
        Vector2 target = (Vector2)sprite.up * (speed * input.magnitude);
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, target, 1f - Mathf.Exp(-smoothing * Time.fixedDeltaTime));
    }
}