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
    float flapBufferTimer;
    const float FLAP_BUFFER = 0.15f;

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

        // Buffer input before state checks so a tap that arrives during a lag frame
        // or in the same frame as Idle→Playing transition is never lost.
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            flapBufferTimer = FLAP_BUFFER;
        flapBufferTimer -= Time.deltaTime;

        var gm = GameManager.Instance;
        if (gm != null && gm.State == GameState.Idle)
        {
            UpdateBob();
            return;
        }

        if (isDead) return;

        if (flapBufferTimer > 0f)
        {
            flapBufferTimer = 0f;
            Flap();
        }

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
        if (isDead) return;   // M3: freeze animation on death; FreezePhysics() sets midflap

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
        flapBufferTimer = 0f;
        rb.simulated = false;
        rb.velocity = Vector2.zero;
        transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        frameTimer = 0f;
        frameIndex = 0;
        if (animationFrames != null && animationFrames.Length > 0 && spriteRenderer != null)
            spriteRenderer.sprite = animationFrames[0];
    }

    // Called by GameManager after the 0.5 s death delay.
    public void FreezePhysics()
    {
        rb.simulated = false;
        // Set midflap sprite per GDD ("frozen on midflap during GameOver")
        if (animationFrames != null && animationFrames.Length > 1 && spriteRenderer != null)
            spriteRenderer.sprite = animationFrames[1];
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        // Physics stays active so the bird falls naturally for the death animation.
        // GameManager.ShowGameOverDelayed() will call FreezePhysics after 0.5 s.
        AudioManager.Instance?.PlayHit();
        GameManager.Instance?.OnPlayerDied();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground") || col.gameObject.CompareTag("Pipe"))
            Die();
    }
}
