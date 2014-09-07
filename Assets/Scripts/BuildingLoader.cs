using UnityEngine;
using System.Collections;
using System;
using System.Linq;

public class BuildingLoader : MonoBehaviour {
    //BuildManager.cs is way better then BuildingLoader.cs. I hope I never have to touch you again
    public GameObject testObject;
    //The amount of columns per row, excluding non integer values
    private const int COLUMNCOUNT = 4;

    private int rowCount;
    private int[] data;
    BuildManager bm;
    private GameObject _currentItem;
    private Item item;
	// Use this for initialization
	void Start () {
        bm = GetComponent<BuildManager>();
        StartCoroutine("DB_GetRowCount");
       
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator DB_GetRowCount()
    {
        string name = PlayerPrefs.GetString("name");
        WWWForm form = new WWWForm();
        form.AddField("loadbuildings_name",name);
        form.AddField("loadbuildings_rowcount", 1);
        WWW www = new WWW("http://kuhmaus.bplaced.net/db_loadbuildings.php", form);
        while (!www.isDone && string.IsNullOrEmpty(www.error))
        {
            Debug.Log("Getting rowcount...");
            yield return null;
        }
        rowCount = Convert.ToInt32(www.text);
        Debug.Log(www.text);
        StartCoroutine("DB_GetBuildings");
        www.Dispose();
    }

    IEnumerator DB_GetBuildings()
    {
        string name = PlayerPrefs.GetString("name");
        WWWForm form = new WWWForm();
        form.AddField("loadbuildings_name", name);
        form.AddField("loadbuildings_rowcount",0);
        WWW www = new WWW("http://kuhmaus.bplaced.net/db_loadbuildings.php", form);
        while (!www.isDone && string.IsNullOrEmpty(www.error))
        {
            Debug.Log("Getting buildings...");
            yield return null;
        }
       ConvertTextToData(www.text);
        www.Dispose();
    }

    void ConvertTextToData(string s)
    {
        data = new int[rowCount*COLUMNCOUNT];
        string[] seperators = { "," };
        string[] results = s.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < results.Length; i++)
        {
            data[i] = Convert.ToInt32(results[i]);
        }
        for (int i = 0; i < rowCount; i++)
        {
            
            _currentItem = Instantiate(testObject, GetPos(i), Quaternion.identity) as GameObject;
            item = (Item)_currentItem.GetComponent<Item>();
            item.topLeftPosition = new Vector2(_currentItem.transform.position.x, _currentItem.transform.position.z);

            item.IsPlaced = true;

            bm.grid.AlignTransform(_currentItem.transform);
            _currentItem.transform.position =CalculateOffsetY();
            
            
        }
       
    }

    Vector3 GetPos(int index)
    {
        Vector2 posData = new Vector2();
       
        for (int i = 0; i < 2; i++)
        {
            if (i == 0)
                posData.x =data[i+index*COLUMNCOUNT];
            if (i == 1)
                posData.y = data[i+index*COLUMNCOUNT];
        }

        Debug.Log(posData);
            return new Vector3(posData.x, 0.5f, posData.y);
    }

    Vector3 CalculateOffsetY()
    {
        //first store the objects position in grid coordinates
        Vector3 gridPosition = bm.grid.WorldToGrid(_currentItem.transform.position);
        //then change only the Y coordinate
        gridPosition.y = 0.5f * _currentItem.transform.lossyScale.y;

        //convert the result back to world coordinates
        return bm.grid.GridToWorld(gridPosition);
    }
}
