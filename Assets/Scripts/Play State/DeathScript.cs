using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScript : MonoBehaviour
{
    PauseScript pauseScript;

    GameObject meshes;
    HoverCraftScript hoverCraftScript;
    ParticleSystem explosion;
    // Start is called before the first frame update
    void Start()
    {
        pauseScript = FindObjectOfType<PauseScript>();

        meshes = gameObject.transform.GetChild(0).gameObject;
        hoverCraftScript = gameObject.GetComponent<HoverCraftScript>();
        explosion = gameObject.transform.GetChild(1).gameObject.GetComponent<ParticleSystem>();
        explosion.Stop(true);
    }

    public void Death()
    {
        pauseScript.pauseActive = false;

        explosion.Play();
        hoverCraftScript.enabled = false;
        gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        meshes.SetActive(false);

        StartCoroutine(EnterScoreState(2f));
    }

    IEnumerator EnterScoreState(float delay){
        yield return new WaitForSeconds(delay);
        
        SceneManager.LoadScene("ScoreState");
    }
}
