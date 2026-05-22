using UnityEngine;

public class EnemyLoot : MonoBehaviour
{
    private Enemy enemy;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemy = GetComponent<Enemy>();
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Disable all colliders
            foreach (Collider col in GetComponentsInChildren<Collider>())
                col.enabled = false;

            // Disable all meshes
            foreach (MeshRenderer mesh in GetComponentsInChildren<MeshRenderer>())
                mesh.enabled = false;
        }
    }
}
