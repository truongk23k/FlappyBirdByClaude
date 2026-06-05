using System.Collections;
using UnityEngine;

[System.Serializable]
public struct DifficultyStep
{
    public int   scoreThreshold;
    public float pipeSpeed;
    public float spawnInterval;
    public float gapSize;
}

public enum GameState { Idle, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public PlayerController player;
    public PipeSpawner pipeSpawner;

    [Header("Difficulty")]
    public DifficultyStep[] difficultySteps;

    GameState state;
    Scroller[] scrollers;
    int currentDifficultyIndex;
    bool canRestart;

    public GameState State => state;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (difficultySteps == null || difficultySteps.Length == 0)
            SetDefaultDifficulty();
    }

    void SetDefaultDifficulty()
    {
        difficultySteps = new[]
        {
            new DifficultyStep { scoreThreshold = 0,  pipeSpeed = 3.0f, spawnInterval = 1.8f, gapSize = 3.5f },
            new DifficultyStep { scoreThreshold = 10, pipeSpeed = 3.5f, spawnInterval = 1.6f, gapSize = 3.2f },
            new DifficultyStep { scoreThreshold = 20, pipeSpeed = 4.0f, spawnInterval = 1.4f, gapSize = 3.0f },
            new DifficultyStep { scoreThreshold = 30, pipeSpeed = 4.5f, spawnInterval = 1.3f, gapSize = 2.8f },
            new DifficultyStep { scoreThreshold = 40, pipeSpeed = 5.0f, spawnInterval = 1.2f, gapSize = 2.6f },
        };
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
        else if (state == GameState.GameOver && canRestart &&
            (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
            Restart();
    }

    public void OnScoreChanged(int score)
    {
        if (difficultySteps == null || difficultySteps.Length == 0) return;
        for (int i = difficultySteps.Length - 1; i >= 0; i--)
        {
            if (score >= difficultySteps[i].scoreThreshold)
            {
                if (i != currentDifficultyIndex)
                {
                    currentDifficultyIndex = i;
                    ApplyDifficulty(difficultySteps[i]);
                }
                break;
            }
        }
    }

    void ApplyDifficulty(DifficultyStep step)
    {
        if (pipeSpawner)
        {
            pipeSpawner.spawnInterval = step.spawnInterval;
            pipeSpawner.pipeSpeed     = step.pipeSpeed;
            pipeSpawner.gapSize       = step.gapSize;
        }
        foreach (var s in scrollers)
            if (s != null && s.gameObject.name == "Ground")
                s.speed = step.pipeSpeed;
    }

    void Restart()
    {
        canRestart = false;
        StopAllCoroutines();                    // C1: cancel ShowGameOverDelayed if still pending
        AudioManager.Instance?.StopAll();       // C3: cancel delayed die.ogg
        currentDifficultyIndex = 0;
        if (difficultySteps != null && difficultySteps.Length > 0)
            ApplyDifficulty(difficultySteps[0]);

        if (player) player.ResetPlayer();
        ScoreManager.Instance?.ResetScore();

        if (pipeSpawner) pipeSpawner.ReturnAllToPool();   // return frozen pipes to pool (no Destroy/Instantiate)
        if (pipeSpawner) pipeSpawner.ResetSpawner();

        foreach (var s in scrollers)
            if (s) s.ResetTiles();

        EnterIdle();
    }

    void EnterIdle()
    {
        state = GameState.Idle;
        if (pipeSpawner) pipeSpawner.enabled = false;
        SetScrollerMultiplier(0.5f);
        UIManager.Instance?.HideGameOver();
        UIManager.Instance?.ShowIdle();
    }

    void EnterPlaying()
    {
        state = GameState.Playing;
        if (player) player.OnStartPlaying();
        if (pipeSpawner) pipeSpawner.enabled = true;
        SetScrollerMultiplier(1f);
        UIManager.Instance?.HideIdle();
    }

    public void OnPlayerDied()
    {
        if (state == GameState.GameOver) return;
        state = GameState.GameOver;
        if (pipeSpawner) pipeSpawner.enabled = false;
        SetScrollerMultiplier(0f);
        foreach (var pipe in FindObjectsOfType<PipeMover>())
            if (pipe) pipe.enabled = false;   // freeze visually during death animation
        StartCoroutine(ShowGameOverDelayed());
    }

    IEnumerator ShowGameOverDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        if (state != GameState.GameOver) yield break;   // C2: guard against restart during delay
        if (player) player.FreezePhysics();
        UIManager.Instance?.ShowGameOver();
        canRestart = true;                              // M1: only allow restart after panel appears
    }

    void SetScrollerMultiplier(float multiplier)
    {
        if (scrollers == null) return;
        foreach (var s in scrollers)
            if (s) s.speedMultiplier = multiplier;
    }
}
