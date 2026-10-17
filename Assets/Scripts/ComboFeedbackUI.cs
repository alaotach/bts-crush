using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ComboFeedbackUI : MonoBehaviour
{
    public static ComboFeedbackUI Instance;
    
    [Header("UI Prefab")]
    public GameObject floatingTextPrefab; // Prefab with TextMeshPro
    
    [Header("Canvas Reference")]
    public Transform canvasTransform;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowComboFeedback(int comboCount, Vector3 worldPosition)
    {
        if (floatingTextPrefab == null || canvasTransform == null) return;
        
        string message = "";
        Color color = Color.white;
        float scale = 1f;

        if (comboCount >= 8)
        {
            message = "ENCORE!";
            color = new Color(1f, 0.2f, 0.8f); // Pink/Purple
            scale = 1.5f;
        }
        else if (comboCount >= 6)
        {
            message = "AMAZING!";
            color = new Color(1f, 0.8f, 0.2f); // Gold
            scale = 1.3f;
        }
        else if (comboCount >= 4)
        {
            message = "GREAT!";
            color = new Color(0.2f, 1f, 0.5f); // Green
            scale = 1.1f;
        }
        else if (comboCount >= 2)
        {
            message = "GOOD!";
            color = new Color(0.2f, 0.8f, 1f); // Blue
            scale = 1f;
        }
        else
        {
            return; // Don't show for tiny combos
        }

        GameObject floatingText = Instantiate(floatingTextPrefab, canvasTransform);
        
        // Convert world position to screen/canvas position
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector2 screenPos = cam.WorldToScreenPoint(worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasTransform as RectTransform, 
                screenPos, 
                null, // Use null for overlay canvas, or cam for screen space camera
                out Vector2 localPoint);
                
            floatingText.GetComponent<RectTransform>().anchoredPosition = localPoint;
        }

        TMP_Text tmpText = floatingText.GetComponent<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.text = message;
            tmpText.color = color;
        }

        floatingText.transform.localScale = Vector3.zero;
        
        // Start animation and destruction
        StartCoroutine(AnimateAndDestroy(floatingText, scale));
    }

    private IEnumerator AnimateAndDestroy(GameObject target, float targetScale)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        
        // Pop in
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Overshoot ease out
            t = Mathf.Sin(t * Mathf.PI * 0.5f) * 1.1f; 
            target.transform.localScale = Vector3.one * (targetScale * t);
            yield return null;
        }
        
        target.transform.localScale = Vector3.one * targetScale;
        
        // Hold
        yield return new WaitForSeconds(0.7f);
        
        // Fade and move up
        elapsed = 0f;
        duration = 0.4f;
        TMP_Text tmpText = target.GetComponent<TMP_Text>();
        Vector3 startPos = target.transform.position;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            target.transform.position = startPos + Vector3.up * (50f * t); // Move up 50 units
            
            if (tmpText != null)
            {
                Color c = tmpText.color;
                c.a = 1f - t;
                tmpText.color = c;
            }
            
            yield return null;
        }
        
        Destroy(target);
    }
}
