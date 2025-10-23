using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
public class ResponsiveCanvasSetup : MonoBehaviour
{
-    public Vector2 referenceResolution = new Vector2(480f, 848f);
    
    [Range(0f, 1f)]
    public float matchWidthOrHeight = 0.5f;
    public ScreenOrientation targetOrientation = ScreenOrientation.Portrait;
    
    private void Awake()
    {
        SetupCanvas();
        SetScreenOrientation();
    }
    
    private void SetupCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.matchWidthOrHeight = matchWidthOrHeight;
        scaler.referencePixelsPerUnit = 100f;
        
    }
    
    private void SetScreenOrientation()
    {
        #if !UNITY_EDITOR
        Screen.orientation = targetOrientation;
        #endif
    }
}
