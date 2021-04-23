using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public CharacterCustomization characterCustomization;
    [Range(0.0f, 100.0f)]
    public float size;

    private CapsuleCollider col;
    private Rigidbody rb;
    public Animator animator;
    public Animation run;
    public Animation push;

    public float force;
    public float maxSpeed;
    float animatorSpeed;
    bool pushing;
    GameObject pushedObject = null;

    bool boosted;
    float boostStart;
    float boostTime = 5;
    float boostSpeed = 15;
    float boostForce = 30;

    public bool stopMoving;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Keep the size contained between 0 and 100
        if (size > 100)
        {
            size = 100;
        }
        if (size < 0)
        {
            size = 0;
        }
        
        //Set the fat of the player based on the given size
        characterCustomization.SetBodyShape(CharacterCustomization.BodyShapeType.Fat, size * 2.5f);
        //Set the muscles of the player based on the given size
        characterCustomization.SetBodyShape(CharacterCustomization.BodyShapeType.Muscles, size);
        //Set the thinness of the player based on the given size
        characterCustomization.SetBodyShape(CharacterCustomization.BodyShapeType.Thin, 100 - size);
        //Set the height of the player based on the given size
        characterCustomization.SetHeight(size / 250);

        //Set the size and relative position of the capsule collider based on the size of the player
        col.center = new Vector3(0, -0.075f + 0.325f * (size * 0.01f), 0);
        col.height = 1.85f + 0.65f * (size / 100);

        //Set a max speed based on th esize of the player
        maxSpeed = 12 - 4.5f * (size * 0.01f);
        //Set the mass of the player based on the size of the player
        rb.mass = Mathf.Pow(2, size / 10);
        //Scale the force based on the mass so that it keeps up
        if (!stopMoving)
        {
            force = rb.mass * 30;
        }
        else
        {
            force = 0;
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        //Is the player boosted
        if (boosted)
        {
            maxSpeed = boostSpeed;
            force = rb.mass * boostForce;
            if (Time.time > boostStart + boostTime)
            {
                boosted = false;
            }
        }

        //Give a force propelling the player forwards
        if (!stopMoving)
        {
            rb.AddForce(Vector3.forward * force);
        }

        //Set the animation speed based on the velocity of the player
        if (rb.velocity.z >= 0)
        {
            animatorSpeed = (1.1f - 0.5f * (size * 0.01f)) * rb.velocity.z * 0.1f;
            if (animatorSpeed < 0)
            {
                animatorSpeed = 0;
            }
            animator.speed = animatorSpeed;
        }
        else
        {
            animator.speed = 0;
        }
        if (pushing && rb.velocity.z < 0.5f)
        {
            animator.speed = 0.2f * (size * 0.04f) + (1.0f - 0.6f * (size * 0.03f)) * rb.velocity.z * 0.1f;
            print("scaling speed");
        }




        //If the player surpasses the max speed
        if (rb.velocity.z > maxSpeed)
        {
            //Set the speed to the max speed value
            rb.velocity = new Vector3(0, rb.velocity.y, maxSpeed);
        }
        //animator.SetFloat("Velocity", rb.velocity.z);
        animator.SetBool("Colliding with obstacle", pushing);
        //Debug.Log(animator.GetFloat("Velocity"));
        //Debug.Log(animator.GetBool("Colliding with obstacle"));
    }

    public void Boost(float duration, float power)
    {
        boosted = true;
        boostStart = Time.time;
        boostSpeed = 10 * power;
        boostForce = 15 * power;
        boostTime = duration;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Push Obs") && pushedObject == null)
        {
            pushing = true;
            pushedObject = collision.gameObject;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (pushedObject == null)
        {
            if (collision.gameObject.CompareTag("Push Obs"))
            {
                pushing = true;
                pushedObject = collision.gameObject;
            }
        }
    }

    public void SetPushing(bool push)
    {
        pushing = push;
    }

}
