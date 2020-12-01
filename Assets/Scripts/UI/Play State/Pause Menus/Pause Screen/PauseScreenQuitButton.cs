using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseScreenQuitButton : MonoBehaviour
{
    PauseScript pauseManager;
    Button button;
    // Start is called before the first frame update
    void Start()
    {
        pauseManager = FindObjectOfType<PauseScript>();

        button = GetComponent<Button>();
        button.onClick.AddListener(LoadQuitScreen);
    }

    void LoadQuitScreen()
    {
        pauseManager.SwitchScreens("quit");
    }
}
