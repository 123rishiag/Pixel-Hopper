using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Inspector Variables

    [Header("Collision Variables")]
    [SerializeField] private float wallCheckDistance = 0.65f;
    [SerializeField] private float groundCheckDistance = 0.8f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Wall Variables")]
    [SerializeField] private float wallJumpDuration = 0.6f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(7f, 14f);

    [Header("Buffer Jump Variables")]
    [SerializeField] private float bufferJumpWindow = 0.25f;

    [Header("Knockback Variables")]
    [SerializeField] private float knockbackDuration = 0.7f;
    [SerializeField] private Vector2 knockbackForce = new Vector2(5f, 7f);

    [Header("Locomotion Variables")]
    [SerializeField] private float moveSpeed = 8.0f;
    [SerializeField] private float jumpForce = 14.0f;
    [SerializeField] private float doubleJumpForce = 11.0f;

    // Private Variables
    private Rigidbody2D rb;
    private Animator anim;

    private bool isWallDetected;
    private bool isWallSliding;
    private bool isWallJumping;
    private WaitForSeconds wallJumpWaitForSecondsYield;
    private IEnumerator wallJumpRoutine;
    private float bufferJumpActivatedTime;

    private bool isGrounded;
    private bool isAirBorne;
    private bool canDoubleJump;

    private bool canBeKnocked;
    private bool isKnocked;

    private WaitForSeconds knockbackWaitForSecondsYield;
    private IEnumerator knockbackRoutine;

    private float xInput;
    private float yInput;
    private bool isJumpKeyPressed;

    private bool isFacingRight;
    private int facingDirection;

    // Animator Variables
    private int isWallSlidingAnimHash = Animator.StringToHash("isWallSliding");
    private int isGroundedAnimHash = Animator.StringToHash("isGrounded");
    private int knockbackAnimHash = Animator.StringToHash("knockback");
    private int xVelocityAnimHash = Animator.StringToHash("xVelocity");
    private int yVelocityAnimHash = Animator.StringToHash("yVelocity");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        isWallDetected = false;
        isWallSliding = false;
        isWallJumping = false;
        wallJumpWaitForSecondsYield = new WaitForSeconds(wallJumpDuration);
        wallJumpRoutine = WallJumpRoutine();
        bufferJumpActivatedTime = -1f;

        isGrounded = true;
        isAirBorne = false;
        canDoubleJump = true;

        canBeKnocked = true;
        isKnocked = false;
        knockbackWaitForSecondsYield = new WaitForSeconds(knockbackDuration);
        knockbackRoutine = KnockbackRoutine();

        xInput = 0f;
        yInput = 0f;
        isJumpKeyPressed = false;

        isFacingRight = true;
        facingDirection = 1;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            KnockBack();
        }

        if (isKnocked)
        {
            return;
        }

        HandleWallSlide();
        HandleAirbone();
        HandleMovement();
        HandleJump();
        HandleFlip();
        HandleInput();

        HandleCollision();
        HandleAnimations();
    }

    private void HandleWallSlide()
    {
        isWallSliding = isWallDetected && rb.linearVelocity.y < 0f;
        float yModifier = yInput < 0f ? 1f : 0.05f;

        if (!isWallSliding)
        {
            return;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * yModifier);
    }

    private void HandleAirbone()
    {
        if (isGrounded && isAirBorne)
        {
            EnableLanding();
        }

        if (!isGrounded && !isAirBorne)
        {
            EnableAirborne();
        }
    }

    private void EnableLanding()
    {
        isAirBorne = false;
        canDoubleJump = true;

        AttemptBufferJump();
    }

    private void EnableAirborne()
    {
        isAirBorne = true;
    }

    private void HandleJump()
    {
        if (isJumpKeyPressed)
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (isWallDetected && !isGrounded)
            {
                WallJump();
            }
            else if (canDoubleJump)
            {
                DoubleJump();
            }
            RequestBufferJump();
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
    private void WallJump()
    {
        canDoubleJump = true;
        rb.linearVelocity = new Vector2(wallJumpForce.x * -facingDirection, wallJumpForce.y);
        Flip();

        wallJumpRoutine = WallJumpRoutine();
        StopCoroutine(wallJumpRoutine);
        StartCoroutine(wallJumpRoutine);
    }
    private IEnumerator WallJumpRoutine()
    {
        isWallJumping = true;
        yield return wallJumpWaitForSecondsYield;
        isWallJumping = false;
    }
    private void DoubleJump()
    {
        isWallJumping = false;
        canDoubleJump = false;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
    }
    private void RequestBufferJump()
    {
        if (isAirBorne)
        {
            bufferJumpActivatedTime = Time.time;
        }
    }
    private void AttemptBufferJump()
    {
        if (Time.time < bufferJumpActivatedTime + bufferJumpWindow)
        {
            bufferJumpActivatedTime = 0f;
            Jump();
        }
    }

    public void KnockBack()
    {
        if (!canBeKnocked)
        {
            return;
        }

        knockbackRoutine = KnockbackRoutine();
        StopCoroutine(knockbackRoutine);
        StartCoroutine(knockbackRoutine);

        anim.SetTrigger(knockbackAnimHash);

        rb.linearVelocity = new Vector2(knockbackForce.x * -facingDirection, knockbackForce.y);
    }

    private IEnumerator KnockbackRoutine()
    {
        canBeKnocked = false;
        isKnocked = true;

        yield return knockbackWaitForSecondsYield;

        canBeKnocked = true;
        isKnocked = false;
    }

    private void HandleMovement()
    {
        if (isWallDetected)
        {
            return;
        }
        if (isWallJumping)
        {
            return;
        }

        rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
    }

    private void HandleFlip()
    {
        if ((xInput < 0f && isFacingRight) || (xInput > 0f && !isFacingRight))
        {
            Flip();
        }
    }
    private void Flip()
    {
        transform.Rotate(0f, 180f, 0f);
        isFacingRight = !isFacingRight;
        facingDirection *= -1;
    }

    private void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        isJumpKeyPressed = Input.GetKeyDown(KeyCode.Space);
    }

    private void HandleCollision()
    {
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround);
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
    }

    private void HandleAnimations()
    {
        anim.SetBool(isWallSlidingAnimHash, isWallSliding);
        anim.SetBool(isGroundedAnimHash, isGrounded);
        anim.SetFloat(xVelocityAnimHash, rb.linearVelocity.x);
        anim.SetFloat(yVelocityAnimHash, rb.linearVelocity.y);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isWallDetected ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (wallCheckDistance * facingDirection), transform.position.y));

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
    }
}
