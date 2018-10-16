using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceEventChangeTexture : MonoBehaviour {
	public Sprite sprite;

	void Start () {
		Slicer2D slicer = GetComponent<Slicer2D>();
		slicer.AddResultEvent(SlicerEvent);	
	}
	
	void SlicerEvent (Slice2D slice) {
		foreach(GameObject g in slice.gameObjects) {
			MeshRenderer slicer = g.GetComponent<MeshRenderer>();
			slicer.material.mainTexture = sprite.texture;
		}
	}
}
