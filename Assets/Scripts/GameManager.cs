﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public delegate void CurrencyChanged();

public class GameManager : Singleton<GameManager> {

	public event CurrencyChanged Changed;

	public TowerBtn ClickedBtn { get; set; }

	private int currency;

	private int wave = 0;

	private int lives;

	private bool gameOver = false;

	private bool choosingWallToLeap = false;

	private TileScript wallToLeap;

	public GameObject SelectedPortal { get; set; }

	public bool ChangingPortal { get; set; }

	[SerializeField]
	private GameObject statsPanel;

	[SerializeField]
	private GameObject gameOverMenu;

	[SerializeField]
	private Text currencyText;

	[SerializeField]
	private GameObject waveButton;

	[SerializeField]
	private GameObject upgradePanel;

	[SerializeField]
	private Text sellText;

	[SerializeField]
	private Text statText;

	[SerializeField]
	private Text upgradePrice;

	[SerializeField]
	private GameObject map;

	private Tower selectedTower;

	private List<Monster> activeMonsters = new List<Monster> ();

	public ObjectPool Pool { get; set; }

	public bool PlacingWalls = false;

	public GameObject Map 
	{
		get 
		{
			return map;
		}
		set 
		{
			map = value;
		}
	}

	public TileScript WallToLeap 
	{
		get 
		{
			return wallToLeap;
		}
		set 
		{
			wallToLeap = value;
		}
	}

	public bool ChoosingWallToLeap 
	{
		get 
		{
			return choosingWallToLeap;
		}
		set 
		{
			choosingWallToLeap = value;
		}
	}

	public bool WaveActive
	{
		get
		{
			return activeMonsters.Count > 0;
		}
	}

	public int Currency
	{
		get 
		{
			return currency;
		}
		set 
		{
			this.currency = value;
			this.currencyText.text = value.ToString () + " <color=yellow>$</color>";

			OnCurrencyChanged ();
		}
	}

	private void Awake ()
	{
		Pool = GetComponent<ObjectPool> ();
	}

	void Start () 
	{
		Currency = 25;
		ChangingPortal = false;
	}
	
	void Update () 
	{
		HandleEscape ();
	}

	public void PickTower(TowerBtn towerBtn)
	{
		if (ClickedBtn != null) 
		{
			Hover.Instance.Deactivate ();
		}
		
		else if (Currency >= towerBtn.Price && !WaveActive) 
		{
			this.ClickedBtn = towerBtn;
			Hover.Instance.Activate (towerBtn.Sprite);
		}
	}

	public void BuyTower()
	{

		if (Currency >= ClickedBtn.Price) 
		{
			Currency -= ClickedBtn.Price;
		}

		if (ClickedBtn.name == "WallBtn") 
		{
			if (Input.GetMouseButtonUp (0)) 
			{
				Hover.Instance.Deactivate ();
			}
		} 
		else 
		{
			Hover.Instance.Deactivate ();
		}
	}

	public void OnCurrencyChanged()
	{
		if(Changed != null)
		{
			Changed ();
		}
	}

	private void HandleEscape()
	{

		if (Input.GetKeyDown (KeyCode.Escape)) 
		{

			Hover.Instance.Deactivate ();
		}
	}

	public void StartWave(string type)
	{
		LevelManager.Instance.GeneratePathBlueToRed ();

		Monster monster = Pool.GetObject (type).GetComponent<Monster> ();

		monster.Spawn ();

		activeMonsters.Add (monster);
	}

//	private IEnumerator SpawnWave()
//	{
//
//
////			yield return new WaitForSeconds (2.5f);
//	}

	public void RemoveMonster(Monster monster)
	{
		activeMonsters.Remove (monster);
	
	}

	public void GameOver()
	{
		if (!gameOver) 
		{
			gameOverMenu.SetActive (true);
		}
	}

