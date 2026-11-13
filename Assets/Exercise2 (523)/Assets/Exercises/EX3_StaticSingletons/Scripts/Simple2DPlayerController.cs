using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Simple2DPlayerController : MonoBehaviour
{
    public Rigidbody rb;
    public float speed = 5f;
    public float jumpForce = 10f;
    public float GroundDistance = 1.2f;
    float inputX;
    bool facingRight = true;
    bool isGrounded = true;

    //public Animator anim;
   // public GameObject sprite;
   // public AudioSource jumpSound;

    [Header("Better Gravity")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Coyote Time")]
    public float coyoteTime = 0.1f;
    private float coyoteTimeCounter;

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;

    void Update()
    {

        // Ground Check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, GroundDistance, LayerMask.GetMask("Ground"));

        // Coyote Time
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // Jump Buffer Input
        if (!isGrounded)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // Jump Execution (Coyote + Buffer Combined)
        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f)
        {
            //jumpSound.Play();
            rb.linearVelocity = new Vector3(rb.linearVelocity.y, jumpForce);

            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;
        }

        // Flip
        if (!facingRight && inputX > 0)
            flip();
        else if (facingRight && inputX < 0)
            flip();

        //anim.SetBool("isRunning", inputX != 0);

        // Variable Jump Height

        // Better Gravity

    }

    void OnMove()
    {
        rb.linearVelocity = new Vector3(inputX * speed, rb.linearVelocity.y, 0);
    }

    void OnJump()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector3.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);

    }
    void flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    [Space(12)]
    public GameObject DEDVFX;

    public void DED()
    {
        speed = 0f;
        jumpForce = 0f;
        rb.mass = 0f;
        rb.linearVelocity = Vector2.zero;
        // sprite.SetActive(false);
        DEDVFX.SetActive(true);
    }










    /*public float moveSpeed;
    private Vector2 movementInput;
    private Rigidbody rgb;

    [SerializeField] private float jumpForce = 10f;

    [SerializeField] bool isGrounded;

    private float GroundDistance = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rgb = GetComponent<Rigidbody>();
        Physics.gravity = new Vector3(0, -200.81f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        rgb.linearVelocity = movementInput * moveSpeed;

        if (Input.GetButtonDown("Vertical") && isGrounded)
            rgb.linearVelocity = new Vector2(rgb.linearVelocity.x, jumpForce);

        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, GroundDistance, LayerMask.GetMask("Rail"));
    }
    void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }*/



}

    /* public float moveSpeed;
    private Vector2 movementInput;
    private Rigidbody rgb2d;
    [SerializeField] int jumpheight = 4;
    [SerializeField] bool isGrounded;


   
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rgb2d = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        rgb2d.AddForce(movementInput * moveSpeed);
        rgb2d.linearVelocity = movementInput*moveSpeed;

    }
    void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();       
    }

    void OnCollisionEnter(Collision collider)
    {
        if (collider.gameObject.CompareTag("Rail"))
        {
            isGrounded = true;
            Debug.Log("I'M GROUNDING I'M GROUHNDING");

        }
        else
        {
            isGrounded = false;
        }
    }

    void OnJump()
    {
        Vector2 jumpVelocity = new Vector2 (0f, jumpheight);

        if (isGrounded == true)
        {
            rgb2d.linearVelocity = jumpVelocity;
        }
    }
}*/
