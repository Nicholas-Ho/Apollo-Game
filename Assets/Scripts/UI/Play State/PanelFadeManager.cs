using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelFadeManager : MonoBehaviour
{
    Image panel;
    PauseScript pauseScript;
    // Start is called before the first frame update
    void Start()
    {
        panel = transform.GetChild(1).gameObject.GetComponent<Image>();
        pauseScript = FindObjectOfType<PauseScript>();

        StartCoroutine(PanelFadeOut(0.75f));
        StartCoroutine(EnablePause(1f));
    }

    IEnumerator PanelFadeOut(float delay)
    {
        yield return new WaitForSeconds(delay);

        Color endColour = new Color(1, 1, 1, 0);
        while(panel.color.a > 0){
            panel.color = Color.Lerp(panel.color, endColour, 2 * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator EnablePause(float delay){
        yield return new WaitForSeconds(delay);

        pauseScript.pauseActive = true;
        yield return null;
    }
}
