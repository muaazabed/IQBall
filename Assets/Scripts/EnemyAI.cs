using UnityEngine;

/// <summary>
/// Attach to each AI-controlled enemy character.
/// Requires: Rigidbody, CapsuleCollider
/// Tag the GameObject as "Enemy".
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the SoccerBall here")]
    public Transform ball;

    [Tooltip("The transform the AI wants to push the ball toward (the Player's goal)")]
    public Transform targetGoal;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 540f;

    [Header("Behaviour Tuning")]
    [Tooltip("AI switches from chasing the ball to retreating when ball is this far behind it")]
    public float retreatThreshold = 1f;

    [Tooltip("Random offset so the two AI don't perfectly overlap")]
    public Vector3 positionOffset = Vector3.zero;

    [Header("Boundaries")]
    [Tooltip("Clamp the AI to the field (half-width on X)")]
    public float fieldHalfX = 14f;
    [Tooltip("Clamp the AI to the field (half-length on Z)")]
    public float fieldHalfZ = 22f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void FixedUpdate()
    {
        if (ball == null || targetGoal == null) return;

        Vector3 ballPos = ball.position;
        Vector3 myPos = transform.position;

        // Decide target point:
        // If the ball is roughly between us and the goal, chase it.
        // Otherwise, reposition toward our own defensive zone.
        Vector3 goalDir = (targetGoal.position - myPos).normalized;
        Vector3 toBall = ballPos - myPos;

        Vector3 destination;

        float dotToGoal = Vector3.Dot(toBall.normalized, goalDir);
        if (dotToGoal > -0.2f || toBall.magnitude < retreatThreshold)
        {
            // Offensive: aim to be behind the ball relative to the target goal
            // so contact pushes ball goalward
            Vector3 behindBall = ballPos - goalDir * 1.2f;
            destination = behindBall + positionOffset;
        }
        else
        {
            // Retreat toward the midpoint between ball and own half
            //destination = new Vector3(ballPos.x, myPos.y, myPos.z) + positionOffset;
            Vector3 behindBall = ballPos - goalDir * 1.2f;
            destination = behindBall + positionOffset;
        }

        destination.y = myPos.y; // stay on the ground plane

        Vector3 moveDir = (destination - myPos);
        moveDir.y = 0f;

        if (moveDir.magnitude < 0.25f) return;

        moveDir = moveDir.normalized;

        // Apply movement
        Vector3 vel = rb.linearVelocity;
        vel.x = moveDir.x * moveSpeed;
        vel.z = moveDir.z * moveSpeed;
        rb.linearVelocity = vel;

        // Face direction
        Quaternion target = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, target, rotationSpeed * Time.fixedDeltaTime);

        // Clamp to field
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -fieldHalfX, fieldHalfX);
        pos.z = Mathf.Clamp(pos.z, -fieldHalfZ, fieldHalfZ);
        transform.position = pos;
    }
}
