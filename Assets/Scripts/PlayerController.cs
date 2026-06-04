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

    [Header("Idle Bob")]
    public float bobAmplitude = 0.2f;
    public float bobFrequency = 2f;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    bool isDead;

    float frameTimer;
    int frameIndex;
    float idleStartY;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.simulated = false;   // held inactive until GameManager enters Playing state
        idleStartY = transform.position.y;
    }

    void Update()
    {
        UpdateAnimation();

        var gm = GameManager.Instance;
        if (gm != null && gm.State == GameState.Idle)
        {
            UpdateBob();
            return;
        }

        if (isDead) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            Flap();

        UpdateRotation();
    }

    void UpdateBob()
    {
        float y = idleStartY + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }

    // Called by GameManager when transitioning Idle -> Playing.
    public void OnStartPlaying()
    {
        rb.simulated = true;
        transform.rotation = Quaternion.identity;
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

    // Called by collision handlers and by GameManager if needed.
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.simulated = false;
        GameManager.Instance?.OnPlayerDied();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground") || col.gameObject.CompareTag("Pipe"))
            Die();
    }
}
