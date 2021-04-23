using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIceTrigger : MonoBehaviour
{
    [Header("Given two sizes the enemy selects one")]
    public float targetSize1;
    public float targetSize2;
    [Header("The chance in percent the size 2 is chosen")]
    [Range(0, 100)]
    public float size2Chance;
    float rnd;

    private void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            rnd = Random.Range(0, 100);
            if (rnd < size2Chance)
            {
                other.GetComponentInParent<Enemy>().SetTargetSize(targetSize2, false);
            }
            else
            {
                other.GetComponentInParent<Enemy>().SetTargetSize(targetSize1, false);
            }
            Destroy(gameObject);
        }
    }
}
