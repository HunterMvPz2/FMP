using System.Collections;
using UnityEngine;

public class EnemySight : MonoBehaviour
{

    public Collider areaOfView;
    private Enemy enemy;
    public AudioSource seenEnemy;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy.seenPlayer && enemy.enemyDead == false)
        {
            enemy.enemytransform.LookAt(enemy.playerTarget);
            enemy.transform.Rotate(0, 90, 0);
        }
    }


    private void OnTriggerEnter(Collider areaOfView)
    {
        if (areaOfView.CompareTag("Player"))
        {
            enemy.seenPlayer = true;
            seenEnemy.Play();
        }
    }
}


