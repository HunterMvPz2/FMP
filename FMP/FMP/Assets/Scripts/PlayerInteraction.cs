using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private PickupSystem pickupSystem;
    private PlayerMovement playerMovement;
    public GameObject TextDebug;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pickupSystem = FindAnyObjectByType<PickupSystem>();
        playerMovement = FindAnyObjectByType<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        //Shoot raycast (interaction)
        Ray ray = new Ray(playerMovement.cameraRotation.position, playerMovement.cameraRotation.forward);
        RaycastHit hit;
        Debug.DrawRay(playerMovement.cameraRotation.position, playerMovement.cameraRotation.forward * 4f, Color.green);

        //Check if raycast hit interactable 

        if (Physics.Raycast(ray, out hit, 4f))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (hit.collider.CompareTag("Interactable"))
                {
                    pickupSystem.PickupWeapon();
                    Debug.Log("works");
                }
            }
        }

        // If object has interactable tag, display text
        if (Physics.Raycast(ray, out hit, 4f))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                TextDebug.SetActive(true);
            }
            else
            {
                TextDebug.SetActive(false);
            }
        }
    }
}
