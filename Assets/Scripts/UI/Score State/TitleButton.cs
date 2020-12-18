using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleButton : MonoBehaviour
{
    Button titleButton;
    // Start is called before the first frame update
    void Start()
    {
        titleButton = GetComponent<Button>();
        titleButton.onClick.AddListener(LoadStartState);
    }

    void LoadStartState()
    {
        DontDestroy.clearDontDestroy();
        SceneManager.LoadScene("StartState");
    }
}
