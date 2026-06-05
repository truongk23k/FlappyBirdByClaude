using System.Collections.Generic;
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

    const float PIPE_H    = 6f;
    const int   POOL_SIZE = 6;

    float timer;
    float lastAppliedGapSize = -1f;   // track last applied gap so collider only resizes on change
    readonly List<GameObject> pool = new List<GameObject>(POOL_SIZE);

    void Awake()
    {
        if (pipePrefab == null) return;

        // 1. Instantiate the pool
        for (int i = 0; i < POOL_SIZE; i++)
        {
            var go = Instantiate(pipePrefab);
            go.transform.position = new Vector3(1000f, 0f, 0f);  // off-screen
            pool.Add(go);
        }

        // 2. Physics warm-up: activate then deactivate every pooled pipe so that
        //    Unity registers (OnEnable) and de-registers (OnDisable) each
        //    BoxCollider2D ONCE at startup.  This eliminates the first-activation
        //    physics-broadphase spike that causes the stutter during gameplay.
        foreach (var go in pool)
        {
            go.SetActive(true);   // OnEnable: Physics2D registers 3 colliders
            go.SetActive(false);  // OnDisable: de-register; object ready in pool
        }
    }

    public void ResetSpawner() { timer = 0f; }

    public void ReturnAllToPool()
    {
        foreach (var go in pool)
        {
            if (go == null) continue;
            var m = go.GetComponent<PipeMover>();
            if (m) m.enabled = true;
            go.SetActive(false);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer -= spawnInterval;
            Spawn();
        }
    }

    GameObject GetFromPool()
    {
        foreach (var go in pool)
            if (go != null && !go.activeInHierarchy) return go;

        // Pool exhausted — should not happen in normal play
        var extra = Instantiate(pipePrefab);
        extra.transform.position = new Vector3(1000f, 0f, 0f);
        extra.SetActive(true);
        extra.SetActive(false);
        pool.Add(extra);
        return extra;
    }

    void Spawn()
    {
        if (pipePrefab == null) return;
        float y      = Random.Range(gapMinY, gapMaxY);
        var   pipeGO = GetFromPool();

        pipeGO.transform.SetPositionAndRotation(
            new Vector3(spawnX, y, 0f), Quaternion.identity);
        pipeGO.SetActive(true);

        // Re-enable ScoreTrigger
        var trigger = pipeGO.transform.Find("ScoreTrigger");
        if (trigger) trigger.gameObject.SetActive(true);

        // Re-enable mover + set speed
        var mover = pipeGO.GetComponent<PipeMover>();
        if (mover) { mover.enabled = true; mover.speed = pipeSpeed; }

        // Reposition top/bottom for gap size
        float offset = gapSize / 2f + PIPE_H / 2f;
        var top = pipeGO.transform.Find("PipeTop");
        var bot = pipeGO.transform.Find("PipeBottom");
        if (top) top.localPosition = new Vector3(0f,  offset, 0f);
        if (bot) bot.localPosition = new Vector3(0f, -offset, 0f);

        // Resize score trigger ONLY when gap size actually changed.
        // Avoids redundant Physics2D shape rebuild every spawn.
        if (trigger && !Mathf.Approximately(gapSize, lastAppliedGapSize))
        {
            var col = trigger.GetComponent<BoxCollider2D>();
            if (col) col.size = new Vector2(col.size.x, Mathf.Max(gapSize - 0.5f, 0.5f));
            lastAppliedGapSize = gapSize;
        }
    }
}
