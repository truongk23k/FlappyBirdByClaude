using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Physics")]
    public float flapForce = 7f;

    [Header("Rotation")]
    public float upAngle = 30f;
    public float downAngle = -90f;
    public float rotationMultiplier = 10f;
    public float rotationSpeed = 8f;

    [Header("Animation")]
    public Sprite[] animationFrames;
    public float frameRate = 0.1f;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    bool isDead;

    float frameTimer;
    int frameIndex;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        UpdateAnimation();

        if (isDead) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            Flap();

        UpdateRotation();
    }

    void Flap()
    {
        rb.velocity = new Vector2(0f, flapForce);
    }

    void UpdateAnimation()
    {
        if (animationFrames == null || animationFrames.Length == 0) return;

        frameTimer += Time.deltaTime;
        if (frameTimer >= frameRate)
        {
            frameTimer -= frameRate;
            frameIndex = (frameIndex + 1) % animationFrames.Length;
            spriteRenderer.sprite = animationFrames[frameIndex];
        }
    }

    void UpdateRotation()
    {
        float targetAngle = Mathf.Clamp(rb.velocity.y * rotationMultiplier, downAngle, upAngle);
        float currentAngle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.simulated = false;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        Die();
        Debug.Log("GameOver");
    }
}
