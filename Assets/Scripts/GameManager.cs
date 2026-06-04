using UnityEngine;

public enum GameState { Idle, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public PlayerController player;
    public PipeSpawner pipeSpawner;

    GameState state;
    Scroller[] scrollers;
    public GameState State => state;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        scrollers = FindObjectsOfType<Scroller>();
        EnterIdle();
    }

    void Update()
    {
        if (state == GameState.Idle &&
            (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
            EnterPlaying();
    }

    void EnterIdle()
    {
        state = GameState.Idle;
        if (pipeSpawner) pipeSpawner.enabled = false;
        SetScrollerMultiplier(0.5f);
        UIManager.Instance?.HideGameOver();
    }

    void EnterPlaying()
    {
        state = GameState.Playing;
        if (player) player.OnStartPlaying();
        if (pipeSpawner) pipeSpawner.enabled = true;
        SetScrollerMultiplier(1f);
    }

    public void OnPlayerDied()
    {
        if (state == GameState.GameOver) return;
        state = GameState.GameOver;
        if (pipeSpawner) pipeSpawner.enabled = false;
        SetScrollerMultiplier(0f);
        foreach (var pipe in FindObjectsOfType<PipeMover>())
            if (pipe) pipe.enabled = false;
        UIManager.Instance?.ShowGameOver();
    }

    void SetScrollerMultiplier(float multiplier)
    {
        if (scrollers == null) return;
        foreach (var s in scrollers)
            if (s) s.speedMultiplier = multiplier;
    }
}
