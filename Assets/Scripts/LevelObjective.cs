using UnityEngine;
using System.Collections.Generic;

public enum ObjectiveType
{
    ScoreTarget,
    CollectMemberTiles,
    ClearStageTiles,
    ComboChallenge
}

[System.Serializable]
public class ObjectiveRequirement
{
    public ObjectiveType type;
    public int targetAmount;
    public BTSCandyType targetMember; // Only for CollectMemberTiles
}

[CreateAssetMenu(fileName = "NewLevelObjective", menuName = "ScriptableObjects/Level Objective", order = 2)]
public class LevelObjective : ScriptableObject
{
    public string levelName = "Chapter 1: Practice Room";
    public int movesAllowed = 20;
    
    public List<ObjectiveRequirement> requirements;
    
    public Sprite backgroundTheme;
}
