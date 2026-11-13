using UnityEngine;
using UnityEngine.InputSystem;

namespace Ex4
{
    public class MovementScript : MonoBehaviour
    {
        public float moveSpeed;
        private Vector2 movementInput;
        private Rigidbody rgb2d;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            rgb2d = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            rgb2d.AddForce(movementInput * moveSpeed);
            //rgb2d.linearVelocity = movementInput * moveSpeed;
        }

        void OnMove(InputValue value)
        {
            movementInput = value.Get<Vector2>();
        }
    }
}
