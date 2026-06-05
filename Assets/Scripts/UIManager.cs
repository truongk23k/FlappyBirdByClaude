using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Score Display")]
    public RectTransform scoreDisplay;
    public Sprite[] digitSprites;

    [Header("Idle Panel")]
    public GameObject idlePanel;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public RectTransform gameOverScoreDisplay;
    public RectTransform gameOverBestDisplay;

    Image[] digitImages;
    Image[] gameOverScoreImages;
    Image[] gameOverBestImages;
    const int MAX_DIGITS = 3;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        digitImages         = BuildDigitImagesFor(scoreDisplay);
        gameOverScoreImages = BuildDigitImagesFor(gameOverScoreDisplay);
        gameOverBestImages  = BuildDigitImagesFor(gameOverBestDisplay);
        UpdateScore(0);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (idlePanel)     idlePanel.SetActive(true);   // game starts in Idle
    }

    Image[] BuildDigitImagesFor(RectTransform parent)
    {
        if (parent == null) return null;
        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
        float w = (digitSprites != null && digitSprites.Length > 0) ? digitSprites[0].rect.width  : 24f;
        float h = (digitSprites != null && digitSprites.Length > 0) ? digitSprites[0].rect.height : 36f;
        var images = new Image[MAX_DIGITS];
        for (int i = 0; i < MAX_DIGITS; i++)
        {
            var go = new GameObject($"Digit_{i}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.preserveAspect = true;
            img.raycastTarget  = false;
            var le = go.AddComponent<LayoutElement>();
            le.minWidth = le.preferredWidth  = w * 2f;
            le.minHeight = le.preferredHeight = h * 2f;
            images[i] = img;
            go.SetActive(false);
        }
        return images;
    }

    void SetDigitDisplay(Image[] images, int score)
    {
        if (images == null || digitSprites == null) return;
        string s = score.ToString();
        for (int i = 0; i < images.Length; i++)
        {
            if (i < s.Length)
            {
                int d = s[i] - '0';
                images[i].sprite = (d < digitSprites.Length) ? digitSprites[d] : null;
                images[i].gameObject.SetActive(true);
            }
            else images[i].gameObject.SetActive(false);
        }
    }

    public void UpdateScore(int score) => SetDigitDisplay(digitImages, score);

    public void ShowIdle()       { if (idlePanel)     idlePanel.SetActive(true); }
    public void HideIdle()       { if (idlePanel)     idlePanel.SetActive(false); }

    public void ShowGameOver()
    {
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true);
            StartCoroutine(ScaleInPanel(gameOverPanel.transform));
        }
        AudioManager.Instance?.PlaySwoosh();
        if (ScoreManager.Instance != null)
        {
            SetDigitDisplay(gameOverScoreImages, ScoreManager.Instance.CurrentScore);
            SetDigitDisplay(gameOverBestImages,  ScoreManager.Instance.BestScore);
        }
    }

    public void HideGameOver()   { if (gameOverPanel) gameOverPanel.SetActive(false); }

    IEnumerator ScaleInPanel(Transform panel)
    {
        float elapsed = 0f;
        const float duration = 0.3f;
        panel.localScale = Vector3.zero;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            panel.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, elapsed / duration);
            yield return null;
        }
        panel.localScale = Vector3.one;
    }
}
