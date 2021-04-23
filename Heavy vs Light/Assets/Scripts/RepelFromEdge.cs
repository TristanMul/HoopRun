using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepelFromEdge : MonoBehaviour
{
    Rigidbody rb;
    public float repelSpeed;

    private void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            rb = other.GetComponentInParent<Rigidbody>();
            rb.velocity = new Vector3(0, rb.velocity.y, -repelSpeed);
        }
    }
}
