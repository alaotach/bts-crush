using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles special candy activation and combo effects for BTS-themed match-3.
/// Manages all the special clearing patterns, animations, and combos.
/// </summary>
public class BTSSpecialCandyManager : MonoBehaviour
{
    [Header("References")]
    public PotionBoard board;
    public BTSCandyDatabase candyDatabase;
    
    [Header("Visual Effects")]
    public GameObject musicNotesPrefab;
    public GameObject lightstickBeamPrefab;
    public GameObject albumBombExplosionPrefab;
    public GameObject stageLightsPrefab;
    public GameObject chibiExplosionPrefab;
    public GameObject fanChantPrefab;
    public GameObject dynamiteExplosionPrefab;
    public GameObject butterSlidePrefab;
    public GameObject megaBombPrefab;
    
    [Header("Audio")]
    public AudioClip micCandySound;
    public AudioClip albumBombSound;
    public AudioClip chibiExplosionSound;
    public AudioClip dynamiteSound;
    public AudioClip fanChantSound;
    public AudioClip comboSound;
    
    private AudioSource audioSource;
    
    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }
    
    /// <summary>
    /// Activate a special candy at the given position
    /// </summary>
    public IEnumerator ActivateSpecialCandy(BTSCandyType candyType, int x, int y, BTSCandyType targetColor = BTSCandyType.RM)
    {
        BTSCandyData candyData = candyDatabase.GetCandyData(candyType);
        
        if (candyData == null || !candyData.isSpecial)
        {
            yield break;
        }
        
        // Play activation sound
        if (candyData.activationSound != null)
        {
            audioSource.PlayOneShot(candyData.activationSound);
        }
        
        List<Vector2Int> candiesToClear = GetClearPattern(candyData.clearPattern, x, y, candyData.clearRadius, targetColor);
        
        // Spawn visual effect
        yield return StartCoroutine(SpawnEffectForCandy(candyType, x, y, candiesToClear));
        
        yield return StartCoroutine(ClearCandiesWithDelay(candiesToClear));
    }
    
    /// <summary>
    /// Get list of positions to clear based on pattern
    /// </summary>
    private List<Vector2Int> GetClearPattern(ClearPattern pattern, int centerX, int centerY, int radius, BTSCandyType targetColor)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        
        switch (pattern)
        {
            case ClearPattern.Row:
                positions = GetRowPattern(centerY);
                break;
                
            case ClearPattern.Column:
                positions = GetColumnPattern(centerX);
                break;
                
            case ClearPattern.Cross:
                positions = GetCrossPattern(centerX, centerY);
                break;
                
            case ClearPattern.Area3x3:
                positions = GetAreaPattern(centerX, centerY, 1);
                break;
                
            case ClearPattern.Area5x5:
                positions = GetAreaPattern(centerX, centerY, 2);
                break;
                
            case ClearPattern.XShape:
                positions = GetXShapePattern(centerX, centerY);
                break;
                
            case ClearPattern.AllOfColor:
                positions = GetAllOfColorPattern(targetColor);
                break;
                
            case ClearPattern.Wave:
                positions = GetWavePattern(centerX, centerY, radius);
                break;
                
            case ClearPattern.Random:
                positions = GetRandomPattern(10); // Clear 10 random candies
                break;
        }
        
        return positions;
    }
    
    private List<Vector2Int> GetRowPattern(int row)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        for (int x = 0; x < board.width; x++)
        {
            positions.Add(new Vector2Int(x, row));
        }
        return positions;
    }
    
    private List<Vector2Int> GetColumnPattern(int col)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        for (int y = 0; y < board.height; y++)
        {
            positions.Add(new Vector2Int(col, y));
        }
        return positions;
    }
    
    private List<Vector2Int> GetCrossPattern(int centerX, int centerY)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        positions.AddRange(GetRowPattern(centerY));
        positions.AddRange(GetColumnPattern(centerX));
        return positions;
    }
    
    private List<Vector2Int> GetAreaPattern(int centerX, int centerY, int radius)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (x >= 0 && x < board.width && y >= 0 && y < board.height)
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }
        }
        
        return positions;
    }
    
    private List<Vector2Int> GetXShapePattern(int centerX, int centerY)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        
        // Diagonal lines
        for (int offset = -3; offset <= 3; offset++)
        {
            // Top-left to bottom-right
            int x1 = centerX + offset;
            int y1 = centerY + offset;
            if (x1 >= 0 && x1 < board.width && y1 >= 0 && y1 < board.height)
            {
                positions.Add(new Vector2Int(x1, y1));
            }
            
            // Top-right to bottom-left
            int x2 = centerX + offset;
            int y2 = centerY - offset;
            if (x2 >= 0 && x2 < board.width && y2 >= 0 && y2 < board.height)
            {
                positions.Add(new Vector2Int(x2, y2));
            }
        }
        
        return positions;
    }
    
    private List<Vector2Int> GetAllOfColorPattern(BTSCandyType targetColor)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                if (board.potionBoard[x, y] != null && 
                    board.potionBoard[x, y].isUsable && 
                    board.potionBoard[x, y].potion != null)
                {
                    Potion potion = board.potionBoard[x, y].potion.GetComponent<Potion>();
                    if (potion.candyType == targetColor)
                    {
                        positions.Add(new Vector2Int(x, y));
                    }
                }
            }
        }
        
        Debug.Log($"Found {positions.Count} candies of color {targetColor}");
        return positions;
    }
    
    private List<Vector2Int> GetWavePattern(int centerX, int centerY, int maxRadius)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        
        for (int radius = 1; radius <= maxRadius; radius++)
        {
            positions.AddRange(GetAreaPattern(centerX, centerY, radius));
        }
        
        return positions;
    }
    
    private List<Vector2Int> GetRandomPattern(int count)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        
        for (int i = 0; i < count; i++)
        {
            int x = Random.Range(0, board.width);
            int y = Random.Range(0, board.height);
            positions.Add(new Vector2Int(x, y));
        }
        
        return positions;
    }
    
    /// <summary>
    /// Spawn visual effects for special candy activation
    /// </summary>
    private IEnumerator SpawnEffectForCandy(BTSCandyType candyType, int x, int y, List<Vector2Int> targets)
    {
        Vector3 worldPos = board.GetWorldPositionForCell(x, y);
        
        switch (candyType)
        {
            case BTSCandyType.MicCandy:
            case BTSCandyType.Lightstick:
                yield return StartCoroutine(PlayMicOrLightstickEffect(worldPos, targets));
                break;
                
            case BTSCandyType.AlbumBomb:
            case BTSCandyType.StageBomb:
                if (albumBombExplosionPrefab != null)
                {
                    Instantiate(albumBombExplosionPrefab, worldPos, Quaternion.identity);
                }
                audioSource.PlayOneShot(albumBombSound);
                break;
                
            case BTSCandyType.RM:
            case BTSCandyType.Jin:
            case BTSCandyType.Suga:
            case BTSCandyType.JHope:
            case BTSCandyType.Jimin:
            case BTSCandyType.V:
            case BTSCandyType.Jungkook:
            case BTSCandyType.FanHeartBomb:
                if (chibiExplosionPrefab != null)
                {
                    Instantiate(chibiExplosionPrefab, worldPos, Quaternion.identity);
                }
                audioSource.PlayOneShot(chibiExplosionSound);
                break;
                
            case BTSCandyType.DynamiteCandy:
                if (dynamiteExplosionPrefab != null)
                {
                    Instantiate(dynamiteExplosionPrefab, worldPos, Quaternion.identity);
                }
                audioSource.PlayOneShot(dynamiteSound);
                break;
                
            case BTSCandyType.ButterSlide:
                yield return StartCoroutine(PlayButterSlideEffect(worldPos, targets));
                break;
                
            case BTSCandyType.FanChant:
                if (fanChantPrefab != null)
                {
                    Instantiate(fanChantPrefab, worldPos, Quaternion.identity);
                }
                audioSource.PlayOneShot(fanChantSound);
                break;
        }
        
        yield return new WaitForSeconds(0.3f);
    }
    
    private IEnumerator PlayMicOrLightstickEffect(Vector3 startPos, List<Vector2Int> targets)
    {
        // Shoot music notes or beam along the line
        foreach (var target in targets)
        {
            Vector3 targetPos = board.GetWorldPositionForCell(target.x, target.y);
            
            if (musicNotesPrefab != null)
            {
                GameObject note = Instantiate(musicNotesPrefab, startPos, Quaternion.identity);
                StartCoroutine(MoveEffectToTarget(note, targetPos, 0.2f));
            }
            
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    private IEnumerator PlayButterSlideEffect(Vector3 startPos, List<Vector2Int> targets)
    {
        if (butterSlidePrefab != null)
        {
            GameObject butter = Instantiate(butterSlidePrefab, startPos, Quaternion.identity);
            
            foreach (var target in targets)
            {
                Vector3 targetPos = board.GetWorldPositionForCell(target.x, target.y);
                yield return StartCoroutine(MoveEffectToTarget(butter, targetPos, 0.1f));
            }
            
            Destroy(butter);
        }
    }
    
    private IEnumerator MoveEffectToTarget(GameObject effect, Vector3 target, float duration)
    {
        Vector3 start = effect.transform.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            effect.transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
    }
    
    private IEnumerator ClearCandiesWithDelay(List<Vector2Int> positions)
    {
        foreach (var pos in positions)
        {
            if (board != null)
            {
                board.ClearCandyAt(pos.x, pos.y);
            }
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    /// <summary>
    /// Handle combo when two special candies are combined
    /// </summary>
    public IEnumerator HandleSpecialCombo(BTSCandyType candy1, BTSCandyType candy2, int x, int y)
    {
        audioSource.PlayOneShot(comboSound);
        
        // Mega combo effect
        if (megaBombPrefab != null)
        {
            Vector3 worldPos = board.GetWorldPositionForCell(x, y);
            Instantiate(megaBombPrefab, worldPos, Quaternion.identity);
        }
        
        // Determine combo effect based on combination
        // Mic + Chibi = Clear 3 rows and 3 columns
        // Album + Lightstick = Clear all of 2 colors
        // etc.
        
        yield return new WaitForSeconds(1f);
    }
}
