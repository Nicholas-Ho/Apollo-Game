using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour {

	// make this static so it's visible across all instances
	public static List<DontDestroy> instances = new List<DontDestroy>();
	public string stringRef;

	// singleton pattern; make sure only one of these exists at one time, else we will
	// get an additional set of sounds with every scene reload, layering on the music
	// track indefinitely
	void Awake() {
		if (instances.Count == 0) {
			instances.Add(this);
			DontDestroyOnLoad(gameObject);
		} else {
			bool presentFlag = false;

			for(int i = 0; i < instances.Count; i++){
				if(instances[i].stringRef == stringRef){
					presentFlag = true;
				}
			}

			if(presentFlag == true){
				Destroy(gameObject);
			} else {
				instances.Add(this);
				DontDestroyOnLoad(gameObject);
			}
		}
	}

	public static void clearDontDestroy(){
		for(int i = 0; i < instances.Count; i++){
            Destroy(instances[i].gameObject);
        }

		instances.Clear();
	}
}
