using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualSpriteRenderer {
	public Sprite sprite;

	public int sortingLayerID = 0;
	public string sortingLayerName = "";
	public int sortingOrder = 0;

	public Color color;
	public Material material;

	public VirtualSpriteRenderer(SpriteRenderer spriteRenderer) {
		sprite = spriteRenderer.sprite;

		sortingLayerID = spriteRenderer.sortingLayerID;
		sortingLayerName = spriteRenderer.sortingLayerName;
		sortingOrder = spriteRenderer.sortingOrder;

		material = new Material(spriteRenderer.sharedMaterial);

		color = spriteRenderer.color;
	}
}


		

