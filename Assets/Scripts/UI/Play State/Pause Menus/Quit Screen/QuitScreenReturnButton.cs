using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitScreenReturnButton : MonoBehaviour
{
    PauseScript pauseManager;
    Button button;
    // Start is called before the first frame update
    void Start()
    {
        pauseManager = FindObjectOfType<PauseScript>();

        button = GetComponent<Button>();
        button.onClick.AddListener(ReturnToPauseScreen);
    }

    void Update(){
        if(Input.GetKeyDown("escape")){
            ReturnToPauseScreen();
        }
    }

    void ReturnToPauseScreen()
    {
        pauseManager.SwitchScreens("pause");
    }
}
