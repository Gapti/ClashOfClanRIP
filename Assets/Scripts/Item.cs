using UnityEngine;
using System.Collections;

public enum Direction
{
	None,
	North,
	South,
	East,
	West
}

public class Item : MonoBehaviour  {

	public int id;
	public int CellWidth;
	public int CellHeight;
	public string ItemName;
	public string ItemDesc;
	public Direction direction = Direction.None;
	public GameObject HightLight;
    public Vector2 topLeftPosition;

	public int Level = 1;

	private bool _canPlace = true;

	private bool _isPlaced;

	public bool IsPlaced
	{
		get
		{
			return _isPlaced;
		}

		set
		{
			if(value)
			{
				HightLight.SetActive(false);
			}
			else
			{
				HightLight.SetActive(true);
			}

			_isPlaced = value;
		}
	}

	public bool CanPLace
	{
		get
		{
			return _canPlace;
		}
	}

	void OnTriggerStay(Collider collider)
	{
		if (!_isPlaced) 
		{
			_canPlace = false;
		
			this.HightLight.renderer.material.SetColor ("_Color", Color.red);
		}
		
	}
	
	void OnTriggerExit(Collider collider)
	{
		if (!_isPlaced) 
		{
			_canPlace = true;
		
			this.HightLight.renderer.material.SetColor ("_Color", Color.green);
		}
	}



}
