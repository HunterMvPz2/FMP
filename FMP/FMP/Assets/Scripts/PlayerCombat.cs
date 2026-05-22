using Unity.VisualScripting;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator inventoryAnimator;
    private PickupSystem PickupSystem;
    private PlayerMovement playerMovement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Basic_slash();
            inventoryAnimator.SetBool("Holding mouse", true);
        }
        else
        {
            inventoryAnimator.SetBool("Holding mouse", false);
        }
    }


    void Basic_slash()
    {
        if(playerMovement.weaponSlotted == true)
        {
          inventoryAnimator.SetTrigger("Basic slash");
        }

    }
}
