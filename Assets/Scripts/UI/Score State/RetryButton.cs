using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RetryButton : MonoBehaviour
{
    public ScoreStateUI uiManager;

    Button retryButton;
    // Start is called before the first frame update
    void Start()
    {
        retryButton = GetComponent<Button>();
        retryButton.onClick.AddListener(LoadPlayState);
    }

    void LoadPlayState()
    {
        uiManager.LoadPlayState();
    }
}
