using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    public GameObject pipePrefab;
    public float spawnInterval = 1.8f;
    public float spawnX = 8f;
    public float gapMinY = -1.5f;
    public float gapMaxY = 1.5f;

    // Set by GameManager.ApplyDifficulty
    public float pipeSpeed = 3f;
    public float gapSize   = 3.5f;

    const float PIPE_H = 6f;   // world height of each pipe sprite (from M6)

    float timer;

    public void ResetSpawner() { timer = 0f; }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer -= spawnInterval;
            Spawn();
        }
    }

    void Spawn()
    {
        if (pipePrefab == null) return;
        float y      = Random.Range(gapMinY, gapMaxY);
        var   pipeGO = Instantiate(pipePrefab, new Vector3(spawnX, y, 0f), Quaternion.identity);

        // Apply current pipe speed
        var mover = pipeGO.GetComponent<PipeMover>();
        if (mover) mover.speed = pipeSpeed;

        // Reposition top/bottom pipes for current gap size
        float offset = gapSize / 2f + PIPE_H / 2f;
        var top = pipeGO.transform.Find("PipeTop");
        var bot = pipeGO.transform.Find("PipeBottom");
        if (top) top.localPosition = new Vector3(0f,  offset, 0f);
        if (bot) bot.localPosition = new Vector3(0f, -offset, 0f);

        // Resize score trigger to stay within the gap
        var trigger = pipeGO.transform.Find("ScoreTrigger");
        if (trigger)
        {
            var col = trigger.GetComponent<BoxCollider2D>();
            if (col) col.size = new Vector2(col.size.x, Mathf.Max(gapSize - 0.5f, 0.5f));
        }
    }
}
