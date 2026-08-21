using UnityEngine;

public class Doofus : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7f;

    [Header("Ground Detection")]
    [SerializeField] private float groundNormalThreshold = 0.5f;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;

    private Vector3 movementInput;
    private bool isGrounded;
    private bool isRunning;
    private bool jumpedRecently;
    private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
    private static readonly int JumpHash = Animator.StringToHash("Jump");

    private void Awake()
    {
        InitializeReferences();
    }

    private void InitializeReferences()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (rb == null)
        {
            Debug.LogError("Doofus: Rigidbody is missing.");
            enabled = false;
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Doofus: Animator is missing.");
            enabled = false;
        }
    }

    private void Start()
    {
        isGrounded = false;
        UpdateAnimator();
    }

    private void Update()
    {
        ReadInput();
        HandleJump();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();

        if (rb.linearVelocity.y > 0.1f)
        {
            isGrounded = false;
        }
    }

    private void ReadInput()
    {
        if (rb == null)
        {
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        movementInput = new Vector3(horizontal, 0f, vertical);

        if (movementInput.sqrMagnitude > 1f)
        {
            movementInput.Normalize();
        }

        isRunning = movementInput.sqrMagnitude > 0.01f && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
    }

    private bool HasMovementInput()
    {
        return movementInput.sqrMagnitude > 0.01f;
    }

    private void HandleMovement()
    {
        if (!HasMovementInput())
        {
            return;
        }

        float speed = isRunning ? runSpeed : walkSpeed;
        Vector3 movement = movementInput * speed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
    }

    private void HandleRotation()
    {
        if (!HasMovementInput())
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(movementInput);
        Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        rb.MoveRotation(newRotation);
    }

    private void HandleJump()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
        {
            return;
        }

        if (!isGrounded)
        {
            return;
        }

        isGrounded = false;
        jumpedRecently = true;

        if (animator != null)
        {
            animator.SetBool(IsGroundedHash, false);
            animator.SetTrigger(JumpHash);
        }

        Vector3 velocity = rb.linearVelocity;

        if (velocity.y < 0f)
        {
            velocity.y = 0f;
            rb.linearVelocity = velocity;
        }

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckGroundCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        CheckGroundCollision(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (jumpedRecently)
        {
            jumpedRecently = false;
        }

        isGrounded = false;
    }

    private void CheckGroundCollision(Collision collision)
    {
        if (rb.linearVelocity.y > 0.1f)
        {
            isGrounded = false;
            return;
        }

        if (jumpedRecently)
        {
            isGrounded = false;
            return;
        }

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > groundNormalThreshold)
            {
                isGrounded = true;
                return;
            }
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null || rb == null)
        {
            return;
        }

        float animationSpeed = 0f;

        if (HasMovementInput())
        {
            animationSpeed = isRunning ? 1f : 0.5f;
        }

        animator.SetFloat(MoveSpeedHash, animationSpeed, 0.1f, Time.deltaTime);
        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetFloat(VerticalVelocityHash, rb.linearVelocity.y);
    }
}