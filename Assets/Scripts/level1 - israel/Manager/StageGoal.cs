using UnityEngine;

/// <summary>
/// Per-scene goal setter. Put this in EACH stage scene and set the goal in Inspector.
/// </summary>
public class StageGoal : MonoBehaviour
{
    [SerializeField] private int targetCoins = 150; // Set different value per scene in Inspector

    private void Start()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.SetTarget(targetCoins);
        else
            Debug.LogWarning("StageGoal: ScoreManager.Instance is null.");
    }
}
