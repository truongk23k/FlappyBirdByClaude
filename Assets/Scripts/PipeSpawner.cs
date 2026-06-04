using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    public GameObject pipePrefab;
    public float spawnInterval = 1.8f;
    public float spawnX = 8f;
    public float gapMinY = -1.5f;
    public float gapMaxY = 1.5f;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer -= spawnInterval; // subtract instead of reset to avoid drift
            Spawn();
        }
    }

    void Spawn()
    {
        if (pipePrefab == null) return;
        float y = Random.Range(gapMinY, gapMaxY);
        Instantiate(pipePrefab, new Vector3(spawnX, y, 0f), Quaternion.identity);
    }
}
