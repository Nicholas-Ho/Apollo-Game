using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathScript : MonoBehaviour
{
    GameObject meshes;
    HoverCraftScript hoverCraftScript;
    ParticleSystem explosion;
    // Start is called before the first frame update
    void Start()
    {
        meshes = gameObject.transform.GetChild(0).gameObject;
        hoverCraftScript = gameObject.GetComponent<HoverCraftScript>();
        explosion = gameObject.transform.GetChild(1).gameObject.GetComponent<ParticleSystem>();
        explosion.Stop(true);
    }

    void OnTriggerEnter()
    {
        explosion.Play();
        hoverCraftScript.enabled = false;
        gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        meshes.SetActive(false);
    }

    IEnumerator ExecuteAfterTime(float time){
        yield return new WaitForSeconds(time);
        
        // Changing screens here
    }
}
