using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Booster : MonoBehaviour
{
    [Header("Speed multiplier")]
    public float power;
    [Header("Duration of the boost in seconds")]
    public float duration;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<Player>().Boost(duration, power);
            Destroy(gameObject);
        }
        if (other.CompareTag("Enemy"))
        {
            other.GetComponentInParent<Enemy>().Boost(duration, power);
            Destroy(gameObject);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
