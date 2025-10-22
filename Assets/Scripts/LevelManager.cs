using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (Input.GetKeyDown(KeyCode.N))
        {
            LoadNextLevel();
        }
    }
    
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            return;
        }
        
        currentLevelIndex = levelIndex;
        LevelConfig level = levels[levelIndex];
        
        Debug.Log($"Loading Level: {level.levelName} ({level.width}x{level.height})");
        
        PotionBoard board = PotionBoard.Instance;
        if (board == null)
        {
            return;
        }
        
        board.ClearAllPotions();
        
        board.width = level.width;
        board.height = level.height;
        
        board.RecalculateBoardTransform();
        
        StartCoroutine(InitializeNewLevel(level));
    }
    
    private IEnumerator InitializeNewLevel(LevelConfig level)
    {
        yield return null;
                
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
