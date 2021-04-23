using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempScaleFix : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = new Vector3(transform.localScale.x / transform.lossyScale.x, transform.localScale.y / transform.lossyScale.y, transform.localScale.z / transform.lossyScale.z);
    }
}
