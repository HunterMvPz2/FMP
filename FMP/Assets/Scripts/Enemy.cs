using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    public Transform enemytransform;
    public int enemyHealth = 10;
    public int enemyDamage = 5;
    public bool enemyDead = false;
    public TextMeshProUGUI damageTaken;
    public Animator numberAnimator;
    public Animator enemyDamaged;
    private PlayerMovement playerMovement;
    public float enemySpeed = 5f;
    public Transform playerTarget;
    public bool seenPlayer = false;
    float dist;
    public bool PlayerCloseToEnemy = false;
    private Rigidbody rb;
    public int enemyKnockback = 50;
    public float distanceFromPlayer;
    public bool enemyStunned = false;
    public int stunTimer;
    private EnemyLoot enemyLoot;
    public GameObject loot;
    public Animator enemyAnims;
    public GameObject goblinRagdoll;
    public AudioSource hit;
    public AudioSource death;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Log(rb);
        enemyLoot = GetComponent<EnemyLoot>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyHealth <= 0)
        {
            enemyHealth = 0;
            Die();
            goblinRagdoll.SetActive(true);
        }


        //Enemy Movement


        //Move towards player
        if (seenPlayer == true && PlayerCloseToEnemy == false && enemyStunned == false && enemyDead == false)
        {
            Vector3 targetPosition = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, enemySpeed * Time.deltaTime);
            enemyAnims.SetTrigger("Walk");
        }

        //Checks what player's distance is

        if (playerTarget)
        {
            dist = Vector3.Distance(playerTarget.position, transform.position);
        }

        //If Enemy is certain distance from player, he'll stop pursuing

        if (dist <= distanceFromPlayer)
        {
            PlayerCloseToEnemy = true;
        }
        else
        {
            PlayerCloseToEnemy = false;
        }

    }
    public int TakeDamage(int damage)
    {
        StartCoroutine(EnemyStunned());
        enemyHealth -= damage;
        damageTaken.text = damage.ToString();
        enemyDamaged.Play("Enemy_TakeDamage");
        Debug.Log("Animator: " + numberAnimator);
        Debug.Log("Animator enabled: " + numberAnimator.enabled);
        Debug.Log("GameObject active: " + numberAnimator.gameObject.activeInHierarchy);
        rb.AddRelativeForce(enemyKnockback, 0, 0, ForceMode.Force);
        hit.Play();        


        int numberPicked = Random.Range(0, 3);

        switch (numberPicked)
        {

            case 0:
                numberAnimator.Play("Damage numbers (up)", 0, 0f);
                break;

            case 1:
                numberAnimator.Play("Damage numbers (right)", 0, 0f);
                break;

            case 2:
                numberAnimator.Play("Damage numbers (left)", 0, 0f);
                break;

        }
        return damage;
    }

    public void AttackPlayer(PlayerHealth playerHealth, PlayerCombat playerCombat)
    {
        Debug.Log("AttackPlayer called");

        if (!playerCombat.playerBlocking)
        {
            Debug.Log("NOT BLOCKING → should deal damage");
            playerHealth.TakeDamage(enemyDamage);
        }
        else
        {
            Debug.Log("blocked");
            playerCombat.inventoryAnimator.SetTrigger("Attackedwhileblocking");
        }

        enemyAnims.SetTrigger("Attack");
    }

    IEnumerator EnemyStunned()
    {
        enemyStunned = true;
        yield return new WaitForSeconds(stunTimer);
        enemyStunned = false;
    }


    private bool hasDied = false;

    void Die()
    {
        if (hasDied) return;
        hasDied = true;
        enemyDead = true;
        rb.constraints = RigidbodyConstraints.None;
        Destroy(gameObject);
        Instantiate(loot, transform.position, Quaternion.identity);
        Instantiate(goblinRagdoll, enemytransform.position, Quaternion.identity);
    }
}
