using System.Collections; // Add this for Coroutine support
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float groundCheckBuffer = 0.1f; // Buffer time for ground detection

    public bool isAutoWalking = false;
    public float autoWalkSpeed = 2f;
    public string nextSceneName = "Level1 - GreenZone";
    private bool canMove = true;
    public bool IsBlocking => animator.GetBool("Block");

    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    public float rollSpeed = 8f;
    public float rollDuration = 0.3f;

    private bool isRolling = false;
    public bool isInvincible = false;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded = true;
    private bool jumpPressed = false;
    private float lastGroundedTime;
    private bool isFalling = false;

    private int attackStage = 0; // 0: Attack1, 1: Attack2, 2: Attack3

    [Header("Block Settings")]
    public float maxBlockStamina = 100f;
    public float blockStaminaDrainRate = 30f; // How fast stamina drains while blocking
    public float blockStaminaRegenRate = 20f; // How fast stamina regenerates when not blocking
    public float minBlockStaminaToBlock = 20f; // Minimum stamina required to start blocking
    public GameObject blockBarContainer; // Reference to the block bar container
    public PlayerBlockBar blockBar; // Reference to the PlayerBlockBar component

    private float currentBlockStamina;
    private bool isCurrentlyBlocking = false; // Track if we're already blocking

    private bool isCheckingGround = false;
    private float groundCheckEndTime = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastGroundedTime = Time.time;

        // Initialize block stamina
        currentBlockStamina = maxBlockStamina;

        // Ensure block bar is properly initialized
        if (blockBar != null && blockBarContainer != null)
        {
            blockBarContainer.SetActive(true);
            blockBar.SetBlock(currentBlockStamina, maxBlockStamina);
            blockBar.OnBlockDepleted += OnBlockDepleted; // Subscribe to the event
            Debug.Log($"Block bar initialized with stamina: {currentBlockStamina}/{maxBlockStamina}");
        }
        else
        {
            Debug.LogWarning("Block bar or container not assigned in PlayerController!");
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscription
        if (blockBar != null)
        {
            blockBar.OnBlockDepleted -= OnBlockDepleted;
        }
    }

    private void OnBlockDepleted()
    {
        // Force stop blocking when stamina is depleted
        animator.SetBool("Block", false);
        Debug.Log("Block stamina depleted - forcing block to stop!");
    }

    private void Update()
    {
        if (DialogueManager.DialogueOpen)
        {
            // Stop sliding immediately
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);   // keep any upward fall speed
            animator.SetInteger("AnimState", 0);            // Idle anim (optional)
            return;                                         // skip the rest of Update
        }

        if (isCheckingGround && Time.time >= groundCheckEndTime)
        {
            isCheckingGround = false;
            if (Time.time - lastGroundedTime >= groundCheckBuffer)
            {
                isGrounded = false;
                isFalling = true;
                animator.SetBool("Grounded", false);
            }
        }

        if (isRolling) return;

        if (!canMove && !isAutoWalking) return; // Allow Update() to continue if AutoWalking

        // ---- Animation Parameters ----
        animator.SetBool("Grounded", isGrounded);
        animator.SetFloat("AirSpeedY", rb.linearVelocity.y);

        if (!isGrounded)
        {
            if (rb.linearVelocity.y > 0.1f)
            {
                animator.SetBool("Jump", true);
                animator.SetInteger("AnimState", 0);
            }
            else if (rb.linearVelocity.y < -0.1f)
            {
                animator.SetBool("Jump", false);
                animator.SetInteger("AnimState", 2); // Fall
            }
        }
        else
        {
            animator.SetBool("Jump", false);

            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                animator.SetInteger("AnimState", 1); // Run
            }
            else
            {
                animator.SetInteger("AnimState", 0); // Idle
            }
        }

        // Handle blocking with stamina system
        bool wantsToBlock = Input.GetKey(KeyCode.L);
        bool canStartBlocking = currentBlockStamina >= minBlockStaminaToBlock; // Need 20+ stamina to START blocking
        bool canContinueBlocking = currentBlockStamina > 0; // Can continue blocking until stamina hits 0

        // Allow blocking if either:
        // 1. We're not blocking and have enough stamina to start (20+)
        // 2. We're already blocking and still have some stamina left
        bool isBlocking = wantsToBlock && ((!isCurrentlyBlocking && canStartBlocking) || (isCurrentlyBlocking && canContinueBlocking));

        // Update our blocking state
        isCurrentlyBlocking = isBlocking;
        animator.SetBool("Block", isBlocking);

        // Update block stamina
        if (isBlocking)
        {
            currentBlockStamina = Mathf.Max(0f, currentBlockStamina - blockStaminaDrainRate * Time.deltaTime);
            if (currentBlockStamina <= 0f)
            {
                // Force stop blocking when stamina is fully depleted
                isCurrentlyBlocking = false;
                animator.SetBool("Block", false);
            }
        }
        else
        {
            currentBlockStamina = Mathf.Min(maxBlockStamina, currentBlockStamina + blockStaminaRegenRate * Time.deltaTime);
        }

        // Update block bar UI
        if (blockBar != null)
        {
            blockBar.SetBlock(currentBlockStamina, maxBlockStamina);
        }

        if (isRolling || !canMove) return; // Prevent all actions during roll

        if (isBlocking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // ---- Movement Input ----
        float move = Input.GetAxisRaw("Horizontal"); // Replace with new Input if needed

        // Move the player
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        // Flip character sprite based on direction
        if (move != 0)
        {
            bool facingLeft = move < 0;
            spriteRenderer.flipX = facingLeft;

            // Flip attackPoint
            if (attackPoint != null)
            {
                Vector3 attackPos = attackPoint.localPosition;
                attackPos.x = Mathf.Abs(attackPos.x) * (facingLeft ? -1 : 1);
                attackPoint.localPosition = attackPos;
            }
        }

        // ---- Jump Input ----
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            jumpPressed = true;
        }

        // ---- Attack Input ----
        if (Input.GetKeyDown(KeyCode.J))
        {
            switch (attackStage)
            {
                case 0:
                    animator.SetTrigger("Attack1");
                    break;
                case 1:
                    animator.SetTrigger("Attack2");
                    break;
                case 2:
                    animator.SetTrigger("Attack3");
                    break;
            }
            attackStage = (attackStage + 1) % 3; // Loop back to 0 after 2
        }

        // ---- Roll Input ----
        if (Input.GetKeyDown(KeyCode.K) && !isRolling)
        {
            StartCoroutine(Roll());
        }
    }

    private void FixedUpdate()
    {
        if (DialogueManager.DialogueOpen)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);   // make sure physics step is flat
            return;
        }


        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpPressed = false;
            isGrounded = false;
        }

        if (isAutoWalking)
        {
            rb.linearVelocity = new Vector2(autoWalkSpeed, rb.linearVelocity.y);
            //animator.SetBool("isWalking", true);
            spriteRenderer.flipX = false; // or true depending on direction
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            isFalling = false;
            lastGroundedTime = Time.time;
            animator.SetBool("Grounded", true);
            animator.SetBool("Jump", false);
            animator.SetInteger("AnimState", 0); // Reset to idle when landing
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!enabled || !gameObject.activeInHierarchy) return;

        if (collision.gameObject.CompareTag("Ground"))
        {
            lastGroundedTime = Time.time;

            isCheckingGround = true;
            groundCheckEndTime = Time.time + groundCheckBuffer;
        }
    }

    public void TriggerAutoWalk()
    {
        isAutoWalking = true;
        canMove = false;
        StartCoroutine(LoadNextSceneAfterDelay());
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(5f); // Wait 5 seconds

        // Start loading the scene asynchronously to prevent game freeze and rendering glitches
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Coin Collection Trigger
        if (other.CompareTag("Coin"))
        {
            Coin coin = other.GetComponent<Coin>();
            if (coin != null)
            {
                coin.Collect();
            }
        }
    }

    private IEnumerator Roll()
    {
        isRolling = true;
        animator.SetTrigger("Roll");

        if (TryGetComponent<PlayerHealth>(out var health))
        {
            health.SetTemporaryInvincibility(rollDuration);
        }

        float direction = spriteRenderer.flipX ? -1f : 1f;
        float timer = 0f;

        while (timer < rollDuration)
        {
            rb.linearVelocity = new Vector2(direction * rollSpeed, rb.linearVelocity.y);
            timer += Time.deltaTime;
            yield return null;
        }

        isRolling = false;
    }
}
