
using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float Sens = 50f;

    private Rigidbody rb;
    private Interactable interactable;
    private float moveX;
    private float moveZ;
    private float mouseX;
    private float mouseY;
    public float jumpPower;
    private float verticalRotation;
    public Transform cameraRotation;
    public bool weaponSlotted = false;
    public Transform jumpraycastorigin;
    public bool isGrounded = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //Get input for movement
        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Vertical");

        //Get input for mouse movement
        mouseY = Input.GetAxis("Mouse Y");
        mouseX = Input.GetAxis("Mouse X");

        verticalRotation -= mouseY * Sens * Time.deltaTime;
        float horizontal = mouseX * Sens * Time.deltaTime;
        Quaternion currentHorizontalRotation = rb.rotation;
        Quaternion newRotation = currentHorizontalRotation * Quaternion.Euler(0, horizontal, 0);
        verticalRotation = Mathf.Clamp(verticalRotation, -45, 45);

        //Mouse movement (Mouse Vertical)
        Vector3 mouseMovement = new Vector3(verticalRotation, 0, 0f);

        cameraRotation.localRotation = Quaternion.Euler(verticalRotation, 0, 0f);
        rb.MoveRotation(newRotation);


        //Check if you can jump
        Ray jumpRay = new Ray(jumpraycastorigin.position, Vector3.down);
        RaycastHit hitground;
        Debug.DrawRay(jumpraycastorigin.position, Vector3.down * 1.1f, Color.yellow);

        if (Physics.Raycast(jumpRay, out hitground, 1.1f))
        {
            Debug.Log("canjump");
        }


        // Physical jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true)
        {
            rb.AddRelativeForce(0, 10, 0 * jumpPower, ForceMode.Impulse);
        }

    }


    void FixedUpdate()
    {
        //camera movement base

        Vector3 forward = cameraRotation.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = cameraRotation.right;
        right.y = 0;
        right.Normalize();

        Vector3 desiredMove = forward * moveZ + right * moveX;

        //Player Movement
        rb.AddForce(desiredMove * moveSpeed, ForceMode.Force);


    }
}
