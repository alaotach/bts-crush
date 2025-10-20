using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{

    public BTSCandyType candyType; // Changed from potionType
    public int xIndex;
    public int yIndex;
    public bool isMatched = false;
    public bool isSpecialCandy = false; // Marks if this is a special candy (MicCandy, AlbumBomb, etc.)
    public BTSCandyType baseColor = BTSCandyType.RM; // The "color" of this special candy (which member it can match with)
    private Vector2 currentPos;
    private Vector2 targetPos;
    public bool isMoving = false;

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

// PotionType enum removed - now using BTSCandyType instead