using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        ScoreManager.Instance?.AddScore();
        gameObject.SetActive(false);  // prevent double-count; destroyed with parent PipePair
    }
}
