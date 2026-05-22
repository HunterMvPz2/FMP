using Unity.VisualScripting;
using UnityEngine;

public class PickupSystem : MonoBehaviour
{

    private Rigidbody body;
    private Vector3 weaponPosition;
    public Transform weaponPOS;
    public Transform weaponSlot;
    public bool equipped = false;
    private PlayerMovement playerMovement;
    private GameObject item;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        weaponPosition = transform.localPosition;
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        body = GetComponent<Rigidbody>();
        item = GetComponent<GameObject>();

    }

    // Update is called once per frame
    void Update()
    {

    }



    public void PickupWeapon()
    {
        //Parents weapon to weapon slot on player

        weaponPOS.SetParent(weaponSlot);
        weaponPOS.position = weaponSlot.position;
        weaponPOS.localEulerAngles = weaponSlot.localEulerAngles;
        body.isKinematic = true;
        playerMovement.weaponSlotted = true;
        item.tag = "Untagged";

    }
}
