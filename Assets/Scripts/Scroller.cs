using UnityEngine;

public class Scroller : MonoBehaviour
{
    public float speed = 3f;
    public float tileWidth = 18f;
    public float speedMultiplier = 0.5f;  // GameManager controls this at runtime

    Transform[] tiles;

    void Awake()
    {
        tiles = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            tiles[i] = transform.GetChild(i);
    }

    void Update()
    {
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
                tile.position = new Vector3(maxX + tileWidth, tile.position.y, tile.position.z);
            }
        }
    }
}
