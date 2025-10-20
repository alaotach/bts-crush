using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    public GameObject backgroundPanel;
    public GameObject victoryPanel;
    public GameObject losePanel;

    public int goal;
    public int moves;
    public int points;
    
    private int startingMoves; // Store initial moves for calculation

    public bool isGameEnded;

    public TMP_Text pointsText;
    public TMP_Text movesText;
    public TMP_Text goalText;
    
    public TMP_Text victoryText;
    public TMP_Text loseText;

    private void Awake()
    {
        instance = this;
        
        if (backgroundPanel != null) backgroundPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void Initialize(int _moves, int _goal)
    {
        moves = _moves;
        startingMoves = _moves; // Store initial moves
        goal = _goal;
        points = 0;
        isGameEnded = false;
    }

    void Update()
    {
        if (pointsText != null)
            pointsText.text = "Points: " + points.ToString();
        if (movesText != null)
            movesText.text = "Moves: " + moves.ToString();
        if (goalText != null)
            goalText.text = "Goal: " + goal.ToString();
    }

    public void ProcessTurn(int _pointsToGain, bool _subtractMoves)
    {
        points += _pointsToGain;
        if (_subtractMoves)
        {
            moves--;
        }
        if (points >= goal)
        {
            isGameEnded = true;
            PotionBoard.Instance.ClearAllPotions(); // Clear the board when winning
            
            int movesUsed = startingMoves - moves;
            if (victoryText != null)
            {
                victoryText.text = $"Congratulations!\nYou have won in {movesUsed} moves,\nand scored {points} points!";
            }
            
            backgroundPanel.SetActive(true);
            victoryPanel.SetActive(true);
            return;
        }
        if ( moves == 0)
        {
            isGameEnded = true;
            PotionBoard.Instance.ClearAllPotions(); // Clear the board when losing
            
            if (loseText != null)
            {
                loseText.text = $"Unfortunately you only got\n{points} points in {startingMoves} moves.\nBetter Luck Next Time!";
            }
            
            backgroundPanel.SetActive(true);
            losePanel.SetActive(true);
            return;
        }

    }

    public void WinGame()
    {
        SceneManager.LoadScene(0);
    }

    public void LoseGame()
    {
        SceneManager.LoadScene(0);
    }
}
