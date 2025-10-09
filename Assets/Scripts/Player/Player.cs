using UnityEngine;

public class Player : MonoBehaviour
{
    // Inspector Variables

    [Header("Collision Variables")]
    [SerializeField] private float wallCheckDistance = 0.65f;
    [SerializeField] private float groundCheckDistance = 0.8f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Locomotion Variables")]
    [SerializeField] private float moveSpeed = 8.0f;
    [SerializeField] private float jumpForce = 14.0f;
    [SerializeField] private float doubleJumpForce = 11.0f;

    // Private Variables
    private Rigidbody2D rb;
    private Animator anim;

    private bool isWallDetected;
    private bool isWallSliding;
    private bool isGrounded;
    private bool isAirBorne;
    private bool canDoubleJump;

    private float xInput;
    private float yInput;
    private bool isJumpKeyPressed;

    private bool isFacingRight;
    private int facingDirection;

    // Animator Variables
    private int isWallSlidingAnimHash = Animator.StringToHash("isWallSliding");
    private int isGroundedAnimHash = Animator.StringToHash("isGrounded");
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
        isGrounded = true;
        isAirBorne = false;
        canDoubleJump = true;

        xInput = 0f;
        yInput = 0f;
        isJumpKeyPressed = false;

        isFacingRight = true;
        facingDirection = 1;
    }

    private void Update()
    {
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
    }

    private void EnableAirborne()
    {
        isAirBorne = true;
    }

    private void HandleMovement()
    {
        if (isWallDetected)
        {
            return;
        }

        rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
    }

    private void HandleJump()
    {
        if (isJumpKeyPressed)
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (canDoubleJump)
            {
                DoubleJump();
            }
        }
    }
    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
    private void DoubleJump()
    {
        canDoubleJump = false;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
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
