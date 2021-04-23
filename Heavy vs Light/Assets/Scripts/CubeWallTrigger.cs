using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeWallTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            foreach(Rigidbody rb in transform.parent.GetComponentsInChildren<Rigidbody>())
            {
                rb.mass = 1e-7f;
                //rb.gameObject.tag = "Default";
            }
        }
    }
}
