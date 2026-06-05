// M11
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int CurrentScore { get; private set; }
    public int BestScore    { get; private set; }

    const string BEST_KEY = "BestScore";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BestScore = PlayerPrefs.GetInt(BEST_KEY, 0);
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        UIManager.Instance?.UpdateScore(0);
    }

    public void AddScore()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        CurrentScore++;
        if (CurrentScore > BestScore)
        {
            BestScore = CurrentScore;
            PlayerPrefs.SetInt(BEST_KEY, BestScore);
            PlayerPrefs.Save();
        }
        Debug.Log($"Score: {CurrentScore}");
        AudioManager.Instance?.PlayPoint();
        UIManager.Instance?.UpdateScore(CurrentScore);
        GameManager.Instance?.OnScoreChanged(CurrentScore);
    }
}
