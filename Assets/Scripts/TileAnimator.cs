using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class TileAnimator : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    
    private Coroutine idleCoroutine;
    private Coroutine selectedCoroutine;

    private bool isSelected = false;
    private bool isMatched = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (!isMatched)
        {
            StartIdleAnimation();
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void StartIdleAnimation()
    {
        isSelected = false;
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
        if (selectedCoroutine != null) StopCoroutine(selectedCoroutine);
        
        idleCoroutine = StartCoroutine(IdleRoutine());
        spriteRenderer.color = Color.white; // Reset glow
    }

    public void StartSelectedAnimation()
    {
        isSelected = true;
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
        if (selectedCoroutine != null) StopCoroutine(selectedCoroutine);
        
        selectedCoroutine = StartCoroutine(SelectedRoutine());
    }

    public void StartMatchAnimation(System.Action onComplete = null)
    {
        isMatched = true;
        StopAllCoroutines();
        StartCoroutine(MatchRoutine(onComplete));
    }

    private IEnumerator IdleRoutine()
    {
        // Random offset so they don't all breathe at the same time
        float timeOffset = Random.Range(0f, 2f * Mathf.PI);
        float speed = 1.5f;
        float magnitude = 0.03f; // Subtle scale
        
        // Reset scale before starting
        transform.localScale = originalScale;

        while (!isSelected && !isMatched)
        {
            float scaleMod = 1f + Mathf.Sin(Time.time * speed + timeOffset) * magnitude;
            transform.localScale = originalScale * scaleMod;
            yield return null;
        }
    }

    private IEnumerator SelectedRoutine()
    {
        float speed = 8f;
        float magnitude = 0.05f;
        
        // Base slightly larger when selected
        Vector3 selectedBaseScale = originalScale * 1.1f;
        spriteRenderer.color = new Color(1.2f, 1.2f, 1.2f); // Glow effect

        while (isSelected && !isMatched)
        {
            float scaleMod = 1f + Mathf.Sin(Time.time * speed) * magnitude;
            transform.localScale = selectedBaseScale * scaleMod;
            yield return null;
        }
    }

    private IEnumerator MatchRoutine(System.Action onComplete)
    {
        float duration = 0.25f;
        float elapsed = 0f;
        
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.3f; // Pop up

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Ease out
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        elapsed = 0f;
        startScale = transform.localScale;
        targetScale = Vector3.zero; // Shrink to nothing
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Ease in
            t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            
            // Fade out
            Color c = spriteRenderer.color;
            c.a = 1f - t;
            spriteRenderer.color = c;
            
            yield return null;
        }
        
        if (onComplete != null)
        {
            onComplete.Invoke();
        }
    }
}
