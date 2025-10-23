using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class SimpleLevelGridManager : MonoBehaviour
{
    public GameObject levelButtonPrefab;

    public Transform levelGridContainer;

    public int totalLevels = 50;

    public string gameSceneName = "GameScene";
    
    public bool allLevelsUnlocked = true;
    
    public int levelsPerPage = 0; 
    
    public int currentPage = 1;
    
    public TMP_Text pageIndicatorText;
    public Button nextPageButton;
    public Button prevPageButton;
    
    private int maxUnlockedLevel = 1;
    
    void Start()
    {
        LoadProgress();
        CreateLevelButtons();
        UpdatePageIndicator();
    }
    
    void LoadProgress()
    {
        maxUnlockedLevel = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
    }
    
    void CreateLevelButtons()
    {
        if (levelButtonPrefab == null)
        {
            return;
        }
        
        if (levelGridContainer == null)
        {
            return;
        }
        
        foreach (Transform child in levelGridContainer)
        {
            Destroy(child.gameObject);
        }
        
        int startLevel = 1;
        int endLevel = totalLevels;
        
        if (levelsPerPage > 0)
        {
            startLevel = ((currentPage - 1) * levelsPerPage) + 1;
            endLevel = Mathf.Min(startLevel + levelsPerPage - 1, totalLevels);
        }
        
        for (int i = startLevel; i <= endLevel; i++)
        {
            int levelNumber = i; 
            GameObject buttonObj = Instantiate(levelButtonPrefab, levelGridContainer);
            buttonObj.name = $"LevelButton_{levelNumber}";
            
            Button button = buttonObj.GetComponent<Button>();
            if (button == null)
            {
                continue;
            }
            
            TMP_Text levelText = buttonObj.GetComponentInChildren<TMP_Text>();
            if (levelText == null)
            {
                Transform textTransform = buttonObj.transform.Find("LevelNumberText");
                if (textTransform != null)
                {
                    levelText = textTransform.GetComponent<TMP_Text>();
                }
            }
            
            Transform lockIcon = buttonObj.transform.Find("LockIcon");
            
            if (levelText != null)
            {
                levelText.text = levelNumber.ToString();
            }
            
            bool isUnlocked = allLevelsUnlocked || (levelNumber <= maxUnlockedLevel);
            
            if (isUnlocked)
            {
                button.interactable = true;
                
                if (lockIcon != null)
                {
                    lockIcon.gameObject.SetActive(false);
                }
                
                if (levelText != null)
                {
                    levelText.color = Color.white;
                }
                
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => StartLevel(levelNumber));
            }
            else
            {
                button.interactable = false;
                
                if (lockIcon != null)
                {
                    lockIcon.gameObject.SetActive(true);
                }
                
                if (levelText != null)
                {
                    levelText.color = new Color(1, 1, 1, 0.3f);
                }
            }
        }
        
        if (levelsPerPage > 0)
        {
            if (prevPageButton != null)
            {
                prevPageButton.interactable = currentPage > 1;
            }
            
            if (nextPageButton != null)
            {
                int maxPages = Mathf.CeilToInt((float)totalLevels / levelsPerPage);
                nextPageButton.interactable = currentPage < maxPages;
            }
        }
    }
    
    public void StartLevel(int levelNumber)
    {
        PlayerPrefs.SetInt("CurrentLevel", levelNumber);
        PlayerPrefs.Save();
        int savedLevel = PlayerPrefs.GetInt("CurrentLevel");
        SceneManager.LoadScene(gameSceneName);
    }
    
    public static void UnlockNextLevel()
    {
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
        if (currentLevel >= maxUnlocked)
        {
            PlayerPrefs.SetInt("MaxUnlockedLevel", currentLevel + 1);
            PlayerPrefs.Save();
        }
    }
    
    public void NextPage()
    {
        int maxPages = Mathf.CeilToInt((float)totalLevels / levelsPerPage);
        if (currentPage < maxPages)
        {
            currentPage++;
            CreateLevelButtons();
            UpdatePageIndicator();
        }
    }
    
    public void PrevPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            CreateLevelButtons();
            UpdatePageIndicator();
        }
    }
    
    void UpdatePageIndicator()
    {
        if (pageIndicatorText != null && levelsPerPage > 0)
        {
            int maxPages = Mathf.CeilToInt((float)totalLevels / levelsPerPage);
            pageIndicatorText.text = $"World {currentPage} / {maxPages}";
        }
    }
    
    public void UnlockAllLevels()
    {
        PlayerPrefs.SetInt("MaxUnlockedLevel", totalLevels);
        PlayerPrefs.Save();
        CreateLevelButtons();
    }
    
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("MaxUnlockedLevel");
        PlayerPrefs.DeleteKey("CurrentLevel");
        PlayerPrefs.Save();
        maxUnlockedLevel = 1;
        CreateLevelButtons();
    }
    
    public void UnlockNext5()
    {
        int current = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
        PlayerPrefs.SetInt("MaxUnlockedLevel", Mathf.Min(current + 5, totalLevels));
        PlayerPrefs.Save();
        LoadProgress();
        CreateLevelButtons();
    }
}
