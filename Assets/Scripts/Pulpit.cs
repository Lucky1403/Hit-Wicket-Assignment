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
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    private float lifetime;
    private float timer;

    private bool spawnTriggered;

    private readonly List<Material> materials = new List<Material>();
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
            if (renderer.GetComponent<TMP_Text>() != null)
                continue;

            foreach (Material material in renderer.materials)
            {
                if (material != null)
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

        timer = 0f;
        spawnTriggered = false;

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

        UpdateTimerText();

        if (!spawnTriggered && timer >= spawnTime)
        {
            spawnTriggered = true;
            OnSpawnNext?.Invoke(this);
        }

        ApplyFade(remainingTime);

        if (timer >= lifetime)
        {
            OnDestroyed?.Invoke(this);
            Destroy(gameObject);
        }
    }

    private void ApplyFade(float remainingTime)
    {
        float fadeT = Mathf.Clamp01(remainingTime / fadeDuration);

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

        timerText.text = remainingTime.ToString("0.00");
    }

    private void SetAlpha(float alpha)
    {
        foreach (Material material in materials)
        {
            if (material == null)
                continue;

            if (material.HasProperty("_BaseColor"))
            {
                Color color = material.GetColor("_BaseColor");
                color.a = alpha;

                material.SetColor("_BaseColor", color);
            }
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