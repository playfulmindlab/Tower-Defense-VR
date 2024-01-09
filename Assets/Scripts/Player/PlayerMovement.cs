using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField] private CharacterController playerController;

    [SerializeField] private float speed;
    [SerializeField] private float sensitivity;

    private Vector3 velocity;
    private Vector3 playerMovementInput;
    private Vector2 playerMouseInput;
    private float xRotation;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerMovementInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        playerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        MovePlayer();
        MovePlayerCamera();
    }

    void MovePlayer()
    {
        Vector3 moveVector = transform.TransformDirection(playerMovementInput);

        if (Input.GetKey(KeyCode.Space))
            velocity.y = 1f;
        if (Input.GetKey(KeyCode.LeftShift))
            velocity.y = -1f;

        playerController.Move(moveVector * speed * Time.deltaTime);
        playerController.Move(velocity * speed * Time.deltaTime);

        velocity.y = 0f;
    }

    void MovePlayerCamera()
    {
        if (Input.GetMouseButton(1))
        {
            xRotation -= playerMouseInput.y * sensitivity;
            transform.Rotate(0f, playerMouseInput.x * sensitivity, 0f);
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }
}
