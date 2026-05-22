
using NUnit.Framework.Internal.Execution;
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
    private PickupSystem pickupSystem;
    public bool weaponSlotted = false;
    public Transform jumpraycastorigin;
    public bool isGrounded = true;
    private Weapon weapon;
    private PlayerHealth playerHealth;
    public Weapon hitWeapon = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pickupSystem = FindAnyObjectByType<PickupSystem>();
        playerHealth = GetComponent<PlayerHealth>();

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
        verticalRotation = Mathf.Clamp(verticalRotation, -90, 90);

        //Mouse movement (Mouse Vertical)
        {
         Vector3 mouseMovement = new Vector3(verticalRotation, 0, 0f);
        }

        if(playerHealth.health != 0)
        {
            cameraRotation.localRotation = Quaternion.Euler(verticalRotation, 0, 0f);
            rb.MoveRotation(newRotation);
        }


        //Check if you can jump
        Ray jumpRay = new Ray(jumpraycastorigin.position, Vector3.down);
        RaycastHit hitground;
        Debug.DrawRay(jumpraycastorigin.position, Vector3.down * 0.2f, Color.yellow);

        if (Physics.Raycast(jumpRay, out hitground, 0.2f))
        {
            if (hitground.collider.CompareTag("Ground"))
            {
                isGrounded = true;
            }
        }
        else
        {
            isGrounded = false;
        }


        // Physical jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true)
        {
            rb.AddRelativeForce(0, 10 * jumpPower, 0, ForceMode.Force);
        }


        //Shoot raycast (interaction)
        Ray ray = new Ray(cameraRotation.position, cameraRotation.forward);
        RaycastHit hit;
        Debug.DrawRay(cameraRotation.position, cameraRotation.forward * 4f, Color.green);

        //Check if raycast hit interactable

        if (Physics.Raycast(ray, out hit, 4f))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (hit.collider.CompareTag("Interactable"))
                {
                    pickupSystem.PickupWeapon();
                }
            }
        }



        // Player's view, checks if it views something (Weapons etc)
         hitWeapon = null;

        if (Physics.Raycast(ray, out hit, 4f))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                hitWeapon = hit.collider.GetComponentInChildren<Weapon>();
            }
        }

        // Check if hover state changed
        if (weapon != hitWeapon)
        {
            // Stop hovering old weapon
            if (weapon != null)
                weapon.StopHoveringOverWeapon();

            // Start hovering new weapon
            weapon = hitWeapon;
            if (weapon != null)
                weapon.HoverOverWeapon();
        }

    }


    void FixedUpdate()
    {
        //camera movement base
        if (playerHealth.health != 0)
        {
            Vector3 forward = cameraRotation.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = cameraRotation.right;
            right.y = 0;
            right.Normalize();

            Vector3 desiredMove = forward * moveZ + right * moveX;

            // Apply velocity directly (preserve vertical velocity)
            Vector3 velocity = desiredMove * moveSpeed;
            velocity.y = rb.linearVelocity.y; // keep vertical momentum (gravity/jumps)
            rb.linearVelocity = velocity;
        }      
    }
}
