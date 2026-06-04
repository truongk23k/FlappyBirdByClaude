using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int CurrentScore { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddScore()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        CurrentScore++;
        Debug.Log($"Score: {CurrentScore}");
        UIManager.Instance?.UpdateScore(CurrentScore);
    }
}
