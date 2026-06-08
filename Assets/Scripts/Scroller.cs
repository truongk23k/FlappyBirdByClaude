using UnityEngine;

public class Scroller : MonoBehaviour
{
    public float speed = 3f;
    public float tileWidth = 18f;
    public float speedMultiplier = 0.5f;  // GameManager controls this at runtime
    [SerializeField] float seamOverlap = 0.05f; // tiles overlap slightly to kill sub-pixel gap

    Transform[] tiles;
    Vector3[] initialPositions;

    void Awake()
    {
        tiles = new Transform[transform.childCount];
        initialPositions = new Vector3[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            tiles[i] = transform.GetChild(i);
            initialPositions[i] = tiles[i].position;
        }
    }

    public void ResetTiles()
    {
        if (tiles == null || initialPositions == null) return;
        for (int i = 0; i < tiles.Length; i++)
            if (tiles[i]) tiles[i].position = initialPositions[i];
    }

    void Update()
    {
        if (speedMultiplier == 0f) return;   // P2: skip entirely when stopped
        float delta = speed * speedMultiplier * Time.deltaTime;
        foreach (var tile in tiles)
            tile.position += Vector3.left * delta;

        foreach (var tile in tiles)
        {
            if (tile.position.x < -tileWidth)
            {
                float maxX = float.MinValue;
                foreach (var t in tiles)
                    if (t.position.x > maxX) maxX = t.position.x;
                tile.position = new Vector3(maxX + tileWidth - seamOverlap, tile.position.y, tile.position.z);
            }
        }
    }
}
