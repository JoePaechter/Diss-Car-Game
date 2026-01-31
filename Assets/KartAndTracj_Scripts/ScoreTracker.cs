using UnityEngine;
using TMPro;
using JetBrains.Annotations;

public class ScoreTracker : MonoBehaviour
{
    public static ScoreTracker Instance;

    public int score = 0;
    public TextMeshProUGUI scoreText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    private void Start()
    {
        updateScore();
    }

    public void addScore(int amount)
    {
        score += amount;
        updateScore();
    }

    public void updateScore()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void resetScore()
    {
        score = 0 ;
        updateScore();
    }
}
