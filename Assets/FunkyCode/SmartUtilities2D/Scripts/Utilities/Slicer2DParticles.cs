using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slicer2DParticles : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Slicer2D slicer = GetComponent<Slicer2D>();
		if (slicer != null) {
			slicer.AddResultEvent(SliceEvent);
		}
	}
	
	void SliceEvent(Slice2D slice) {
		foreach(List<Vector2D> pointList in slice.slices) {
			foreach(Pair2D p in Pair2D.GetList(pointList)) {
				Vector2 pos = p.A.vector;
				while (Vector2.Distance(pos, p.B.vector) > 0.5) {
					pos = Vector2.MoveTowards(pos, p.B.vector, 0.35f);
					Particle2D.Create(Random.Range(0, 360), new Vector3(pos.x, pos.y, -1));
				}
			}
		} 
	}
}
