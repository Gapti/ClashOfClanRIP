using UnityEngine;
using System.Collections;

public class MoveBuilding : MonoBehaviour {

	public GFGrid grid;
	public Collider gridCollider;

	private bool beingDragged; // true while the player is dragging the block around, otherwise false
	private Vector3 oldPosition; // the previous valid position
	
	private Transform cachedTransform; //cache the transform for performance

	void Awake()
	{
		cachedTransform = this.transform;
		grid = GameObject.FindGameObjectWithTag ("Grid").GetComponent<GFGrid> ();
		gridCollider = grid.GetComponent<Collider> ();
	}

	void OnMouseDown(){
		beingDragged = true;
	}
	void OnMouseUp(){
		beingDragged = false;
		cachedTransform.position = oldPosition; // place on the last valid position
	}

	void FixedUpdate(){
		if(beingDragged){
			//store the position if it is valid
				oldPosition = cachedTransform.position;
			DragObject();
		}
	}

	//this function gets called every frame while the object (its collider) is being dragged with the mouse
	void DragObject(){
		if(!grid || !gridCollider) // if there is no grid or no collider there is nothing more we can do
			return;
		
		//handle mouse input to convert it to world coordinates
		Vector3 cursorWorldPoint = ShootRay();
		
		//change the X and Z coordinates according to the cursor (the Y coordinate stays the same after the last step)
		cachedTransform.position = cursorWorldPoint;
		
		//now align the object and snap it to the bottom.
		grid.AlignTransform(cachedTransform);
		cachedTransform.position = CalculateOffsetY(); // this forces the Y-coordinate into position
	}
	
	// makes the object snap to the bottom of the grid, respecting the grid's rotation
	Vector3 CalculateOffsetY(){
		//first store the objects position in grid coordinates
		Vector3 gridPosition = grid.WorldToGrid(cachedTransform.position);
		//then change only the Y coordinate
		gridPosition.y = 0.5f * cachedTransform.lossyScale.y;
		
		//convert the result back to world coordinates
		return grid.GridToWorld(gridPosition);
	}
	
	// shoots a ray, which can only hit the grid plane, from the mouse cursor via the camera and returns the hit position
	Vector3 ShootRay () {
		RaycastHit hit;
		gridCollider.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, Mathf.Infinity);
		//this is where the player's cursor is pointing towards (if nothing was hit return the current position => no movement)
		return hit.collider != null ? hit.point : cachedTransform.position;
	}

}
