using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Example Level Manager showing how to dynamically change board sizes
/// The board will automatically resize and reposition itself!
/// </summary>
public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelConfig
    {
        public string levelName;
        public int width;
        public int height;
        public int moves;
        public int targetScore;
    }
    
    public List<LevelConfig> levels = new List<LevelConfig>
    {
        new LevelConfig { levelName = "Easy", width = 6, height = 8, moves = 15, targetScore = 50 },
        new LevelConfig { levelName = "Medium", width = 7, height = 9, moves = 12, targetScore = 75 },
        new LevelConfig { levelName = "Hard", width = 8, height = 10, moves = 10, targetScore = 100 },
        new LevelConfig { levelName = "Expert", width = 9, height = 11, moves = 8, targetScore = 150 },
    };
    
    private int currentLevelIndex = 0;
    
    void Start()
    {
        LoadLevel(0);
    }
    
    void Update()
    {
        // Example: Press N to load next level (for testing)
        if (Input.GetKeyDown(KeyCode.N))
        {
            LoadNextLevel();
        }
    }
    
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError($"Level index {levelIndex} is out of range!");
            return;
        }
        
        currentLevelIndex = levelIndex;
        LevelConfig level = levels[levelIndex];
        
        Debug.Log($"Loading Level: {level.levelName} ({level.width}x{level.height})");
        
        PotionBoard board = PotionBoard.Instance;
        if (board == null)
        {
            Debug.LogError("PotionBoard instance not found!");
            return;
        }
        
        board.ClearAllPotions();
        
        board.width = level.width;
        board.height = level.height;
        
        // IMPORTANT: Recalculate board transform for new dimensions
        // This is the magic - the board will automatically fit!
        board.RecalculateBoardTransform();
        
        StartCoroutine(InitializeNewLevel(level));
    }
    
    private IEnumerator InitializeNewLevel(LevelConfig level)
    {
        yield return null;
        
        // Reinitialize the board (you might need to make InitializeBoardCoroutine public)
        // For now, you can trigger it through your own initialization
        
        if (GameManager.instance != null)
        {
            GameManager.instance.Initialize(level.moves, level.targetScore);
        }
        
        Debug.Log($"Level {level.levelName} loaded successfully!");
    }
    
    public void LoadNextLevel()
    {
        int nextLevel = (currentLevelIndex + 1) % levels.Count;
        LoadLevel(nextLevel);
    }
    
    public void LoadPreviousLevel()
    {
        int prevLevel = (currentLevelIndex - 1 + levels.Count) % levels.Count;
        LoadLevel(prevLevel);
    }
}
