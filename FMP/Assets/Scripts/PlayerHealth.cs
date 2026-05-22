using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health;
    public bool playerIsDead = false;
    public Animator playerCamera;
    public Animator playerDamaged;
    public Animator playerStatus;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            health = 0;
            playerIsDead = true;
            PlayerDeath("Player is dead");
            playerDamaged.SetBool("PlayerAlive", true);
        }

        playerStatus.SetInteger("Health", health);
        int currentAnimatorHealth = playerStatus.GetInteger("Health");
    }


    public int TakeDamage(int damage)
    {
        health -= damage;
        playerDamaged.SetTrigger("TakeDamage");
        return health;
    }

    public string PlayerDeath(string PlayerDied)
    {
        playerCamera.Play("Player death");
        playerDamaged.Play("Player_Death");
        return PlayerDied;
    }
}
