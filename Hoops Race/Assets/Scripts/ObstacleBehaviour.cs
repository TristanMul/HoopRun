using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBehaviour : MonoBehaviour
{
    public float minimumSize;
    public GameObject BalancePiece;

    private bool open;
    private Rigidbody rb;
    private HingeJoint hj;
    private JointSpring js;

    private void Start()
    {
        rb = transform.parent.GetComponent<Rigidbody>();
        hj = transform.parent.GetComponent<HingeJoint>();
    }


    void OnTriggerStay(Collider collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Enemy"))
        {
            if (collision.CompareTag("Player"))
            {
                if (collision.GetComponentInParent<Player>().size > minimumSize)
                {
                    rb.mass = 250 - 249 * (collision.GetComponentInParent<Player>().size - minimumSize) / minimumSize;
                    js = hj.spring;
                    js.damper = 2500 - 2499 * (collision.GetComponentInParent<Player>().size - minimumSize) / minimumSize;
                    hj.spring = js;
                }
                else
                {
                    rb.mass = 250;
                    js = hj.spring;
                    js.damper = 2500;
                    hj.spring = js;
                }
            }

            if (!open)
            {
                //Change the second part ofcourse with whatever you use for the player
                //if (collision.transform.GetComponent<Player>().size > minimumSize)
                {
                    if (BalancePiece != null)
                    {
                        Destroy(BalancePiece);
                        open = true;
                    }
                }
            }
        }
    }

}
