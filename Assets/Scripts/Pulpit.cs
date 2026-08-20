using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class Pulpit : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float minLifetime = 4f;
    [SerializeField] private float maxLifetime = 5f;

    [Header("Next Pulpit")]
    [SerializeField] private float spawnTime = 2.5f;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 1.5f; // smooth fade window before destruction

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;

    private float lifetime;
    private float timer;

    private bool spawnTriggered;

    private readonly List<Material> materials = new List<Material>();

    // Required by PulpitSpawner
    public event Action<Pulpit> OnSpawnNext;
    public event Action<Pulpit> OnDestroyed;

    public float LifeTime => lifetime;

    private void Awake()
    {
        CollectMaterials();
    }

    private void CollectMaterials()
    {
        Renderer[] allRenderers =
            GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in allRenderers)
        {
            // Ignore TextMeshPro renderer.
            // We don't want the countdown text to fade with the platform.
            if (renderer.GetComponent<TMP_Text>() != null)
                continue;

            // A renderer can contain multiple materials.
            foreach (Material material in renderer.materials)
            {
                if (material != null)
                {
                    materials.Add(material);
                }
            }
        }
    }

    // Called by PulpitSpawner
    public void Initialize()
    {
        lifetime = UnityEngine.Random.Range(
            minLifetime,
            maxLifetime
        );

        timer = 0f;
        spawnTriggered = false;

        // Make sure the platform starts completely visible.
        SetAlpha(1f);

        UpdateTimerText();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float remainingTime = Mathf.Max(
            0f,
            lifetime - timer
        );

        // Update countdown
        UpdateTimerText();

        // Spawn the next Pulpit at 2.5 seconds.
        if (!spawnTriggered && timer >= spawnTime)
        {
            spawnTriggered = true;
            OnSpawnNext?.Invoke(this);
        }

        ApplyFade(remainingTime);

        // Destroy after lifetime expires.
        if (timer >= lifetime)
        {
            OnDestroyed?.Invoke(this);
            Destroy(gameObject);
        }
    }

    private void ApplyFade(float remainingTime)
    {
        // Ratio of remaining time within the fade window (1 = not fading yet, 0 = fully faded).
        float fadeT = Mathf.Clamp01(remainingTime / fadeDuration);

        // SmoothStep gives an ease-in/ease-out curve so the fade
        // starts and ends gently instead of moving at a constant rate.
        float alpha = Mathf.SmoothStep(0f, 1f, fadeT);

        SetAlpha(alpha);
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        float remainingTime = Mathf.Max(
            0f,
            lifetime - timer
        );

        // Example:
        // 4.73
        // 4.72
        // 4.71
        // ...
        // 0.01
        // 0.00
        timerText.text = remainingTime.ToString("0.00");
    }

    private void SetAlpha(float alpha)
    {
        foreach (Material material in materials)
        {
            if (material == null)
                continue;

            // URP Lit materials
            if (material.HasProperty("_BaseColor"))
            {
                Color color = material.GetColor("_BaseColor");
                color.a = alpha;

                material.SetColor("_BaseColor", color);
            }
            // Built-in/legacy materials
            else if (material.HasProperty("_Color"))
            {
                Color color = material.GetColor("_Color");
                color.a = alpha;

                material.SetColor("_Color", color);
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up material instances created by Renderer.materials.
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