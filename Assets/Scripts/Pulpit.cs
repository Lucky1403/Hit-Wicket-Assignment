using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class Pulpit : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float minLifetime = 4f;
    [SerializeField] private float maxLifetime = 5f;

    [Header("Next Pulpit")]
    [SerializeField] private float spawnTime = 2.5f;

    [Header("Vortex Disappear")]
    [SerializeField] private float vortexDuration = 1.2f;
    [SerializeField] private float sinkDistance = 0.4f;
    [SerializeField] private float finalScale = 0.05f;
    [SerializeField] private float rotationAmount = 1080f;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;

    private float lifetime;
    private float timer;

    private bool spawnTriggered;
    private bool isDisappearing;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private readonly List<Material> materials =
        new List<Material>();

    public event Action<Pulpit> OnSpawnNext;
    public event Action<Pulpit> OnDestroyed;

    public float LifeTime => lifetime;

    private void Awake()
    {
        originalScale = transform.localScale;
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        CollectMaterials();
    }

    private void CollectMaterials()
    {
        Renderer[] allRenderers =
            GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in allRenderers)
        {
            // Create material instances so this tile does not affect
            // materials used by other instantiated tiles.
            Material[] rendererMaterials = renderer.materials;

            foreach (Material material in rendererMaterials)
            {
                if (material != null && !materials.Contains(material))
                {
                    materials.Add(material);
                }
            }
        }
    }

    public void Initialize()
    {
        lifetime = UnityEngine.Random.Range(
            minLifetime,
            maxLifetime
        );

        // Make sure the vortex animation can fit inside
        // the randomly generated lifetime.
        vortexDuration = Mathf.Min(
            vortexDuration,
            lifetime
        );

        timer = 0f;
        spawnTriggered = false;
        isDisappearing = false;

        transform.localScale = originalScale;
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        SetAlpha(1f);

        UpdateTimerText();
    }

    private void Update()
    {
        if (isDisappearing)
            return;

        timer += Time.deltaTime;

        float remainingTime = Mathf.Max(
            0f,
            lifetime - timer
        );

        UpdateTimerText();

        // Spawn the next tile.
        if (!spawnTriggered && timer >= spawnTime)
        {
            spawnTriggered = true;
            OnSpawnNext?.Invoke(this);
        }

        // Start the vortex before the lifetime ends.
        if (remainingTime <= vortexDuration)
        {
            StartCoroutine(VortexDisappear());
        }
    }

    private IEnumerator VortexDisappear()
    {
        isDisappearing = true;

        // Stop showing a normal countdown during the animation.
        if (timerText != null)
        {
            timerText.text = "0.00";
        }

        Vector3 startScale = transform.localScale;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        float elapsed = 0f;

        while (elapsed < vortexDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / vortexDuration
            );

            // Accelerates the animation toward the end.
            float curvedT = t * t * (3f - 2f * t);

            // SHRINK
            transform.localScale = Vector3.Lerp(
                startScale,
                originalScale * finalScale,
                curvedT
            );

            // SINK DOWN
            transform.position = Vector3.Lerp(
                startPosition,
                startPosition + Vector3.down * sinkDistance,
                curvedT
            );

            // VORTEX ROTATION
            float currentRotation =
                rotationAmount * curvedT;

            transform.rotation =
                startRotation *
                Quaternion.Euler(
                    0f,
                    currentRotation,
                    currentRotation * 0.15f
                );

            // Keep the tile mostly visible initially,
            // then disappear quickly near the end.
            float alpha = 1f;

            if (t > 0.55f)
            {
                float fadeT =
                    Mathf.InverseLerp(
                        0.55f,
                        1f,
                        t
                    );

                alpha = 1f - fadeT;
            }

            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(0f);

        OnDestroyed?.Invoke(this);

        Destroy(gameObject);
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        float remainingTime = Mathf.Max(
            0f,
            lifetime - timer
        );

        timerText.text =
            remainingTime.ToString("0.00");
    }

    private void SetAlpha(float alpha)
    {
        foreach (Material material in materials)
        {
            if (material == null)
                continue;

            if (material.HasProperty("_BaseColor"))
            {
                Color color =
                    material.GetColor("_BaseColor");

                color.a = alpha;

                material.SetColor(
                    "_BaseColor",
                    color
                );
            }
            else if (material.HasProperty("_Color"))
            {
                Color color =
                    material.GetColor("_Color");

                color.a = alpha;

                material.SetColor(
                    "_Color",
                    color
                );
            }
        }
    }

    private void OnDestroy()
    {
        foreach (Material material in materials)
        {
            if (material != null)
            {
                Destroy(material);
            }
        }

        materials.Clear();
    }
}