using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator inventoryAnimator;
    private PickupSystem PickupSystem;
    private PlayerMovement playerMovement;
    public Weapon weapon;
    public Collider attackZone;
    public bool swingDelay = false;
    public float swingDelayTimer = 0.5f;
    public bool playerBlocking = true;
    public AudioSource swing;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && swingDelay == false && playerBlocking == false)
        {
            Basic_slash(); 
        }
        else
        {
            inventoryAnimator.SetBool("Holding mouse", false);
        }


        if (Input.GetMouseButton(1))
        {
            Block();
            playerBlocking = true;

        }
        else
        {
            inventoryAnimator.SetBool("Holding Mouse2", false);
            playerBlocking = false;
        }
    }


    void Basic_slash()
    {
        if (playerMovement.weaponSlotted == true)
        {
            inventoryAnimator.SetBool("Holding mouse", true);
            swing.Play();
            attackZone.enabled = true;
            StartCoroutine("DelayBetweenAttack");
            swingDelay = true;
        }
    }

    void Block()
    {
        if (playerMovement.weaponSlotted == true)
        {
            inventoryAnimator.SetBool("Holding Mouse2", true);
        }
    }

    private void OnTriggerEnter(Collider attackZone)
    {
        if (attackZone.CompareTag("Enemy"))
        {
            Enemy enemy = attackZone.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                Debug.Log("attack");
                AttackEnemy(enemy); // uses equippedWeapon.damage
            }
        }
    }

    public void AttackEnemy(Enemy enemy)
    {
        if (weapon != null && enemy != null)
        {
            Debug.Log("damaged");
            enemy.TakeDamage(weapon.damage);
        }
    }


    IEnumerator DelayBetweenAttack()
    {
        yield return new WaitForSeconds(swingDelayTimer);
        swingDelay = false;
        attackZone.enabled = false;
    }
}
