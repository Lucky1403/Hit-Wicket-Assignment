using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class Pulpit : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float minLifetime = 4f;
    [SerializeField] private float maxLifetime = 5f;

    [Header("Next Tile Spawn Time")]
    [SerializeField] private float spawnTime = 2.5f;

    [Header("Vortex Dissolve")]
    [SerializeField] private float dissolveDuration = 0.5f;

    [Header("Spawn Animation")]
    [SerializeField] private float spawnDuration = 0.2f;
    [SerializeField] private float spawnOvershoot = 1.08f;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;

    [Header("Tile Renderer")]
    [SerializeField] private Renderer tileRenderer;

    private float lifetime;
    private float timer;
    private bool spawnTriggered;
    private bool isInitialized;
    private Vector3 originalScale;
    private MaterialPropertyBlock propertyBlock;
    private static readonly int DissolveAmountHash = Shader.PropertyToID("_DissolveAmount");

    public event Action<Pulpit> OnSpawnNext;
    public event Action<Pulpit> OnDestroyed;
    public float LifeTime => lifetime;

    private void Awake()
    {
        originalScale = transform.localScale;
        propertyBlock = new MaterialPropertyBlock();

        if (tileRenderer == null)
        {
            tileRenderer = GetComponentInChildren<Renderer>();
        }
    }

    public void Initialize()
    {
        lifetime = UnityEngine.Random.Range(minLifetime, maxLifetime);

        timer = 0f;
        spawnTriggered = false;
        isInitialized = true;

        SetDissolveAmount(0f);
        UpdateTimerText();

        StopAllCoroutines();
        StartCoroutine(SpawnAnimation());
    }

    private void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        timer += Time.deltaTime;

        float remainingTime = Mathf.Max(0f, lifetime - timer);

        UpdateTimerText();

        if (!spawnTriggered && timer >= spawnTime)
        {
            spawnTriggered = true;
            OnSpawnNext?.Invoke(this);
        }

        if (remainingTime <= dissolveDuration)
        {
            float dissolveProgress = 1f - (remainingTime / dissolveDuration);
            dissolveProgress = Mathf.Clamp01(dissolveProgress);
            dissolveProgress = Mathf.SmoothStep(0f, 1f, dissolveProgress);

            SetDissolveAmount(dissolveProgress);
        }

        if (timer >= lifetime)
        {
            SetDissolveAmount(1f);
            OnDestroyed?.Invoke(this);
            Destroy(gameObject);
        }
    }

    private IEnumerator SpawnAnimation()
    {
        transform.localScale = Vector3.zero;

        float elapsed = 0f;

        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / spawnDuration);
            t = 1f - Mathf.Pow(1f - t, 3f);

            float scaleMultiplier = Mathf.Lerp(0f, spawnOvershoot, t);
            transform.localScale = originalScale * scaleMultiplier;

            yield return null;
        }

        transform.localScale = originalScale * spawnOvershoot;

        elapsed = 0f;
        float settleDuration = spawnDuration * 0.5f;

        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / settleDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = Vector3.Lerp(originalScale * spawnOvershoot, originalScale, t);

            yield return null;
        }

        transform.localScale = originalScale;
    }

    private void SetDissolveAmount(float amount)
    {
        if (tileRenderer == null || propertyBlock == null)
        {
            return;
        }

        tileRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(DissolveAmountHash, amount);
        tileRenderer.SetPropertyBlock(propertyBlock);
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        float remainingTime = Mathf.Max(0f, lifetime - timer);
        timerText.text = remainingTime.ToString("0.00");
    }

    private void OnDestroy()
    {
        OnSpawnNext = null;
        OnDestroyed = null;
    }
}