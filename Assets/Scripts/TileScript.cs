﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class TileScript : MonoBehaviour {

	public Point GridPosition { get; private set; }

	public bool IsEmpty { get; set; }

	private Color32 fullColor = new Color32(255, 118, 118, 255);
	private Color32 emptyColor = new Color32(96, 255, 90, 255);

	private Tower myTower;

	private SpriteRenderer spriteRenderer;

	public bool Walkable { get; set; }

	public bool Debugging { get; set; }

	public Vector2 WorldPosition 
	{ 
		get 
		{
			return new Vector2(transform.position.x + (GetComponent<SpriteRenderer>().bounds.size.x/2), transform.position.y - (GetComponent<SpriteRenderer>().bounds.size.y/2));
		}
	}

	// Use this for initialization
	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void Setup(Point gridPosition, Vector3 worldPosition, Transform parent)
	{
		Walkable = true;
		IsEmpty = true;
		this.GridPosition = gridPosition;
		transform.position = worldPosition;
		transform.SetParent (parent);

		LevelManager.Instance.Tiles.Add(gridPosition, this);
	}

	private void OnMouseOver()
	{
		if (GameManager.Instance.ChoosingWallToLeap) 
		{
			if (this.transform.childCount > 0 && this.transform.GetChild (0).tag == "Wall") 
			{
				this.transform.GetChild (0).GetComponent<SpriteRenderer> ().color = Color.green;
			} 
			else
			{
				ColorTile (fullColor);
			} 

			if (!EventSystem.current.IsPointerOverGameObject () && Input.GetMouseButtonDown (0)) 
			{
				if (this.transform.GetChild (0).tag == "Wall") 
				{
					if (GameManager.Instance.WallToLeap != null) 
					{
						GameManager.Instance.WallToLeap.transform.GetChild (0).GetComponent<SpriteRenderer> ().color = Color.white;

						GameManager.Instance.WallToLeap = null;
					}
					GameManager.Instance.WallToLeap = this;

					this.transform.GetChild (0).GetComponent<SpriteRenderer> ().color = Color.green;

					GameManager.Instance.ChoosingWallToLeap = false;

					GameManager.Instance.ResetWallLayer ();

					GameManager.Instance.WhitewashNonWallToLeaps ();
				}
			}
		} 
		else 
		{
			if (GameManager.Instance.ChangingPortal && Input.GetMouseButtonUp (0)) 
			{
				PlacePortal ();
			} 
			else if (!EventSystem.current.IsPointerOverGameObject () && GameManager.Instance.ClickedBtn != null) 
			{
				if (IsEmpty && !Debugging && !GameManager.Instance.ChoosingWallToLeap) 
				{
					ColorTile (emptyColor);
				}

				if (!IsEmpty && !Debugging && !GameManager.Instance.ChoosingWallToLeap) 
				{
					ColorTile (fullColor);
				} 
				else if (Input.GetMouseButtonDown (0)) 
				{
					PlaceTower ();
				}
			} 
			else if (!EventSystem.current.IsPointerOverGameObject () && GameManager.Instance.ClickedBtn == null && Input.GetMouseButtonDown (0)) 
			{
				if (myTower != null) 
				{
					GameManager.Instance.SelectTower (myTower);
				} 
				else 
				{
					GameManager.Instance.DeselectTower ();
				}
			} 
		}
	}

	private void OnMouseExit()
	{
		if (GameManager.Instance.ChoosingWallToLeap)
		{
			if (this.transform.childCount > 0 && this.transform.GetChild (0).tag == "Wall") 
			{
				this.transform.GetChild (0).GetComponent<SpriteRenderer> ().color = Color.white;
			} 

			if (this.transform.childCount > 0 && this.transform.GetChild (0).tag == "Tower") 
			{
				ColorTile (Color.grey);
				this.transform.GetChild (0).GetComponent<SpriteRenderer> ().color = Color.grey;
			} 
			else 
			{
				ColorTile (Color.grey);
			}
		}
		else if (!Debugging) 
		{
			ColorTile (Color.white);
		}
	}

	private void PlaceTower()
	{

		GameObject tower = Instantiate (GameManager.Instance.ClickedBtn.TowerPrefab, transform.position, Quaternion.identity);
		tower.GetComponent<SpriteRenderer> ().sortingOrder = GridPosition.Y;

		tower.transform.SetParent (transform);

		this.myTower = tower.transform.GetChild (0).GetComponent<Tower> ();

		IsEmpty = false;

		ColorTile (Color.white);

		myTower.Price = GameManager.Instance.ClickedBtn.Price;

		GameManager.Instance.BuyTower ();

		myTower.IsActive = true;

		Walkable = false;
	}

	private void ColorTile(Color32 newColor)
	{
			spriteRenderer.color = newColor;
	}

	private void PlacePortal(){
		GameObject portal = GameManager.Instance.SelectedPortal;

//		Debug.Log ("tile pos: " + transform.position);

		portal.transform.position = transform.position;
		gameObject.GetComponent<SpriteRenderer> ().color = Color.white;

//		Debug.Log ("portal pos: " + portal.transform.position);

		portal.SetActive(true);
					
		if (GameManager.Instance.SelectedPortal.name == "BluePortal") {
			LevelManager.Instance.BlueSpawn = new Point (this.GridPosition.X, this.GridPosition.Y);
		}

		if (GameManager.Instance.SelectedPortal.name == "RedPortal(Clone)") {
			LevelManager.Instance.RedSpawn = new Point (this.GridPosition.X, this.GridPosition.Y);
		}

		Hover.Instance.Deactivate ();
		GameManager.Instance.ChangingPortal = false;
	}
}
