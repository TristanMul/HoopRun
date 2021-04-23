using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockBridge : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.localEulerAngles.x >= 89)
        {
            transform.localEulerAngles = new Vector3(89, 0, 0);
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}
