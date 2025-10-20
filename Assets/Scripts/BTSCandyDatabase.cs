using UnityEngine;

/// <summary>
/// Manager for BTS candy configurations
/// ScriptableObject that holds all candy data for the game
/// </summary>
[CreateAssetMenu(fileName = "BTSCandyDatabase", menuName = "ScriptableObjects/BTS Candy Database", order = 1)]
public class BTSCandyDatabase : ScriptableObject
{
    public BTSCandyData[] candies;
    
    [Header("Gameplay Settings")]
    [Tooltip("Number of different member types to use per game (recommended: 5 for easier matches)")]
    public int activeMemberCount = 5;
    
    // Active members for current game (randomly selected from all 7)
    private BTSCandyType[] activeMembersThisGame;
    private bool isInitialized = false;
    
    /// <summary>
    /// Initialize by randomly selecting 5 members from the 7 available
    /// </summary>
    public void InitializeActiveMembersForGame()
    {
        // All 7 BTS members
        BTSCandyType[] allMembers = new BTSCandyType[]
        {
            BTSCandyType.RM,
            BTSCandyType.Jin,
            BTSCandyType.Suga,
            BTSCandyType.JHope,
            BTSCandyType.Jimin,
            BTSCandyType.V,
            BTSCandyType.Jungkook
        };
        
        activeMemberCount = Mathf.Clamp(activeMemberCount, 3, 7);
        
        // Shuffle and select first N members
        System.Collections.Generic.List<BTSCandyType> shuffled = 
            new System.Collections.Generic.List<BTSCandyType>(allMembers);
        
        // Fisher-Yates shuffle
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            BTSCandyType temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }
        
        activeMembersThisGame = new BTSCandyType[activeMemberCount];
        for (int i = 0; i < activeMemberCount; i++)
        {
            activeMembersThisGame[i] = shuffled[i];
        }
        
        isInitialized = true;
        
        Debug.Log($"🎮 Active BTS members for this game ({activeMemberCount}):");
        foreach (var member in activeMembersThisGame)
        {
            Debug.Log($"   - {member}");
        }
    }
    
    /// <summary>
    /// Get candy data by type
    /// </summary>
    public BTSCandyData GetCandyData(BTSCandyType type)
    {
        foreach (var candy in candies)
        {
            if (candy.candyType == type)
                return candy;
        }
        
        Debug.LogWarning($"Candy data not found for type: {type}");
        return null;
    }
    
    /// <summary>
    /// Get random regular candy type from active members only
    /// </summary>
    public BTSCandyType GetRandomRegularCandy()
    {
        if (!isInitialized || activeMembersThisGame == null || activeMembersThisGame.Length == 0)
        {
            InitializeActiveMembersForGame();
        }
        
        return activeMembersThisGame[Random.Range(0, activeMembersThisGame.Length)];
    }
    
    /// <summary>
    /// Get all active members for this game
    /// </summary>
    public BTSCandyType[] GetActiveMembers()
    {
        if (!isInitialized)
        {
            InitializeActiveMembersForGame();
        }
        return activeMembersThisGame;
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// Context menu to test different member counts
    /// </summary>
    [ContextMenu("Test: 3 Members (Very Easy)")]
    private void Test3Members()
    {
        activeMemberCount = 3;
        InitializeActiveMembersForGame();
    }
    
    [ContextMenu("Test: 4 Members (Easy)")]
    private void Test4Members()
    {
        activeMemberCount = 4;
        InitializeActiveMembersForGame();
    }
    
    [ContextMenu("Test: 5 Members (Balanced)")]
    private void Test5Members()
    {
        activeMemberCount = 5;
        InitializeActiveMembersForGame();
    }
    
    [ContextMenu("Test: 6 Members (Medium)")]
    private void Test6Members()
    {
        activeMemberCount = 6;
        InitializeActiveMembersForGame();
    }
    
    [ContextMenu("Test: 7 Members (Hard)")]
    private void Test7Members()
    {
        activeMemberCount = 7;
        InitializeActiveMembersForGame();
    }
#endif
    
    /// <summary>
    /// Determine what special candy to create based on match type
    /// </summary>
    public BTSCandyType GetSpecialCandyForMatch(MatchType matchType, bool isVertical = false)
    {
        switch (matchType)
        {
            case MatchType.Row4:
                return Random.value > 0.5f ? BTSCandyType.MicCandy : BTSCandyType.Lightstick;
                
            case MatchType.Column4:
                return Random.value > 0.5f ? BTSCandyType.MicCandy : BTSCandyType.Lightstick;
                
            case MatchType.Row5Plus:
            case MatchType.Column5Plus:
                return Random.value > 0.5f ? BTSCandyType.AlbumBomb : BTSCandyType.StageBomb;
                
            case MatchType.LShape:
            case MatchType.Square2x2:
                // L or T shape creates Fan Heart Bomb
                return BTSCandyType.FanHeartBomb;
                
            case MatchType.Match6Plus:
                return Random.value > 0.5f ? BTSCandyType.DynamiteCandy : BTSCandyType.ButterSlide;
                
            default:
                return BTSCandyType.RM; // Default to RM if no match
        }
    }
}
