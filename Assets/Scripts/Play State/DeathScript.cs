using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScript : MonoBehaviour
{
    PauseScript pauseScript;
    AudioSource explosionAudio;

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
        explosionAudio = DontDestroy.instances[0].GetComponents<AudioSource>()[1];
        explosion.Stop(true);
    }

    public void Death()
    {
        pauseScript.pauseActive = false;

        explosion.Play();
        explosionAudio.Play();
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
