using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InfiniteLevelSelectUI : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject levelButtonPrefab;
    
    [Header("UI References")]
    [SerializeField] private Transform levelButtonContainer;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TextMeshProUGUI currentTierText;
    [SerializeField] private Button nextTierButton;
    [SerializeField] private Button prevTierButton;
    
    [Header("Settings")]
    [SerializeField] private int levelsPerPage = 15;
    [SerializeField] private int currentTier = 1;
    
    private int highestLevelReached = 1;
    private List<GameObject> levelButtons = new List<GameObject>();
    
    private void Start()
    {
        LoadProgress();
        ShowTier(currentTier);
    }
    
    private void LoadProgress()
    {
        highestLevelReached = PlayerPrefs.GetInt("HighestLevel", 1);
        currentTier = GetLevelTier(highestLevelReached);
    }
    
    private int GetLevelTier(int levelNumber)
    {
        return ((levelNumber - 1) / levelsPerPage) + 1;
    }
    
    public void ShowTier(int tier)
    {
        currentTier = tier;
        foreach (GameObject btn in levelButtons)
        {
            Destroy(btn);
        }
        levelButtons.Clear();
        if (currentTierText != null)
        {
            currentTierText.text = $"World {tier}";
        }
        int startLevel = (tier - 1) * levelsPerPage + 1;
        int endLevel = tier * levelsPerPage;
        
        for (int level = startLevel; level <= endLevel; level++)
        {
            CreateLevelButton(level);
        }
        UpdateNavigationButtons();
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
    
    private void CreateLevelButton(int levelNumber)
    {
        if (levelButtonPrefab == null || levelButtonContainer == null) return;
        
        GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonContainer);
        levelButtons.Add(buttonObj);
        Button button = buttonObj.GetComponent<Button>();
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        
        if (buttonText != null)
        {
            buttonText.text = levelNumber.ToString();
        }
        bool isUnlocked = levelNumber <= highestLevelReached;
        button.interactable = isUnlocked;
        Image buttonImage = buttonObj.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.7f);
        }
        int levelToLoad = levelNumber;
        button.onClick.AddListener(() => OnLevelButtonClicked(levelToLoad));
    }
    
    private void OnLevelButtonClicked(int levelNumber)
    {
        PlayerPrefs.SetInt("SelectedLevel", levelNumber);
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
    
    public void OnNextTierClicked()
    {
        ShowTier(currentTier + 1);
    }
    
    public void OnPrevTierClicked()
    {
        if (currentTier > 1)
        {
            ShowTier(currentTier - 1);
        }
    }
    
    private void UpdateNavigationButtons()
    {
        if (nextTierButton != null)
        {
            nextTierButton.interactable = true;
        }
        
        if (prevTierButton != null)
        {
            prevTierButton.interactable = currentTier > 1;
        }
    }
}
