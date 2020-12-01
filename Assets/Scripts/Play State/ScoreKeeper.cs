using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ScoreKeeper : MonoBehaviour
{
    public static int score;
    public Transform player;
    int startingPosZ;
    Text text;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        startingPosZ = (int)player.position.z;

        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        score = (int)player.position.z - startingPosZ;
        text.text = score.ToString() + "m";
    }
}
