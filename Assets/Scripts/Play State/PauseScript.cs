using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Associated Pause UI scripts are in Assets/Scripts/UI/StartState
public class PauseScript : MonoBehaviour
{
    static bool isPaused;
    Rigidbody hoverCraftRigidBody;
    [System.NonSerialized]
    public bool pauseActive;

    public GameObject pauseScreen, quitScreen;

    /* // HoverCraft Saved Variables
    Vector3 savedVelocity;
    Vector3 savedAngularVel;*/

    string[] pauseKeys = {"p", "space", "enter", "return", "escape"};

    public bool paused
    {
        get{
            return isPaused;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        isPaused = false;
        pauseActive = false;
        hoverCraftRigidBody = FindObjectOfType<HoverCraftScript>().gameObject.GetComponent<Rigidbody>();

        SwitchScreens("closeAll");
    }

    // Update is called once per frame
    void Update()
    {
        if(pauseActive && AnyKeysPressed() && quitScreen.activeSelf == false){
            SetPause();
        }
    }

    bool AnyKeysPressed(){
        for(int i = 0; i < pauseKeys.Length; i++){
            if(Input.GetKeyDown(pauseKeys[i])){
                return true;
            }
        }
        return false;
    }

    public void SetPause()
    {
        if(isPaused){
            isPaused = false;
        } else {
            isPaused = true;
        }
        OnPauseChange();
    }

    public void SwitchScreens(string screen)
    {
        if(screen == "pause"){
            pauseScreen.SetActive(true);
            quitScreen.SetActive(false);
        } else if(screen == "quit") {
            pauseScreen.SetActive(false);
            quitScreen.SetActive(true);
        } else if(screen == "closeAll"){
            pauseScreen.SetActive(false);
            quitScreen.SetActive(false);
        }
    }

    void OnPauseChange(){
        if(isPaused){
            Time.timeScale = 0;
            SwitchScreens("pause");
        } else {
            Time.timeScale = 1;
            SwitchScreens("closeAll");
        }
    }

    /*void pauseHoverCraft(){
        if(!isPaused){
            // Unpause Game
            hoverCraftRigidBody.isKinematic = false;
            hoverCraftRigidBody.AddForce(savedVelocity, ForceMode.VelocityChange);
            hoverCraftRigidBody.AddTorque(savedAngularVel, ForceMode.VelocityChange);
        } else {
            // Pause Game
            savedVelocity = hoverCraftRigidBody.velocity;
            savedAngularVel = hoverCraftRigidBody.angularVelocity;
            hoverCraftRigidBody.isKinematic = true;
        }
    }*/
}
