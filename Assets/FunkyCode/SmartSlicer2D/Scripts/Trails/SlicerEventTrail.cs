using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SlicerEventTrail : MonoBehaviour {
	public GameObject prefab;

	void Start () {
		Slicer2D slicer = GetComponent<Slicer2D>();
		slicer.AddResultEvent(SliceEvent);
	}
	
	void SliceEvent (Slice2D slice2D) {
		foreach(List<Vector2D> points in slice2D.slices) {
			if (points.Count > 1) {
				Vector3 pos = points.First().vector;
				pos.z = transform.position.z;

				GameObject create = Instantiate(prefab, pos, transform.rotation) as GameObject;

				Slicer2DTrail trail = create.AddComponent<Slicer2DTrail>();

				foreach(Vector2D point in points) {
					trail.path.Add(point.vector);
				}
			} else {
			//	Debug.LogError("Incorrect Number Of Slices");
			}
		}

	}
}
