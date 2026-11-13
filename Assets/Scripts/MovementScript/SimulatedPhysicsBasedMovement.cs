using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class SimulatedPhysicsBasedMovement : MonoBehaviour
{
    Vector2 movement;

    [SerializeField] private float speed = 5f;

    [SerializeField] private Transform camTransform;

    private CharacterController charController;
    private Vector2 moveInput;

    private InputAction moveAction;

    private float gravity = -9.81f;
    private float verticalVelocity;

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        charController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (charController.isGrounded)
        {
            verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

}
