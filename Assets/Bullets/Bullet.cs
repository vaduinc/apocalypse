using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Call the Destroy method on the gameObject property after 5 seconds
        Destroy(gameObject, 5f);
    }

}
