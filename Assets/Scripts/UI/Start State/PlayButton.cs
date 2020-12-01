using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    public StartStateUI uiManager;

    Button playButton;
    // Start is called before the first frame update
    void Start()
    {
        playButton = GetComponent<Button>();
        playButton.onClick.AddListener(LoadPlayState);
    }

    void LoadPlayState()
    {
        uiManager.LoadPlayState();
    }
}
