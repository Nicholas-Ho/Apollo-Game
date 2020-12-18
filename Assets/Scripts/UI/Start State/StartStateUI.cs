using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Text))]
public class StartStateUI : MonoBehaviour
{
    Image panel;
    Text title;
    Text play;
    bool uiComplete;

    // Start is called before the first frame update
    void Start()
    {
        uiComplete = false;
        
        panel = transform.GetChild(0).gameObject.GetComponent<Image>();
        title = transform.GetChild(1).gameObject.GetComponent<Text>();
        play = transform.GetChild(2).gameObject.transform.GetChild(0).GetComponent<Text>();

        EnterStartState();
    }

    void Update()
    {
        if(uiComplete && Input.GetKeyDown("escape")){
            Application.Quit();
        }
    }

    public bool uiIsComplete()
    {
        return uiComplete;
    }

    void EnterStartState(){
        StartCoroutine(TitleFadeIn(1.5f));
        StartCoroutine(PanelFadeOut(4f));
        StartCoroutine(PlayFadeIn(6f));
        StartCoroutine(PlayButtonActive(8f));
    }

    public void LoadPlayState(){
        StartCoroutine(PanelFadeIn(0));
        StartCoroutine(GoToPlayState(2f));
    }

    IEnumerator TitleFadeIn(float delay)
    {
        yield return new WaitForSeconds(delay);

        while(title.color.a < 1){
            title.color = Color.Lerp(title.color, Color.white, 2 * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator PanelFadeOut(float delay)
    {
        yield return new WaitForSeconds(delay);

        Color endColour = new Color(0, 0, 0, 0);
        while(panel.color.a > 0){
            panel.color = Color.Lerp(panel.color, endColour, 2 * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator PlayFadeIn(float delay)
    {
        yield return new WaitForSeconds(delay);

        while(play.color.a < 1){
            play.color = Color.Lerp(play.color, Color.white, 2 * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator PlayButtonActive(float delay)
    {
        yield return new WaitForSeconds(delay);
        uiComplete = true;
        yield return null;
    }

    IEnumerator PanelFadeIn(float delay)
    {
        panel.color = new Color(1, 1, 1, 0);
        yield return new WaitForSeconds(delay);

        while(panel.color.a < 1){
            panel.color = Color.Lerp(panel.color, Color.white, 1.5f * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator GoToPlayState(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        DontDestroy.clearDontDestroy();
        SceneManager.LoadScene("PlayState");
        yield return null;
    }
}
