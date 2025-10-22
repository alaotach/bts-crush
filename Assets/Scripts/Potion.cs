using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{

    public BTSCandyType candyType;
    public int xIndex;
    public int yIndex;
    public bool isMatched = false;
    public bool isSpecialCandy = false; 
    public BTSCandyType baseColor = BTSCandyType.RM;
    private Vector2 currentPos;
    private Vector2 targetPos;
    public bool isMoving = false;
    
    private SpecialCandyVisualizer visualizer;

    public Potion(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }

    public void SetIndices(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }
    

    public void UpdateVisualEffects()
    {
        if (visualizer == null)
        {
            visualizer = GetComponent<SpecialCandyVisualizer>();
            if (visualizer == null)
            {
                visualizer = gameObject.AddComponent<SpecialCandyVisualizer>();
            }
        }
        
        switch (candyType)
        {
            case BTSCandyType.StripedHorizontal:
                visualizer.ApplyHorizontalStripes();
                isSpecialCandy = true;
                break;
                
            case BTSCandyType.StripedVertical:
                visualizer.ApplyVerticalStripes();
                isSpecialCandy = true;
                break;
                
            case BTSCandyType.Balloon:
                Color memberColor = baseColor.GetMemberColor();
                visualizer.ApplyBalloon(memberColor);
                isSpecialCandy = true;
                break;
                
            case BTSCandyType.ColorBomb:
                visualizer.ApplyRainbow();
                isSpecialCandy = true;
                break;
                
            case BTSCandyType.SuperBomb:
                visualizer.ApplyRainbow();
                isSpecialCandy = true;
                break;
                
            default:
                visualizer.ClearEffects();
                isSpecialCandy = false;
                break;
        }
        
    }

    public void MoveToTarget(Vector2  _targetPos)
    {
        StartCoroutine(MoveRoutine(_targetPos));
    }
    
    public void MoveToTarget(Vector2  _targetPos, float delay)
    {
        StartCoroutine(MoveRoutineWithDelay(_targetPos, delay));
    }
    
    private IEnumerator MoveRoutineWithDelay(Vector2 _targetPos, float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(MoveRoutine(_targetPos));
    }
    private IEnumerator MoveRoutine(Vector2 _targetPos)
    {
        isMoving = true;
        float duration = 0.2f; 

        Vector2 startPosition   = transform.position;
        float elapsedTime      = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            transform.position = Vector2.Lerp(startPosition, _targetPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = _targetPos;
        isMoving = false;
    }
}
