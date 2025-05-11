using UnityEngine;

public class SpawnTest : MonoBehaviour
{
    public GameObject prefabToSpawn; 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnPrefab();
        }
    }

    void SpawnPrefab()
    {
        Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
    }
}