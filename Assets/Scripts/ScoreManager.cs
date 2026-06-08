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

    void OnApplicationPause(bool paused) { if (paused) PlayerPrefs.Save(); }
    void OnApplicationQuit()             { PlayerPrefs.Save(); }

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
            // Save() deferred to app pause/quit to avoid synchronous disk I/O on the main thread
        }
        AudioManager.Instance?.PlayPoint();
        UIManager.Instance?.UpdateScore(CurrentScore);
        GameManager.Instance?.OnScoreChanged(CurrentScore);
    }
}
