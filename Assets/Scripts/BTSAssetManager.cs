using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BTSMemberAsset
{
    public BTSCandyType memberType;
    public string displayName;
    public Sprite portraitSprite;
    public Color themeColor = Color.white;
    public AudioClip selectionSound;
    public AudioClip matchSound;
}

[CreateAssetMenu(fileName = "BTSAssetManager", menuName = "ScriptableObjects/BTS Asset Manager", order = 0)]
public class BTSAssetManager : ScriptableObject
{
    [Header("Member Assets")]
    public BTSMemberAsset[] members;

    [Header("Game Audio (Placeholders)")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    public AudioClip victoryMusic;
    public AudioClip defeatMusic;
    public AudioClip comboGood;
    public AudioClip comboGreat;
    public AudioClip comboAmazing;
    public AudioClip comboEncore;

    private Dictionary<BTSCandyType, BTSMemberAsset> memberDict;

    public void Initialize()
    {
        memberDict = new Dictionary<BTSCandyType, BTSMemberAsset>();
        foreach (var member in members)
        {
            if (!memberDict.ContainsKey(member.memberType))
            {
                memberDict.Add(member.memberType, member);
            }
        }
    }

    public BTSMemberAsset GetMemberAsset(BTSCandyType type)
    {
        if (memberDict == null || memberDict.Count == 0)
        {
            Initialize();
        }
        
        if (memberDict.TryGetValue(type, out var asset))
        {
            return asset;
        }
        return null;
    }
}
