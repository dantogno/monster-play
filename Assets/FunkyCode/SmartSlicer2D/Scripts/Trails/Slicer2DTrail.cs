using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Slicer2DTrail : MonoBehaviour {
	public List<Vector2> path = new List<Vector2>();
	float speed = 90f;

	// Update is called once per frame
	void Update () {
		if (path.Count < 1) {
			return;
		}

		UpdatePosition();

		if (Vector2.Distance(transform.position, path.First()) < 0.1f) {
			path.RemoveAt(0);
		}
	}

	public void UpdatePosition() {
      	Vector3 pos = Vector2.MoveTowards(transform.position, path.First(), speed * Time.deltaTime);
		pos.z = transform.position.z;
		transform.position = pos;
	}
}
