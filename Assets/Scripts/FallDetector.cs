using UnityEngine;
using System.Collections;

public class FallDetector : MonoBehaviour
{
    [Header("Fall Detection")]
    [SerializeField] private float fallStartHeight = -1.5f;
    [SerializeField] private float fallDuration = 0.5f;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private MonoBehaviour playerController;

    private bool isFallingToDeath = false;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    private void Update()
    {
        if (isFallingToDeath || !IsBelowFallThreshold())
        {
            return;
        }

        StartCoroutine(HandleFall());
    }

    private bool IsBelowFallThreshold()
    {
        return transform.position.y <= fallStartHeight;
    }

    private IEnumerator HandleFall()
    {
        isFallingToDeath = true;

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        yield return new WaitForSeconds(fallDuration);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.GameOver();
        }
    }
}