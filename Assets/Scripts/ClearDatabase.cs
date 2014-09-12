using UnityEngine;
using System.Collections;

public class ClearDatabase : MonoBehaviour {

    public void StartClearing()
    {
        StartCoroutine("DB_ClearBuildings");
    }

	IEnumerator DB_ClearBuildings () {
        WWWForm form = new WWWForm();
        form.AddField("clearBuildings_player", PlayerPrefs.GetString("name"));
        WWW www = new WWW("http://kuhmaus.bplaced.net/db_clearBuildings.php", form);
        while (!www.isDone && string.IsNullOrEmpty(www.error))
        {
            Debug.Log("Removing Buildings Of: " + PlayerPrefs.GetString("name"));
            yield return null;
        }
        www.Dispose();
	}
	

}
