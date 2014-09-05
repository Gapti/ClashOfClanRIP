using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CreatePlayer : MonoBehaviour {
    public GameObject nameInput;
    public GameObject submitButton;
	
	void Start () {
        //TODO: Dont forget to remove the line below! ;)
        PlayerPrefs.DeleteAll();

	if(!PlayerPrefs.HasKey("name")){
        nameInput.SetActive(true);
        submitButton.SetActive(true);
    }
    else
    {
        nameInput.SetActive(false);
        submitButton.SetActive(false);
    }

	}

    public void StartSaveName()
    {
        if (PlayerPrefs.HasKey("name"))
            return;
        if (nameInput.GetComponent<InputField>().text.text.Length < 4)
            return;
        
        StartCoroutine("SaveName");
    }


    IEnumerator SaveName()
    {
        //TODO: Only allow A-z and numbers from 0-9 for Player names
        //TODO: Add feedback for "forbidden characters", "Player name taken"  
        int money = 200;
        //Checks if there is already a Player with this name in the database
        WWW www = new WWW("http://kuhmaus.bplaced.net/db_checkplayers.php");
        while (!www.isDone && string.IsNullOrEmpty(www.error))
        {
            Debug.Log("Checking Player Database...");
            yield return null;
        }
        //If the name is taken, reset the input field and wait for a new name
        if (www.text.Contains(nameInput.GetComponent<InputField>().text.text))
        {
            nameInput.GetComponent<InputField>().text.text = "";
            Debug.Log("There is already a Player with this name, please enter a new name");
        }
        //If the name is not taken, store it to the database and save the name to PlayerPrefs. Then disable the Name Input UI.
        else
        {
            WWWForm form = new WWWForm();
            form.AddField("createplayer_name", nameInput.GetComponent<InputField>().text.text);
            form.AddField("createplayer_money", money);
            WWW w = new WWW("http://kuhmaus.bplaced.net/db_createplayer.php",form);
            while (!w.isDone && string.IsNullOrEmpty(w.error))
            {
                Debug.Log("Adding Player: " + nameInput.GetComponent<InputField>().text.text + ", " + money);
                yield return null;
            }
            PlayerPrefs.SetString("name", nameInput.GetComponent<InputField>().text.text);
            w.Dispose();
            nameInput.SetActive(false);
            submitButton.SetActive(false);
            Debug.Log("Player Added");
        }
  
        www.Dispose();
        
    }
}
