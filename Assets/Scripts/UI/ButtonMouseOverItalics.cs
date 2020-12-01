using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonMouseOverItalics : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Text text;
    Font standardFont;
    public Font italicFont;
    bool mouseOver;
    // Start is called before the first frame update
    void Start()
    {
        text = transform.GetChild(0).GetComponent<Text>();
        standardFont = text.font;
    }

    void Update()
    {
        if(mouseOver){
            text.font = italicFont;
        } else {
            text.font = standardFont;
        }
    }

    public void OnPointerEnter(PointerEventData data)
    {
        mouseOver = true;
    }

    public void OnPointerExit(PointerEventData data)
    {
        mouseOver = false;
    }
}