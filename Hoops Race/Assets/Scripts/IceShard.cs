using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceShard : MonoBehaviour
{
    public bool iceBroken;
    private bool shardBroken;
    //&& (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
    private void OnCollisionEnter(Collision collision)
    {
        if (iceBroken && !shardBroken)
        {
            GetComponent<Rigidbody>().isKinematic = false;
            shardBroken = true;
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
