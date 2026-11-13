using UnityEngine;
using UnityEngine.InputSystem;

namespace Ex4
{
    public class Simple2DPlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float jumpForce = 7f;

        [Header("Ground Check")]
        public Transform groundCheck;
        public LayerMask groundLayers;
        public bool onGround;

        private Vector2 movementInput;
        private Rigidbody rgb2d; // Changed to Rigidbody2D for 2D physics

        void Start()
        {
            rgb2d = GetComponent<Rigidbody>();
        }

        void Update()
        {
            // Check if player is grounded
            if (groundCheck != null)
            {
                onGround = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayers);
            }

            // Apply horizontal velocity while preserving current Y velocity (gravity/jumping)
            rgb2d.linearVelocity = new Vector2(movementInput.x * moveSpeed, rgb2d.linearVelocity.y);

            // Handle Jump when pressing W (movementInput.y > 0)
            if (movementInput.y > 0.5f && onGround)
            {
                Jump();
            }
        }

        void Jump()
        {
            // Apply upward velocity impulse
            rgb2d.linearVelocity = new Vector2(rgb2d.linearVelocity.x, jumpForce);
        }

        void OnMove(InputValue value)
        {
            movementInput = value.Get<Vector2>();
        }
    }
}