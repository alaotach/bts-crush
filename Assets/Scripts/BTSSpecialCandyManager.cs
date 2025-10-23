using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTSSpecialCandyManager : MonoBehaviour
{
    public PotionBoard board;
    public BTSCandyDatabase candyDatabase;
    
    public GameObject lightstickBeamPrefab;
    
    public GameObject explosionPrefab;
    
    public GameObject megaBombPrefab;
    
    public AudioClip stripedSound;
    
    public AudioClip balloonSound;
    
    public AudioClip colorBombSound;
    
    public AudioClip superBombSound;
    
    public AudioClip comboSound;
    
    public AudioClip swapSound;
    
    public AudioClip invalidSwapSound;
    
    public AudioClip matchSound3;
    
    public AudioClip matchSound4;
    
    public AudioClip matchSound5;
    
    public AudioClip matchSound7Plus;
    
    public AudioClip specialCreatedSound;
    
    public AudioClip cascadeSound;
    
    public AudioClip levelCompletedSound;
    
    public AudioClip levelFailedSound;
    
    public AudioClip oneStarSound;
    
    public AudioClip twoStarSound;
    
    public AudioClip threeStarSound;
    
    public AudioClip lowMovesSound;
    
    public AudioClip buttonPressSound;
    
    public AudioClip buttonDownSound;
    
    public AudioClip buttonReleaseSound;
    
    public AudioClip panelOpenSound;
    
    public AudioClip panelCloseSound;
    
    public AudioClip levelUnlockedSound;
    
    public AudioClip episodeUnlockedSound;
    
    public AudioClip allAboardSound;
    
    public AudioClip ticketsSound;
    
    public AudioClip obstacleClearedSound;
    
    private AudioSource audioSource;
    private static int activationCounter = 0; 
    private HashSet<string> activeActivations = new HashSet<string>(); 
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        if (audioSource == null) return;
        
        audioSource.PlayOneShot(clip);
    }
    
    public void PlayMatchSound(int comboSize)
    {
        if (comboSize >= 7)
            PlaySound(matchSound7Plus);
        else if (comboSize >= 5)
            PlaySound(matchSound5);
        else if (comboSize == 4)
            PlaySound(matchSound4);
        else if (comboSize >= 3)
            PlaySound(matchSound3);
    }
    
    public void PlaySwapSound() => PlaySound(swapSound);
    public void PlayInvalidSwapSound() => PlaySound(invalidSwapSound);
    public void PlaySpecialCreatedSound() => PlaySound(specialCreatedSound);
    public void PlayCascadeSound() => PlaySound(cascadeSound);
    
    public void PlayLevelCompletedSound() => PlaySound(levelCompletedSound);
    public void PlayLevelFailedSound() => PlaySound(levelFailedSound);
    public void PlayLowMovesSound() => PlaySound(lowMovesSound);
    
    public void PlayStarSound(int stars)
    {
        switch (stars)
        {
            case 1: PlaySound(oneStarSound); break;
            case 2: PlaySound(twoStarSound); break;
            case 3: PlaySound(threeStarSound); break;
        }
    }
    
    public void PlayButtonPressSound() => PlaySound(buttonPressSound);
    public void PlayButtonDownSound() => PlaySound(buttonDownSound);
    public void PlayButtonReleaseSound() => PlaySound(buttonReleaseSound);
    public void PlayPanelOpenSound() => PlaySound(panelOpenSound);
    public void PlayPanelCloseSound() => PlaySound(panelCloseSound);
    
    public void PlayLevelUnlockedSound() => PlaySound(levelUnlockedSound);
    public void PlayEpisodeUnlockedSound() => PlaySound(episodeUnlockedSound);
    public void PlayAllAboardSound() => PlaySound(allAboardSound);
    public void PlayTicketsSound() => PlaySound(ticketsSound);
    public void PlayObstacleClearedSound() => PlaySound(obstacleClearedSound);
    
    public void PlayCustomSound(AudioClip clip) => PlaySound(clip);
    
    public IEnumerator ActivateSpecialCandy(BTSCandyType candyType, int x, int y, BTSCandyType targetColor = BTSCandyType.RM)
    {
        int activationId = ++activationCounter;
        string candyKey = $"{candyType}_{x}_{y}";
        if (activeActivations.Contains(candyKey))
        {
            yield break;
        }
        
        activeActivations.Add(candyKey);
        
        BTSCandyData candyData = candyDatabase.GetCandyData(candyType);
        
        if (candyData == null)
        {
            activeActivations.Remove(candyKey);
            yield break;
        }
        
        List<Vector2Int> candiesToClear = new List<Vector2Int>();
        switch (candyType)
        {
            case BTSCandyType.StripedHorizontal:
                candiesToClear = GetRowPattern(y);
                break;
                
            case BTSCandyType.StripedVertical:
                candiesToClear = GetColumnPattern(x);
                break;
                
            case BTSCandyType.Balloon:
                candiesToClear = GetAreaPatternExcludingCenter(x, y, 1);
                yield return StartCoroutine(SpawnEffectForCandy(candyType, x, y, candiesToClear));
                yield return new WaitForSeconds(0.15f);
                PlaySound(balloonSound);
                yield return StartCoroutine(ClearCandiesWithDelay(candiesToClear));
                yield return new WaitForSeconds(0.3f);
                yield return new WaitUntil(() => !board.AnyPotionsMoving());
                Vector2Int balloonNewPos = FindBalloonPosition(x, y);
                if (balloonNewPos.x == -1)
                {
                    activeActivations.Remove(candyKey);
                    yield break;
                }
                candiesToClear = GetAreaPattern(balloonNewPos.x, balloonNewPos.y, 1);
                yield return StartCoroutine(SpawnEffectForCandy(candyType, balloonNewPos.x, balloonNewPos.y, candiesToClear));
                yield return new WaitForSeconds(0.15f);
                PlaySound(balloonSound);
                yield return StartCoroutine(ClearCandiesWithDelay(candiesToClear));
                activeActivations.Remove(candyKey);
                yield break;
            case BTSCandyType.ColorBomb:
                if (targetColor.IsRegularMember())
                {
                    candiesToClear = GetAllOfColorPattern(targetColor);
                }
                else
                {
                    BTSCandyType randomColor = candyDatabase.GetRandomRegularCandy();
                    candiesToClear = GetAllOfColorPattern(randomColor);
                }
                break;
                
            case BTSCandyType.SuperBomb:
                candiesToClear = GetAreaPattern(x, y, 2);
                break;
                
            default:
                yield break;
        }
        
        yield return StartCoroutine(SpawnEffectForCandy(candyType, x, y, candiesToClear));
        
        yield return StartCoroutine(ClearCandiesWithDelay(candiesToClear));
        
        activeActivations.Remove(candyKey);
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
    
    private List<Vector2Int> GetAreaPatternExcludingCenter(int centerX, int centerY, int radius)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (x == centerX && y == centerY)
                    continue;
                    
                if (x >= 0 && x < board.width && y >= 0 && y < board.height)
                {
                    positions.Add(new Vector2Int(x, y));
                }
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
        
        return positions;
    }
    

    private IEnumerator SpawnExpandingBeam(Vector3 worldPos, bool isHorizontal)
    {
        if (lightstickBeamPrefab == null) yield break;
        
        worldPos.z = 11f; 
        GameObject beam = Instantiate(lightstickBeamPrefab, worldPos, Quaternion.identity);
        Transform beamTransform = beam.transform;
        
        if (!isHorizontal)
        {
            beamTransform.rotation = Quaternion.Euler(0, 0, 90f);
        }
        Vector3 startScale = beamTransform.localScale;
        if (isHorizontal)
        {
            startScale.x = 0.1f;
            startScale.y = 0.3f;
        }
        else
        {
            startScale.x = 0.1f;
            startScale.y = 0.3f;
        }
        beamTransform.localScale = startScale;
        float boardWorldWidth = (board.width - 1) * board.potionSpacingX;
        float boardWorldHeight = (board.height - 1) * board.potionSpacingY;
        SpriteRenderer spriteRenderer = beam.GetComponent<SpriteRenderer>();
        float spriteWidth = 1f;
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            spriteWidth = spriteRenderer.sprite.bounds.size.x;
        }
        float expandDuration = 0.25f;
        float elapsed = 0f;
        Vector3 targetScale = beamTransform.localScale;
        
        if (isHorizontal)
        {
            targetScale.x = (boardWorldWidth * 0.8f) / spriteWidth; 
            targetScale.y = 0.3f;
        }
        else
        {
            targetScale.x = (boardWorldHeight * 0.8f) / spriteWidth;
            targetScale.y = 0.3f;
        }
        
        while (elapsed < expandDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / expandDuration;
            beamTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        beamTransform.localScale = targetScale;
        
        yield return new WaitForSeconds(0.3f);
        Destroy(beam);
    }
    

    private IEnumerator SpawnEffectForCandy(BTSCandyType candyType, int x, int y, List<Vector2Int> targets)
    {
        Vector3 worldPos = board.GetWorldPositionForCell(x, y);
        
        switch (candyType)
        {
            case BTSCandyType.StripedHorizontal:
                StartCoroutine(SpawnExpandingBeam(worldPos, true));
                PlaySound(stripedSound);
                break;
                
            case BTSCandyType.StripedVertical:
                StartCoroutine(SpawnExpandingBeam(worldPos, false));
                PlaySound(stripedSound);
                break;
                
            case BTSCandyType.Balloon:
                if (explosionPrefab != null)
                {
                    Instantiate(explosionPrefab, worldPos, Quaternion.identity);
                }
                PlaySound(balloonSound);
                break;
                
            case BTSCandyType.ColorBomb:
                if (explosionPrefab != null)
                {
                    GameObject effect = Instantiate(explosionPrefab, worldPos, Quaternion.identity);
                    effect.transform.localScale *= 1.2f;
                }
                PlaySound(colorBombSound);
                break;
                
            case BTSCandyType.SuperBomb:
                if (explosionPrefab != null)
                {
                    GameObject explosion = Instantiate(explosionPrefab, worldPos, Quaternion.identity);
                    explosion.transform.localScale *= 1.8f;
                }
                PlaySound(superBombSound);
                break;
        }
        
        yield return new WaitForSeconds(0.3f);
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
    

    public IEnumerator HandleSpecialCombo(BTSCandyType candy1, BTSCandyType candy2, BTSCandyType baseColor1, BTSCandyType baseColor2, int x, int y)
    {
        PlaySound(comboSound);
        if (megaBombPrefab != null)
        {
            Vector3 worldPos = board.GetWorldPositionForCell(x, y);
            Instantiate(megaBombPrefab, worldPos, Quaternion.identity);
        }
        
        List<Vector2Int> candiesToClear = new List<Vector2Int>();
        if (candy1 == BTSCandyType.ColorBomb && candy2 == BTSCandyType.ColorBomb)
        {
            candiesToClear = GetEntireBoardPattern();
        }
        else if (candy1 == BTSCandyType.SuperBomb || candy2 == BTSCandyType.SuperBomb)
        {
            candiesToClear = GetAreaPattern(x, y, 3);
        }
        else if (candy1 == BTSCandyType.ColorBomb || candy2 == BTSCandyType.ColorBomb)
        {
            BTSCandyType otherCandy = (candy1 == BTSCandyType.ColorBomb) ? candy2 : candy1;
            BTSCandyType otherBaseColor = (candy1 == BTSCandyType.ColorBomb) ? baseColor2 : baseColor1;
            
            if (otherCandy == BTSCandyType.StripedHorizontal || otherCandy == BTSCandyType.StripedVertical)
            {
                candiesToClear = TransformAndActivateStriped(otherCandy, otherBaseColor);
            }
            else if (otherCandy == BTSCandyType.Balloon)
            {
                candiesToClear = TransformAndActivateBalloons(otherBaseColor);
            }
        }
        else if ((candy1 == BTSCandyType.StripedHorizontal || candy1 == BTSCandyType.StripedVertical) &&
                 (candy2 == BTSCandyType.StripedHorizontal || candy2 == BTSCandyType.StripedVertical))
        {
            candiesToClear = GetCrossPattern(x, y);
        }
        else if ((candy1 == BTSCandyType.Balloon && (candy2 == BTSCandyType.StripedHorizontal || candy2 == BTSCandyType.StripedVertical)) ||
                 (candy2 == BTSCandyType.Balloon && (candy1 == BTSCandyType.StripedHorizontal || candy1 == BTSCandyType.StripedVertical)))
        {
            candiesToClear = GetCrossPattern(x, y);
        }
        else if (candy1 == BTSCandyType.Balloon && candy2 == BTSCandyType.Balloon)
        {
            candiesToClear = GetAreaPattern(x, y, 2);
        }
        
        yield return StartCoroutine(ClearCandiesWithDelay(candiesToClear));
        
        yield return new WaitForSeconds(0.5f);
    }
    
    private List<Vector2Int> GetEntireBoardPattern()
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                positions.Add(new Vector2Int(x, y));
            }
        }
        return positions;
    }
    

    private List<Vector2Int> TransformAndActivateStriped(BTSCandyType stripedType, BTSCandyType targetColor)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        List<Potion> transformedCandies = new List<Potion>();
        
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                if (board.potionBoard[x, y] != null && board.potionBoard[x, y].isUsable && board.potionBoard[x, y].potion != null)
                {
                    Potion potion = board.potionBoard[x, y].potion.GetComponent<Potion>();
                    BTSCandyType potionType = potion.isSpecialCandy ? potion.baseColor : potion.candyType;
                    
                    if (potionType == targetColor && !potion.isSpecialCandy)
                    {
                        potion.candyType = stripedType;
                        potion.isSpecialCandy = true;
                        potion.baseColor = targetColor;
                        potion.UpdateVisualEffects();
                        transformedCandies.Add(potion);
                    }
                    else if (potionType == targetColor && potion.isSpecialCandy)
                    {
                        transformedCandies.Add(potion);
                    }
                }
            }
        }
        
        foreach (Potion striped in transformedCandies)
        {
            if (stripedType == BTSCandyType.StripedHorizontal)
            {
                positions.AddRange(GetRowPattern(striped.yIndex));
            }
            else
            {
                positions.AddRange(GetColumnPattern(striped.xIndex));
            }
        }
        
        return positions;
    }
    

    private List<Vector2Int> TransformAndActivateBalloons(BTSCandyType targetColor)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        List<Potion> transformedCandies = new List<Potion>();
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                if (board.potionBoard[x, y] != null && board.potionBoard[x, y].isUsable && board.potionBoard[x, y].potion != null)
                {
                    Potion potion = board.potionBoard[x, y].potion.GetComponent<Potion>();
                    BTSCandyType potionType = potion.isSpecialCandy ? potion.baseColor : potion.candyType;
                    
                    if (potionType == targetColor && !potion.isSpecialCandy)
                    {
                        potion.candyType = BTSCandyType.Balloon;
                        potion.isSpecialCandy = true;
                        potion.baseColor = targetColor;
                        BTSCandyData balloonData = candyDatabase.GetCandyData(BTSCandyType.Balloon);
                        if (balloonData != null && balloonData.balloonSprite != null)
                        {
                            potion.UpdateVisualEffects(balloonData.balloonSprite);
                        }
                        else
                        {
                            potion.UpdateVisualEffects();
                        }
                        
                        transformedCandies.Add(potion);
                    }
                    else if (potionType == targetColor && potion.isSpecialCandy)
                    {
                        transformedCandies.Add(potion);
                    }
                }
            }
        }
        
        foreach (Potion balloon in transformedCandies)
        {
            positions.AddRange(GetAreaPattern(balloon.xIndex, balloon.yIndex, 1));
        }
        return positions;
    }
    
    private Vector2Int FindBalloonPosition(int originalX, int originalY)
    {
        if (board.potionBoard[originalX, originalY] != null && 
            board.potionBoard[originalX, originalY].isUsable && 
            board.potionBoard[originalX, originalY].potion != null)
        {
            Potion potion = board.potionBoard[originalX, originalY].potion.GetComponent<Potion>();
            if (potion != null && potion.candyType == BTSCandyType.Balloon)
            {
                return new Vector2Int(originalX, originalY);
            }
        }
        
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                if (board.potionBoard[x, y] != null && 
                    board.potionBoard[x, y].isUsable && 
                    board.potionBoard[x, y].potion != null)
                {
                    Potion potion = board.potionBoard[x, y].potion.GetComponent<Potion>();
                    if (potion != null && potion.candyType == BTSCandyType.Balloon && 
                        Mathf.Abs(x - originalX) <= 2 && Mathf.Abs(y - originalY) <= 2)
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
        }
        
        return new Vector2Int(-1, -1);
    }
}
