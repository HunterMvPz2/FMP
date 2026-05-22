// DamageField.cs - put this on the Damage field child object
using System.Collections;
using UnityEngine;

public class DamageField : MonoBehaviour
{
    private Enemy enemy;
    public Collider enemyAttackZone;
    public bool attackDelay = false;
    public int attackDelayTimer;

    void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter(Collider attackZone)
    {
        if (attackZone.CompareTag("Player") && !enemy.enemyStunned)
        {
            PlayerHealth playerHealth = attackZone.GetComponentInParent<PlayerHealth>();
            PlayerCombat playerCombat = attackZone.GetComponentInParent<PlayerCombat>();

            if (playerHealth != null && playerCombat != null && !attackDelay && enemy.enemyDead == false)
            {
                enemy.AttackPlayer(playerHealth, playerCombat);
                attackDelay = true;
                enemyAttackZone.enabled = false;
                StartCoroutine(DelayBetweenAttack());
            }
        }
    }

    IEnumerator DelayBetweenAttack()
    {
        yield return new WaitForSeconds(attackDelayTimer);
        attackDelay = false;
        enemyAttackZone.enabled = true;
    }
}