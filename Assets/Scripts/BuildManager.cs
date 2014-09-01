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

				Vector2 leftTop = new Vector3 (calcPoint.x - 1, calcPoint.z + 1);

				item.IsPlaced = true;

				UpdateDatabase(leftTop);

				ModeType = Mode.Move;
				_currentItem = null;
			}
			else if(Input.GetMouseButtonDown(0) && ModeType == Mode.Move) // move the item
			{
				Item ItemHit = hit.transform.GetComponent<Item>();

				if(ItemHit != null)
				{
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
	

	public void UpdateDatabase(Vector2 topLeft)
	{

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
