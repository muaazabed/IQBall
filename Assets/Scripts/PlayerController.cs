using UnityEngine;

/// <summary>
/// Attach to each player-controlled character.
/// Requires: Rigidbody, CapsuleCollider
/// Tag the GameObject as "Player".
/// 
/// Unity 6000.3 uses the new Input System by default.
/// This script reads raw gamepad axes so it works without
/// an Input Actions asset — just set the playerIndex in the Inspector.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public enum PlayerIndex { Player1_LeftStick, Player2_RightStick }

    [Header("Identity")]
    [Tooltip("Player1 uses left stick + LB.  Player2 uses right stick + RB.")]
    public PlayerIndex playerIndex = PlayerIndex.Player1_LeftStick;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 720f;

    [Tooltip("How quickly the character reaches top speed (higher = snappier)")]
    public float acceleration = 50f;

    [Tooltip("How quickly the character stops when the stick is released (higher = tighter)")]
    public float deceleration = 40f;

    [Header("Jump")]
    public float jumpForce = 7f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool isGrounded;
    private float groundCheckRadius = 0.3f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Lock rotation so the character doesn't topple
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        // Extra drag to kill residual sliding from collisions
        rb.linearDamping = 2f;
    }

    void Update()
    {
        HandleJump();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    // ──────────────────────────────────────────────
    //  INPUT HELPERS  (raw Gamepad via new Input System)
    // ──────────────────────────────────────────────

    private Vector2 ReadStick()
    {
        var gp = UnityEngine.InputSystem.Gamepad.current;
        if (gp == null) return Vector2.zero;

        return playerIndex == PlayerIndex.Player1_LeftStick
            ? gp.leftStick.ReadValue()
            : gp.rightStick.ReadValue();
    }

    private bool ReadJumpButton()
    {
        var gp = UnityEngine.InputSystem.Gamepad.current;
        if (gp == null) return false;

        return playerIndex == PlayerIndex.Player1_LeftStick
            ? gp.leftShoulder.wasPressedThisFrame
            : gp.rightShoulder.wasPressedThisFrame;
    }

    // ──────────────────────────────────────────────
    //  MOVEMENT
    // ──────────────────────────────────────────────

    private void HandleMovement()
    {
        Vector2 input = ReadStick();
        Vector3 vel = rb.linearVelocity;

        // Dead-zone — actively brake when the stick is released
        if (input.magnitude < 0.15f)
        {
            // Decelerate horizontal velocity toward zero
            float brakeAmount = deceleration * Time.fixedDeltaTime;
            vel.x = Mathf.MoveTowards(vel.x, 0f, brakeAmount);
            vel.z = Mathf.MoveTowards(vel.z, 0f, brakeAmount);
            rb.linearVelocity = vel;
            return;
        }

        // World-space direction (X = left/right, Z = forward/back on the pitch)
        Vector3 moveDir = new Vector3(input.x, 0f, input.y).normalized;
        Vector3 targetVel = moveDir * moveSpeed;

        // Accelerate toward the target velocity
        float accelStep = acceleration * Time.fixedDeltaTime;
        vel.x = Mathf.MoveTowards(vel.x, targetVel.x, accelStep);
        vel.z = Mathf.MoveTowards(vel.z, targetVel.z, accelStep);
        rb.linearVelocity = vel;

        // Face movement direction
        if (moveDir != Vector3.zero)
        {
            Quaternion target = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, target, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    // ──────────────────────────────────────────────
    //  JUMP
    // ──────────────────────────────────────────────

    private void HandleJump()
    {
        CheckGrounded();

        if (ReadJumpButton() && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void CheckGrounded()
    {
        // Small sphere-cast downward from the character's feet
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out _, 0.3f, groundLayer);
    }
}