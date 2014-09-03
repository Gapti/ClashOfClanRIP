using UnityEngine;
using System.Collections;

public enum Mode
{
	Move,
	Build
}

public class BuildManager : MonoBehaviour {
	
	public GameObject Obj;

	private TileMaps _tileMaps;
	private GameObject _hover;

	private GameObject _currentItem;
	private Item item;

	public int MapWidth = 50;
	public int MapHeight = 50;

	public Mode ModeType = Mode.Move;

	//Mathf.RoundToInt( (calcPoint.y / 10 ) * 10 + (calcPoint.x * 10)), new Vector3(calcPoint.x, 0f, calcPoint.y)


	void Awake()
	{
		_tileMaps = GetComponent<TileMaps> ();
		_hover = GameObject.FindGameObjectWithTag ("Hover");
	}

	// Update is called once per frame
	void Update () {

		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

		if (Physics.Raycast (ray, out hit, 1000)) 
		{
			Vector3 calcPoint = getTilePoints(hit.point);

			if(ModeType == Mode.Build || ModeType == Mode.Move && _currentItem != null)
			_currentItem.transform.localPosition = new Vector3(calcPoint.x, 0f, calcPoint.y);

			///place the item
			if(Input.GetMouseButtonDown(0) && ModeType == Mode.Build)
			{
				if(!item.CanPLace)
					return;
                item.topLeftPosition = new Vector2(calcPoint.x + 1, calcPoint.y + 1);

				item.IsPlaced = true;

                StartCoroutine("DB_InsertBuilding", item.topLeftPosition);

				ModeType = Mode.Move;
				_currentItem = null;
			}
			else if(Input.GetMouseButtonDown(0) && ModeType == Mode.Move) // move the item
			{
				Item ItemHit = hit.transform.GetComponent<Item>();

				if(ItemHit != null)
				{
                    StartCoroutine("DB_RemoveBuilding", ItemHit.topLeftPosition);
					_currentItem = hit.transform.gameObject;
					item = (Item)_currentItem.GetComponent<Item> ();
					item.IsPlaced = false;
					ModeType = Mode.Build;

				}
			}
		}
	}
	
	public void StartBuild()
	{
		if (ModeType == Mode.Build)
		return;

		ModeType = Mode.Build;

		_currentItem = (GameObject)Instantiate (Obj, Input.mousePosition, Quaternion.identity);
		item = (Item)_currentItem.GetComponent<Item> ();
	}


    IEnumerator DB_InsertBuilding(Vector2 topLeft)
	{
		//Add a new building to the database as soon as its placed
        string name = "Kuhmaus";
        int xPos = (int)topLeft.x;
        int yPos = (int)topLeft.y;
        int buildingID = 2;
        int level =3;
        WWWForm form = new WWWForm();
        form.AddField("addbuilding_name",name);
        form.AddField("addbuilding_xPos",xPos);
        form.AddField("addbuilding_yPos", yPos);
        form.AddField("addbuilding_buildingID",buildingID);
        form.AddField("addbuilding_level", level);
        WWW www = new WWW("http://kuhmaus.bplaced.net/db_storebuilding.php",form);
        while (!www.isDone && string.IsNullOrEmpty(www.error))
        {
            Debug.Log("Storing building: " + name + ", " + xPos + ", " + yPos + ", " + buildingID + ", " + level);
            yield return null;
        }
        www.Dispose();
	}

    IEnumerator DB_RemoveBuilding(Vector2 topLeft)
    {
        string name = "Kuhmaus";
        int xPos = (int)topLeft.x;
        int yPos = (int)topLeft.y;
        WWWForm form = new WWWForm();
        
        form.AddField("removebuilding_name", name);
        form.AddField("removebuilding_xPos", xPos);
        form.AddField("removebuilding_yPos", yPos);
        WWW www = new WWW("http://kuhmaus.bplaced.net/db_removebuilding.php", form);
        while (!www.isDone && string.IsNullOrEmpty(www.error))
        {
            Debug.Log("Removing building: " + name + ", " + xPos + ", " + yPos);
            yield return null;
        }
        www.Dispose();
    }


	// Convert world space floor points to tile points
	Vector2 getTilePoints(Vector3 floorPoints)
	{
		Vector2 tilePoints = new Vector2();
		// Convert the space points to tile points
		tilePoints.x = (int)(floorPoints.x);
		tilePoints.y = (int)(floorPoints.z);

		//print ("y " + tilePoints.y + " x " + tilePoints.x); 

		// Return the tile points
		return tilePoints;
	}
}
