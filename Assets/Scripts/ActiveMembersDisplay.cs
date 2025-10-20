using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays which BTS members are active in the current game
/// Optional UI component to show players which 5 members they're matching
/// </summary>
public class ActiveMembersDisplay : MonoBehaviour
{
    [Header("References")]
    public BTSCandyDatabase candyDatabase;
    public PotionBoard board;
    
    [Header("UI")]
    public TMP_Text membersText;
    public GameObject memberIconsParent;
    public Image[] memberIconSlots; // 5 slots for member portraits
    
    [Header("Settings")]
    public bool showAtStart = true;
    public float displayDuration = 5f; // How long to show at game start
    
    private void Start()
    {
        if (showAtStart)
        {
            Invoke(nameof(UpdateDisplay), 0.5f); // Wait for board initialization
            
            if (displayDuration > 0)
            {
                Invoke(nameof(HideDisplay), displayDuration);
            }
        }
    }
    
    /// <summary>
    /// Update the display to show current active members
    /// </summary>
    public void UpdateDisplay()
    {
        if (candyDatabase == null)
        {
            Debug.LogWarning("ActiveMembersDisplay: No database assigned!");
            return;
        }
        
        BTSCandyType[] activeMembers = candyDatabase.GetActiveMembers();
        
        if (membersText != null)
        {
            string memberNames = "Active Members:\n";
            foreach (var member in activeMembers)
            {
                memberNames += GetMemberEmoji(member) + " " + member.ToString() + "\n";
            }
            membersText.text = memberNames;
        }
        
        if (memberIconSlots != null && memberIconSlots.Length > 0)
        {
            for (int i = 0; i < memberIconSlots.Length; i++)
            {
                if (i < activeMembers.Length && memberIconSlots[i] != null)
                {
                    BTSCandyData candyData = candyDatabase.GetCandyData(activeMembers[i]);
                    if (candyData != null && candyData.sprite != null)
                    {
                        memberIconSlots[i].sprite = candyData.sprite;
                        memberIconSlots[i].gameObject.SetActive(true);
                    }
                }
                else if (memberIconSlots[i] != null)
                {
                    memberIconSlots[i].gameObject.SetActive(false);
                }
            }
        }
        
        if (memberIconsParent != null)
        {
            memberIconsParent.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide the member display
    /// </summary>
    public void HideDisplay()
    {
        if (memberIconsParent != null)
        {
            memberIconsParent.SetActive(false);
        }
        
        if (membersText != null && membersText.gameObject != memberIconsParent)
        {
            membersText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Show the member display
    /// </summary>
    public void ShowDisplay()
    {
        UpdateDisplay();
    }
    
    /// <summary>
    /// Get emoji for each member
    /// </summary>
    private string GetMemberEmoji(BTSCandyType member)
    {
        switch (member)
        {
            case BTSCandyType.RM: return "💜"; // Leader
            case BTSCandyType.Jin: return "💗"; // Worldwide Handsome
            case BTSCandyType.Suga: return "🖤"; // Savage
            case BTSCandyType.JHope: return "🧡"; // Sunshine
            case BTSCandyType.Jimin: return "💛"; // Angel
            case BTSCandyType.V: return "💚"; // Taehyung
            case BTSCandyType.Jungkook: return "💙"; // Golden Maknae
            default: return "⭐";
        }
    }
    
    /// <summary>
    /// Context menu option to test display
    /// </summary>
    [ContextMenu("Test Display")]
    private void TestDisplay()
    {
        UpdateDisplay();
    }
}
