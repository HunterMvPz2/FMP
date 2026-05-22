using TMPro;
using UnityEngine;

public class MagneticLootPull : MonoBehaviour
{

    private PlayerMovement playerMovement;
    private Enemy enemy;
    public int speed;
    public bool playerInRange = false;
    public Collider zone;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        enemy = FindFirstObjectByType<Enemy>();

    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange == true)
        {
            Vector3 targetPosition = new Vector3(enemy.playerTarget.position.x, transform.position.y, enemy.playerTarget.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }

        if (enemy.playerTarget)
        {
            float dist = Vector3.Distance(transform.position, enemy.playerTarget.position);
            Debug.Log(dist);
            if (dist <= 0.5)
            {
                GetComponentInChildren<MeshRenderer>().enabled = false;
            }
        }

    }



    private void OnTriggerEnter(Collider zone)
    {
        if (zone.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }
}
