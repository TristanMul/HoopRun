using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockY : MonoBehaviour
{
    private void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.attachedRigidbody != null)
        {
            collision.attachedRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotation;
            //Destroy(gameObject);
        }
    }
    void OnTriggerExit(Collider collision)
    {
        if (collision.attachedRigidbody != null)
        {
            collision.attachedRigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotation;
        }
    }
}
