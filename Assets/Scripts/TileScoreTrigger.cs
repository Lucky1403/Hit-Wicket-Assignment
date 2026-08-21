using UnityEngine;

public class TileScoreTrigger : MonoBehaviour
{
    private bool hasBeenScored = false;
    private bool scoringEnabled = true;

    public void SetScoringEnabled(bool enabled)
    {
        scoringEnabled = enabled;

        if (!enabled)
        {
            hasBeenScored = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!scoringEnabled || hasBeenScored)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        hasBeenScored = true;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(1);
        }
    }
}