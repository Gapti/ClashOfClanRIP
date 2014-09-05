using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;
public class CreatePlayer : MonoBehaviour
{
    public GameObject nameInput;
    public GameObject submitButton;
    public GameObject characterWarning;
    public GameObject duplicatePlayerWarning;
    public GameObject alreadyRegistered;
    public GameObject signupSuccessful;
    public GameObject enternameText;
    private bool validName = true;
    private bool set = false;
    void Start()
    {
        //TODO: Dont forget to remove the line below! ;)
        PlayerPrefs.DeleteAll();

        if (!PlayerPrefs.HasKey("name"))
        {
            nameInput.SetActive(true);
            submitButton.SetActive(true);
            alreadyRegistered.SetActive(false);
            enternameText.SetActive(true);
        }
        else
        {
            nameInput.SetActive(false);
            submitButton.SetActive(false);
            alreadyRegistered.SetActive(true);
            enternameText.SetActive(false);
        }


    }

    void Update()
    {
        if (set)
            return;
        if (nameInput.GetComponent<InputField>().text.text != Regex.Replace(nameInput.GetComponent<InputField>().text.text, @"[^a-zA-Z0-9]", ""))
        {

            characterWarning.SetActive(true);
            validName = false;
        }
        else
        {
            validName = true;
            characterWarning.SetActive(false);
        }


        if (nameInput.GetComponent<InputField>().text.text.Length < 4)
        {
            submitButton.SetActive(false);

        }
        else if (!validName)
        {
            submitButton.SetActive(false);
        }
        else
        {
            submitButton.SetActive(true);

        }


    }

    public void StartSaveName()
    {
        if (set)
            return;
        StartCoroutine("SaveName");
    }


    IEnumerator SaveName()
    {

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
            Debug.Log("There is already a Player with this name, please enter a new name");
            StartCoroutine("Failed");
        }
        //If the name is not taken, store it to the database and save the name to PlayerPrefs. Then disable the Name Input UI.
        else
        {
            WWWForm form = new WWWForm();
            form.AddField("createplayer_name", nameInput.GetComponent<InputField>().text.text);
            form.AddField("createplayer_money", money);
            WWW w = new WWW("http://kuhmaus.bplaced.net/db_createplayer.php", form);
            while (!w.isDone && string.IsNullOrEmpty(w.error))
            {
                Debug.Log("Adding Player: " + nameInput.GetComponent<InputField>().text.text + ", " + money);

                yield return null;
            }
            PlayerPrefs.SetString("name", nameInput.GetComponent<InputField>().text.text);
            set = true;
            StartCoroutine("Success");
            w.Dispose();
            enternameText.SetActive(false);
            nameInput.SetActive(false);
            submitButton.SetActive(false);
            Debug.Log("Player Added");
        }

        www.Dispose();

    }


    IEnumerator Success()
    {
        signupSuccessful.SetActive(true);
        duplicatePlayerWarning.SetActive(false);
        yield return new WaitForSeconds(2);
        signupSuccessful.SetActive(false);
        duplicatePlayerWarning.SetActive(false);
    }

    IEnumerator Failed()
    {
        duplicatePlayerWarning.SetActive(true);
        yield return new WaitForSeconds(2.3f);
        duplicatePlayerWarning.SetActive(false);
    }
}
