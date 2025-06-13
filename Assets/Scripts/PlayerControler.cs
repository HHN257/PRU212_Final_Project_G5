using UnityEngine;
using UnityEngine.InputSystem; // Only if you're using the new Input System
using System.Collections; // Add this for Coroutine support

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded = true;
    private bool jumpPressed = false;

    private int attackStage = 0; // 0: Attack1, 1: Attack2, 2: Attack3

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // ---- Movement Input ----
        float move = Input.GetAxisRaw("Horizontal"); // Replace with new Input if needed

        // Move the player
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        // Flip character sprite based on direction
        if (move != 0)
            spriteRenderer.flipX = move < 0;

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
        if (Input.GetKeyDown(KeyCode.K))
        {
            animator.SetTrigger("Roll");
        }

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
    }

    private void FixedUpdate()
    {
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpPressed = false;
            isGrounded = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("Grounded", true);
        }
    }
}
