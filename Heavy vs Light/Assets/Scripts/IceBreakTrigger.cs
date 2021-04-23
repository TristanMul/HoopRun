using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBreakTrigger : MonoBehaviour
{
    public GameObject ice1;
    public GameObject ice2;

    public float breakSize;

    private bool broken;

    private void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") && other.GetComponentInParent<Player>().size > breakSize) ||
            (other.CompareTag("Enemy") && other.GetComponentInParent<Enemy>().size > breakSize))
        {
            BreakIce();
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if ((other.CompareTag("Player") && other.GetComponentInParent<Player>().size > breakSize) ||
            (other.CompareTag("Enemy") && other.GetComponentInParent<Enemy>().size > breakSize))
        {
            BreakIce();
            Destroy(gameObject);
        }
    }

    private void BreakIce()
    {
        if (!broken)
        {
            broken = true;
            ice1.transform.GetChild(0).GetComponent<BoxCollider>().enabled = false;
            ice2.transform.GetChild(0).GetComponent<BoxCollider>().enabled = false;
            foreach (IceShard ic in ice1.GetComponentsInChildren<IceShard>())
            {
                ic.iceBroken = true;
            }
            foreach (IceShard ic in ice2.GetComponentsInChildren<IceShard>())
            {
                ic.iceBroken = true;
            }

        }
    }
}
