using UnityEngine;
using System.Collections;

public enum Mode
{
	Move,
	Build
}

public class BuildManager : MonoBehaviour {
	
	public GFGrid grid;
	public Collider gridCollider;

	public GameObject CampFire;
	public GameObject Rock;
	public GameObject Crate;

	private GameObject _currentItem;
	private Item item;

	public int MapWidth = 50;
	public int MapHeight = 50;

	public Mode ModeType = Mode.Move;

	public Vector3 OldPosition;


	void Awake()
	{
		grid = GameObject.FindGameObjectWithTag ("Grid").GetComponent<GFGrid> ();
		gridCollider = grid.GetComponent<Collider> ();
	}

	// Update is called once per frame
	void FixedUpdate () {

		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

		if (Physics.Raycast (ray, out hit, 1000)) 
		{

			if(ModeType == Mode.Build || ModeType == Mode.Move && _currentItem != null)
			{
				_currentItem.transform.position = hit.point;
				grid.AlignTransform(_currentItem.transform);
				_currentItem.transform.position = CalculateOffsetY(); 
			}


			///place the item
			if(Input.GetMouseButtonDown(0) && ModeType == Mode.Build)
			{
				if(!item.CanPLace)
					return;
				item.topLeftPosition = new Vector2( _currentItem.transform.position.x, _currentItem.transform.position.z);

				item.IsPlaced = true;

				grid.AlignTransform(_currentItem.transform);
				_currentItem.transform.position = CalculateOffsetY(); 

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
	
	public void BuildCampFire()
	{
		if (ModeType == Mode.Build)
		return;

		ModeType = Mode.Build;

		_currentItem = (GameObject)Instantiate (CampFire, Input.mousePosition, Quaternion.identity);
		item = (Item)_currentItem.GetComponent<Item> ();
	}

	public void BuildRock()
	{
		if (ModeType == Mode.Build)
			return;
		
		ModeType = Mode.Build;
		
		_currentItem = (GameObject)Instantiate (Rock, Input.mousePosition, Quaternion.identity);
		item = (Item)_currentItem.GetComponent<Item> ();
	}

	public void BuildCrate()
	{
		if (ModeType == Mode.Build)
			return;
		
		ModeType = Mode.Build;
		
		_currentItem = (GameObject)Instantiate (Crate, Input.mousePosition, Quaternion.identity);
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


	Vector3 CalculateOffsetY(){
		//first store the objects position in grid coordinates
		Vector3 gridPosition = grid.WorldToGrid(_currentItem.transform.position);
		//then change only the Y coordinate
		gridPosition.y = 0.5f * _currentItem.transform.lossyScale.y;
		
		//convert the result back to world coordinates
		return grid.GridToWorld(gridPosition);
	}
}
