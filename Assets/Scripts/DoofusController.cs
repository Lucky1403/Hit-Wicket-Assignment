using UnityEngine;

public class Doofus : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    private Vector3 movementInput;
    private void Awake()
    {
        if(rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        if(animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }
   
    private void Start()
    {
        
    }

    private void Update()
    {
        ReadInput();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        Move();
        Rotate();
        UpdateGroundedState();
    }
    private void ReadInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        movementInput = new Vector3(horizontal, 0f, vertical).normalized;
    }

    private void Move()
    {
        Vector3 velocity = movementInput * moveSpeed;

        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }

    private void Rotate()
    {
        if (movementInput.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(movementInput);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    private void UpdateAnimation()
    {
        float movementAmount = movementInput.magnitude;

        animator.SetFloat("MoveSpeed", movementAmount, 0.1f, Time.deltaTime);
    }

    private void UpdateGroundedState()
    {
        bool grounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        animator.SetBool("IsGrounded", grounded);
    }
}
