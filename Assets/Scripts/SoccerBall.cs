using UnityEngine;

/// <summary>
/// Attach to the soccer ball GameObject.
/// Requires: Rigidbody, SphereCollider
/// </summary>
public class SoccerBall : MonoBehaviour
{
    [Header("Kick Settings")]
    [Tooltip("Force applied when a character touches the ball")]
    public float kickForce = 12f;

    [Tooltip("Upward bias added to every kick so the ball arcs")]
    public float kickLift = 2f;

    [Header("Speed Limits")]
    public float maxSpeed = 25f;

    private Rigidbody rb;
    private Vector3 startPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        // Clamp velocity so the ball doesn't go crazy
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            // Direction = away from the character that touched it
            Vector3 kickDir = (transform.position - collision.transform.position).normalized;
            kickDir.y = 0f; // flatten, then add lift separately
            kickDir = kickDir.normalized;

            // Reset velocity so kicks feel consistent
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 force = (kickDir * kickForce) + (Vector3.up * kickLift);
            rb.AddForce(force, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Called by GoalZone after a goal is scored.
    /// </summary>
    public void ResetBall()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPosition;
    }
}
