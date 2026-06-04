using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Score Display")]
    public RectTransform scoreDisplay;
    public Sprite[] digitSprites;       // indices 0-9 match digit value

    Image[] digitImages;
    const int MAX_DIGITS = 3;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        BuildDigitImages();
        UpdateScore(0);
    }

    void BuildDigitImages()
    {
        // Clear any existing children (safe for future restarts)
        for (int i = scoreDisplay.childCount - 1; i >= 0; i--)
            Destroy(scoreDisplay.GetChild(i).gameObject);

        float w = (digitSprites != null && digitSprites.Length > 0) ? digitSprites[0].rect.width  : 24f;
        float h = (digitSprites != null && digitSprites.Length > 0) ? digitSprites[0].rect.height : 36f;

        digitImages = new Image[MAX_DIGITS];
        for (int i = 0; i < MAX_DIGITS; i++)
        {
            var go = new GameObject($"Digit_{i}", typeof(RectTransform));
            go.transform.SetParent(scoreDisplay, false);

            var img = go.AddComponent<Image>();
            img.preserveAspect = true;
            img.raycastTarget  = false;

            var le = go.AddComponent<LayoutElement>();
            le.minWidth        = w * 2f;
            le.minHeight       = h * 2f;
            le.preferredWidth  = w * 2f;
            le.preferredHeight = h * 2f;

            digitImages[i] = img;
            go.SetActive(false);
        }
    }

    public void UpdateScore(int score)
    {
        if (digitImages == null || digitSprites == null) return;
        string s = score.ToString();
        for (int i = 0; i < digitImages.Length; i++)
        {
            if (i < s.Length)
            {
                int d = s[i] - '0';
                digitImages[i].sprite = (d < digitSprites.Length) ? digitSprites[d] : null;
                digitImages[i].gameObject.SetActive(true);
            }
            else
            {
                digitImages[i].gameObject.SetActive(false);
            }
        }
    }
}
