using UnityEngine;

/// <summary>
/// Attach to each player-controlled character to clamp them inside the field.
/// (The AI already self-clamps; this handles the human players.)
/// </summary>
public class FieldBounds : MonoBehaviour
{
    [Tooltip("Half-width of the field on the X axis")]
    public float halfX = 14f;

    [Tooltip("Half-length of the field on the Z axis")]
    public float halfZ = 22f;

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -halfX, halfX);
        pos.z = Mathf.Clamp(pos.z, -halfZ, halfZ);
        transform.position = pos;
    }
}
