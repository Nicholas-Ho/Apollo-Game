using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DisplayScore : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       GetComponent<Text>().text = ScoreKeeper.score.ToString() + "m";
    }
}