	public void Restart()
	{
		Time.timeScale = 1;

		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

	public void QuitGame()
	{
		Application.Quit ();
	}

	public void SelectTower(Tower tower)
	{
		//if a tower is already selected, deselect it
		if (selectedTower != null) {
			selectedTower.Select ();
		}
		
		selectedTower = tower;
		selectedTower.Select ();

		sellText.text = "+" + (selectedTower.Price / 2).ToString () + " $";

		upgradePanel.SetActive (true);
	
	}

	public void DeselectTower()
	{
		
		if (selectedTower != null) 
		{
			selectedTower.Select ();
		}

		upgradePanel.SetActive (false);

		selectedTower = null;
	}

	public void SellTower()
	{
		if (selectedTower != null) 
		{
			Currency += selectedTower.Price / 2;

			selectedTower.GetComponentInParent<TileScript> ().IsEmpty = true;

			selectedTower.transform.parent.GetComponent<SpecificObject>().DestroySavable ();

			Destroy (selectedTower.transform.parent.gameObject);

			DeselectTower ();

		}
	}

	public void ShowStats()
	{
		statsPanel.SetActive (!statsPanel.activeSelf);
	}

	public void SetTooltipText(string txt)
	{
		statText.text = txt;
	}

	public void UpdateUpgradeTooltip()
	{
		if (selectedTower != null) 
		{
			sellText.text = "+ " + (selectedTower.Price/2).ToString() + " $";
			SetTooltipText (selectedTower.GetStats());

			if (selectedTower.NextUpgrade != null) 
			{
				upgradePrice.text = "- " + selectedTower.NextUpgrade.Price.ToString () + " $";
			} 
			else 
			{
				upgradePrice.text = string.Empty;
			}
		}

	}

	public void ShowSelectedTowerStats()
	{
		statsPanel.SetActive (!statsPanel.activeSelf);
		UpdateUpgradeTooltip ();
	}

	public void UpgradeTower()
	{
		if (selectedTower != null) 
		{
			if (selectedTower.Level <= selectedTower.TowerUpgrades.Length && Currency >= selectedTower.NextUpgrade.Price) 
			{
				selectedTower.Upgrade ();
			}
		}
	}

	public void EnterBaconraid()
	{
		SceneManager.LoadScene ("Baconraid");
	}

	public void EndBaconraid()
	{
		SceneManager.LoadScene ("Home");
	}

	public void SelectWallToLeap()
	{
		WallTower[] walls = GameObject.FindObjectsOfType<WallTower> ();

		if (ChoosingWallToLeap) {
			WhitewashNonWallToLeaps ();

			TileScript[] tiles = GameObject.FindObjectsOfType<TileScript>();

			foreach (TileScript tile in tiles) {
				if (tile.Discovered) {
					tile.GetComponent<SpriteRenderer> ().color = Color.white;
				} else
					tile.GetComponent<SpriteRenderer> ().color = Color.grey;
			}
		}

		if (!ChoosingWallToLeap) {
			GreyoutNonWalls ();
		}

		ChoosingWallToLeap = !ChoosingWallToLeap;

		foreach (WallTower wall in walls) {
			if (wall.transform.parent.parent.GetComponent<TileScript> ().Discovered) {
				Debug.Log (walls.Length > 0);
				wall.gameObject.layer = 2;
				wall.transform.parent.gameObject.layer = 2;
				wall.transform.parent.GetChild (1).gameObject.layer = 2;
			}
		}
	}

	public void ResetWallLayer()
	{
		WallTower[] walls = GameObject.FindObjectsOfType<WallTower> ();
	
		foreach (WallTower wall in walls) {
			if (wall.transform.parent.parent.GetComponent<TileScript> ().Discovered) {
				wall.gameObject.layer = 0;
				wall.transform.parent.gameObject.layer = 0;
				wall.transform.parent.GetChild (1).gameObject.layer = 0;
				wall.transform.parent.parent.GetComponent<SpriteRenderer> ().color = Color.white;
			}
		}
	}

	public void GreyoutNonWalls()
	{
		TileScript[] tiles = GameObject.FindObjectsOfType<TileScript> ();
		Tower[] towers = GameObject.FindObjectsOfType<Tower> ();

		foreach (TileScript tile in tiles) 
		{
			tile.GetComponent<SpriteRenderer> ().color = new Color32(255,0,0,100);
		}
			
		foreach (Tower tower in towers) 
		{
			if (tower.transform.parent.tag != "Wall") 
			{
				tower.transform.parent.GetComponent<SpriteRenderer> ().color = new Color32(255,0,0,100);
			}
		}
	}

	public void WhitewashNonWallToLeaps()
	{
		TileScript[] tiles = GameObject.FindObjectsOfType<TileScript> ();
		Tower[] towers = GameObject.FindObjectsOfType<Tower> ();

		foreach (TileScript tile in tiles) 
		{
			if (tile.Discovered) {
				tile.GetComponent<SpriteRenderer> ().color = Color.white;
			}
		}

		foreach (Tower tower in towers) 
		{
			if (tower.transform.parent.tag != "Wall") 
			{
				tower.transform.parent.GetComponent<SpriteRenderer> ().color = Color.white;
			}		
		}
	}
}
