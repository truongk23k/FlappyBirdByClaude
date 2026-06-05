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
    Vector3 spawnPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.simulated = false;
        spawnPosition = transform.position;
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

    public void OnStartPlaying()
    {
        rb.simulated = true;
        transform.rotation = Quaternion.identity;
    }

    void Flap()
    {
        rb.velocity = new Vector2(0f, flapForce);
        AudioManager.Instance?.PlayWing();   // Playing state only — Idle guard above prevents bob flap
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

    public void ResetPlayer()
    {
        isDead = false;
        rb.simulated = false;
        rb.velocity = Vector2.zero;
        transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        frameTimer = 0f;
        frameIndex = 0;
        if (animationFrames != null && animationFrames.Length > 0 && spriteRenderer != null)
            spriteRenderer.sprite = animationFrames[0];
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.simulated = false;
        AudioManager.Instance?.PlayHit();    // also schedules die.ogg after 0.3 s internally
        GameManager.Instance?.OnPlayerDied();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground") || col.gameObject.CompareTag("Pipe"))
            Die();
    }
}
