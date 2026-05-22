using NUnit.Framework.Constraints;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject nameOfWeapon;
    public int damage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }



    public void HoverOverWeapon()
    {
        nameOfWeapon.SetActive(true);
    }


    public void StopHoveringOverWeapon()
    {
        nameOfWeapon.SetActive(false);
    }
}
