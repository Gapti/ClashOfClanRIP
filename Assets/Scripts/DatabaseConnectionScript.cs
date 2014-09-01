using UnityEngine;
using System.Collections;

public class DatabaseConnectionScript : MonoBehaviour {

	void Start() {
		StartCoroutine(GetPlayerData());
	}
	
	
	IEnumerator GetPlayerData() {
		WWW www = new WWW("http://kuhmaus.bplaced.net/db_connect.php"); 
		
		while(!www.isDone && string.IsNullOrEmpty(www.error)) {
            Debug.Log("Loading");
			yield return null;
		}
		
		Debug.Log (www.text);
	}
}
