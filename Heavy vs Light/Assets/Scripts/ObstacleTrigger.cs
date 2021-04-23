using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleTrigger : MonoBehaviour
{
    public float targetSize;

    private void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponentInParent<Enemy>().SetTargetSize(targetSize);
            Destroy(gameObject);
        }
    }
}
