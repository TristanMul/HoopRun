using UnityEngine;
using System.Collections;

public class Damper : MonoBehaviour
{
    public Transform mPlatform;
    public Transform bottom;
    public Animation mAnim;

    float mMinLocalY;
    float mMaxLocalY;

    // Use this for initialization
    void Start ()
    {
        float mMaxLocalY = mPlatform.transform.position.y;
        mAnim["move"].speed = 0.0f;
        mAnim.Play();
        StartCoroutine(StartWait());
    }

    IEnumerator StartWait()
    {
        yield return new WaitForSeconds(0.1f);
        float mMinLocalY = bottom.transform.position.y;
        
    }
	
	// Update is called once per frame
	void Update ()
    {
        //float poshelper = (mMaxLocalY - (mPlatform.localPosition.y + 5.67f)) / mMaxLocalY;
        float poshelper = (mPlatform.localPosition.y + 5.67f);
        poshelper = Mathf.Clamp01(poshelper);
        mAnim["move"].normalizedTime = poshelper;
		//Debug.Log("Platform Y: " + mPlatform.localPosition.y);
    }
}
