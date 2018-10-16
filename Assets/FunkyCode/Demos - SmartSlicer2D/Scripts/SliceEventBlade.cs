using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceEventBlade : MonoBehaviour {

	void Start () {
		Slicer2DAnchors slicer = GetComponent<Slicer2DAnchors>();
		if (slicer != null) {
			slicer.AddResultEvent(sliceEvent);
		}
	}
	
	void sliceEvent (Slice2D slice) {
		foreach(GameObject g in slice.gameObjects) {
			Destroy(g.GetComponent<SpinBlade>());
			Destroy(g.GetComponent<SliceEventBlade>());
			g.transform.parent = null;
		}
	}
}
