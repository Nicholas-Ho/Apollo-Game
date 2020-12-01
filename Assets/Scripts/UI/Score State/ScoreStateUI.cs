using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreStateUI : MonoBehaviour
{
    bool transition;
    Image transitionPanel;
    // Start is called before the first frame update
    void Start()
    {
        transitionPanel = transform.GetChild(transform.childCount - 3).gameObject.GetComponent<Image>();

        transition = false;
    }

    void Update()
    {
        if(!transition && Input.GetKeyDown("escape")){
            Application.Quit();
        }
    }

    public void LoadPlayState()
    {
        transition = true;

        StartCoroutine(PanelFadeIn(0));
        StartCoroutine(GoToPlayState(2f));
    }

    IEnumerator PanelFadeIn(float delay)
    {
        transitionPanel.color = new Color(1, 1, 1, 0);
        yield return new WaitForSeconds(delay);

        while(transitionPanel.color.a < 1){
            transitionPanel.color = Color.Lerp(transitionPanel.color, Color.white, 1.5f * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator GoToPlayState(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        SceneManager.LoadScene("PlayState");
        yield return null;
    }
}
