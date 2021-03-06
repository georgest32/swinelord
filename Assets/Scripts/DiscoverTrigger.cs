﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoverTrigger : MonoBehaviour {

	public void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Monster") 
		{
			this.transform.parent.GetComponent<SpriteRenderer> ().sortingOrder = 1;
			transform.parent.parent.GetComponent<TileScript> ().Discovered = true;
			transform.parent.parent.GetComponent<TileScript> ().DiscoverTile();
			transform.parent.GetComponent<SpriteRenderer> ().enabled = true;
		}
	}
}