using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ScoreKeeper : MonoBehaviour
{
    public Transform player;
    int startingPosZ;
    Text text;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        startingPosZ = (int)player.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        text.text = (((int)player.position.z - startingPosZ) / 2).ToString();
    }
}
