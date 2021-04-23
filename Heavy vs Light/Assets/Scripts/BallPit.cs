using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPit : MonoBehaviour
{
    public GameObject ball;
    public int ballAmount;
    private GameObject[] balls;

    // Start is called before the first frame update
    void Start()
    {
        balls = new GameObject[ballAmount];

    }
}
