using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuitScreenQuitButton : MonoBehaviour
{
    Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(LoadStartState);
    }

    void LoadStartState()
    {
        Time.timeScale = 1; // Unpause
        DontDestroy.clearDontDestroy();
        SceneManager.LoadScene("StartState");
    }
}
