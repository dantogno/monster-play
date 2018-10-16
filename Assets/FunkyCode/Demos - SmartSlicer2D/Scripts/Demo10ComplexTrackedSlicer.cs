using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo10ComplexTrackedSlicer : MonoBehaviour {
	public ComplexSlicerTracker trackerObject = new ComplexSlicerTracker();

	void Update () {
		trackerObject.Update(transform.position);
	}

	public void OnRenderObject()
	{
		Max2D.SetLineWidth (0.5f);
		Max2D.SetColor (Color.black);
		Max2D.SetBorder (false);
		Max2D.SetLineMode(Max2D.LineMode.Smooth);
		
		foreach(ComplexSlicerTrackerObject tracker in trackerObject.trackerList) {
			if (tracker.slicer != null && tracker.tracking) {
				Max2D.DrawSlice(Vector2DList.ToWorldSpace(tracker.slicer.transform, tracker.pointsList), tracker.slicer.transform.position.z - 0.001f);
			}
		}
	}
}