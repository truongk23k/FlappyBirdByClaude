using UnityEngine;

public class PipeMover : MonoBehaviour
{
    public float speed = 3f;
    public float destroyX = -8f;

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
        if (transform.position.x < destroyX)
            Destroy(gameObject);
    }
}
