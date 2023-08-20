using UnityEngine;
using UnityEngine.AI;

public class CreateEnemies : MonoBehaviour
{
    public GameObject zombiePrefab;
    public int number;
    public float createRadius;
    public bool CreateOnStart = true;

    void Start()
    {
        if (CreateOnStart)
        {
            Create();
        }
    }

    void Create()
    {
        for (int i = 0; i < number; i++)
        {
            Vector3 randomPoint = this.transform.position + Random.insideUnitSphere * createRadius;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 10.0f, NavMesh.AllAreas))
            {
                Instantiate(zombiePrefab, hit.position, Quaternion.identity);
            }
            else
            {
                i--;
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (!CreateOnStart && collider.gameObject.tag == "Player")
        {
            Create();
            Destroy(gameObject);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (!CreateOnStart && collision.gameObject.tag == "Player")
        {
            Create();
            Destroy(gameObject);
        }
    }

}
