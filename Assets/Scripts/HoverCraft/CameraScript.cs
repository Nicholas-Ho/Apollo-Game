using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject viewer;
    HoverCraftScript.HorizontalMovementMode viewerHorMovementMode;

    void Start(){
        viewerHorMovementMode = viewer.GetComponent<HoverCraftScript>().horizontalMovementMode;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 projection = Vector3.ProjectOnPlane(viewer.transform.forward, new Vector3(0, 1, 0)).normalized;
        transform.position = viewer.transform.position;
        
        /*if(viewerHorMovementMode == HoverCraftScript.HorizontalMovementMode.Turning){
            transform.forward = projection;
        }*/
    }
}
