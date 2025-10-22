using UnityEngine;

[CreateAssetMenu(fileName = "BTSCandyDatabase", menuName = "ScriptableObjects/BTS Candy Database", order = 1)]
public class BTSCandyDatabase : ScriptableObject
{
    public BTSCandyData[] candies;
    public int activeMemberCount = 5;
    
    private BTSCandyType[] activeMembersThisGame;
    private bool isInitialized = false;

    public void InitializeActiveMembersForGame()
    {
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
        
        System.Collections.Generic.List<BTSCandyType> shuffled = 
            new System.Collections.Generic.List<BTSCandyType>(allMembers);
        
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
    }
 
    public BTSCandyData GetCandyData(BTSCandyType type)
    {
        foreach (var candy in candies)
        {
            if (candy.candyType == type)
                return candy;
        }        
        return null;
    }
    
    public BTSCandyType GetRandomRegularCandy()
    {
        if (!isInitialized || activeMembersThisGame == null || activeMembersThisGame.Length == 0)
        {
            InitializeActiveMembersForGame();
        }
        
        return activeMembersThisGame[Random.Range(0, activeMembersThisGame.Length)];
    }
    
    public BTSCandyType[] GetActiveMembers()
    {
        if (!isInitialized)
        {
            InitializeActiveMembersForGame();
        }
        return activeMembersThisGame;
    }
    
#if UNITY_EDITOR

    private void Test3Members()
    {
        activeMemberCount = 3;
        InitializeActiveMembersForGame();
    }
    
    private void Test4Members()
    {
        activeMemberCount = 4;
        InitializeActiveMembersForGame();
    }
    
    private void Test5Members()
    {
        activeMemberCount = 5;
        InitializeActiveMembersForGame();
    }
    
    private void Test6Members()
    {
        activeMemberCount = 6;
        InitializeActiveMembersForGame();
    }
    
    private void Test7Members()
    {
        activeMemberCount = 7;
        InitializeActiveMembersForGame();
    }
#endif
    

    public BTSCandyType GetSpecialCandyForMatch(MatchType matchType)
    {
        switch (matchType)
        {
            case MatchType.Match4Horizontal:
                return BTSCandyType.StripedVertical;
                
            case MatchType.Match4Vertical:
                return BTSCandyType.StripedHorizontal;
                
            case MatchType.TShape:
            case MatchType.LShape:
                return BTSCandyType.Balloon;
                
            case MatchType.Match5:
                return BTSCandyType.ColorBomb;
                
            case MatchType.Match6Plus:
                return BTSCandyType.SuperBomb;
                
            default:
                return BTSCandyType.RM;
        }
    }
}
