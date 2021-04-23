using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceDown : MonoBehaviour
{
    Rigidbody rb;

    private void OnCollisionEnter(Collision collision)
    {
        rb = collision.gameObject.GetComponent<Rigidbody>();
    }

    private void OnCollisionStay(Collision collision)
    {
        rb.AddForce(Vector3.down * rb.mass * 25);
    }
}
