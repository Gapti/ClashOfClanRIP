using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class TileMaps : MonoBehaviour {

	public GameObject[] DataObjects;

	void Awake()
	{
		DataObjects = new GameObject[100];
	}

	void AddObject(GameObject obj, int index)
	{
		DataObjects [index] = obj;
	}

	public bool CheckPosition(GameObject obj, int index)
	{
		if (DataObjects[index] == null) 
		{
			AddObject (obj, index);
			return true;
		}
			else 
		{
			return false;
		}
	}

	public void RemoveObject(int index)
	{
		if(DataObjects[index] != null)
		Destroy(DataObjects[index].gameObject);
	}

}
