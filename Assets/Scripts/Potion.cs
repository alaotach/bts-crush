using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{

    public PotionType potionType;
    public int xIndex;
    public int yIndex;
    public bool isMatched = false;
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

public enum PotionType
{
    Red,
    Blue,
    Purple,
    Green,
    White
}