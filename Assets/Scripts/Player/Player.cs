using UnityEngine;

public class Player : MonoBehaviour
{
    // Inspector Variables

    [Header("Collision Variables")]
    [SerializeField] private float groundCheckDistance = 0.8f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Locomotion Variables")]
    [SerializeField] private float moveSpeed = 4.0f;
    [SerializeField] private float jumpForce = 14.0f;

    // Private Variables
    private Rigidbody2D rb;
    private Animator anim;

    private bool isGrounded;

    private float xInput;
    private bool isJumpKeyPressed;
    private bool isFacingRight;
    private int facingDirection;

    // Animator Variables
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
        isGrounded = true;

        xInput = 0f;
        isJumpKeyPressed = false;
        isFacingRight = true;
        facingDirection = 1;
    }

    private void Update()
    {
        HandleCollision();
        HandleInput();
        HandleLocomotion();
        HandleFlip();
        HandleAnimations();
    }

    private void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
    }

    private void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        isJumpKeyPressed = Input.GetKeyDown(KeyCode.Space);
    }

    private void HandleLocomotion()
    {
        float horizontalVelocity = xInput * moveSpeed;
        float verticalVelocity = (isJumpKeyPressed && isGrounded) ? jumpForce : rb.linearVelocity.y;

        rb.linearVelocity = new Vector2(horizontalVelocity, verticalVelocity);
    }

    private void HandleFlip()
    {
        if ((rb.linearVelocity.x < 0f && isFacingRight) || (rb.linearVelocity.x > 0f && !isFacingRight))
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

    private void HandleAnimations()
    {
        anim.SetFloat(xVelocityAnimHash, rb.linearVelocity.x);
        anim.SetFloat(yVelocityAnimHash, rb.linearVelocity.y);
        anim.SetBool(isGroundedAnimHash, isGrounded);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
    }
}
