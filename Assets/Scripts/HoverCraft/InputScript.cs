using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputScript : MonoBehaviour
{
    [Range(0,1)]
    public float horizontalInputThresh = 0.5f;
    [System.NonSerialized]
    public float forward;
    [System.NonSerialized]
    public float backward;
    [System.NonSerialized]
    public float horizontal;

    // Update is called once per frame
    void FixedUpdate()
    {
        forward = 0f;
        backward = 0f;
        if(Input.GetAxis("Vertical") > 0){
            forward = Input.GetAxis("Vertical");
        } else if(Input.GetAxis("Vertical") < 0){
            backward = -Input.GetAxis("Vertical");
        }

        // Right is positive
        if(Input.GetAxis("Horizontal") != 0){
            horizontal = Input.GetAxis("Horizontal");
        } else {
            horizontal = 0;
        }
    }
}
