using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rampSlowTrigger: MonoBehaviour
{
    Player p;
    Enemy e;
    Rigidbody rb;
    float force;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            p = other.GetComponent<Player>();
            rb = other.GetComponent<Rigidbody>();
            p.maxSpeed *= 0.5f;
        }
        if (other.CompareTag("Enemy"))
        {
            e = other.GetComponent<Enemy>();
            rb = other.GetComponent<Rigidbody>();
            e.maxSpeed *= 0.5f;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            force = rb.mass * 15;
            p.maxSpeed *= 0.5f;
            rb.AddForce(Vector3.back * force);
        }
        if (other.CompareTag("Enemy"))
        {
            force = rb.mass * 15;
            e.maxSpeed *= 0.5f;
            rb.AddForce(Vector3.back * force);
        }
    }


}
