using UnityEngine;
using System.Collections.Generic;

public class PulpitSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private Pulpit pulpitPrefab;

    [Header("Settings")]
    [SerializeField] private float tileSize = 9f;
    [SerializeField] private float gap = 1f;

    private Pulpit currentPulpit;
    private Pulpit previousPulpit;

    private readonly List<Vector3> directions = new List<Vector3>()
    {
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right
    };

    private Vector3 lastDirection = Vector3.zero;

    private void Start()
    {
        if (pulpitPrefab == null)
        {
            Debug.LogError("PulpitSpawner: pulpitPrefab is not assigned.", this);
            enabled = false;
            return;
        }

        SpawnFirstPulpit();
    }

    private void SpawnFirstPulpit()
    {
        Vector3 startPosition = Vector3.zero;

        currentPulpit = SpawnPulpit(startPosition);

        if (currentPulpit == null)
        {
            return;
        }

        TileScoreTrigger scoreTrigger = currentPulpit.GetComponentInChildren<TileScoreTrigger>();

        if (scoreTrigger != null)
        {
            scoreTrigger.SetScoringEnabled(false);
        }
    }

    private Pulpit SpawnPulpit(Vector3 position)
    {
        if (pulpitPrefab == null)
        {
            return null;
        }

        Pulpit newPulpit = Instantiate(pulpitPrefab, position, Quaternion.identity);

        newPulpit.Initialize();
        newPulpit.OnSpawnNext += HandleSpawnNext;
        newPulpit.OnDestroyed += HandleDestroyed;

        return newPulpit;
    }

    private void HandleSpawnNext(Pulpit source)
    {
        if (source != currentPulpit)
        {
            return;
        }

        Vector3 nextPosition = GetNextPosition();

        previousPulpit = currentPulpit;
        currentPulpit = SpawnPulpit(nextPosition);
    }

    private Vector3 GetNextPosition()
    {
        if (currentPulpit == null)
        {
            return transform.position;
        }

        List<Vector3> availableDirections = new List<Vector3>(directions);

        if (lastDirection != Vector3.zero)
        {
            availableDirections.Remove(-lastDirection);
        }

        if (availableDirections.Count == 0)
        {
            return currentPulpit.transform.position;
        }

        Vector3 chosenDirection = availableDirections[Random.Range(0, availableDirections.Count)];
        lastDirection = chosenDirection;

        float distance = tileSize + gap;
        return currentPulpit.transform.position + chosenDirection * distance;
    }

    private void HandleDestroyed(Pulpit destroyed)
    {
        if (destroyed == previousPulpit)
        {
            previousPulpit = null;
        }

        if (destroyed == currentPulpit)
        {
            currentPulpit = null;
        }
    }
}