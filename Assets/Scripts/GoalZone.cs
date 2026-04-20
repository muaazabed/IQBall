using UnityEngine;

/// <summary>
/// Attach to each goal trigger zone (an empty GameObject with a BoxCollider set to Is Trigger).
/// </summary>
public class GoalZone : MonoBehaviour
{
    public enum GoalOwner { PlayerGoal, EnemyGoal }

    [Tooltip("Which side owns this goal?\n" +
             "PlayerGoal = if ball enters here the ENEMY scores.\n" +
             "EnemyGoal  = if ball enters here the PLAYER scores.")]
    public GoalOwner owner;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            if (owner == GoalOwner.PlayerGoal)
            {
                GameManager.Instance.EnemyScored();
            }
            else
            {
                GameManager.Instance.PlayerScored();
            }
        }
    }
}
