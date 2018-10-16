using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThinSliceEdge : MonoBehaviour {

	void OnEnable() { edgeList.Add (this);}
	void OnDisable() { edgeList.Remove (this);}
	static private List<ThinSliceEdge> edgeList = new List<ThinSliceEdge>();
	static public List<ThinSliceEdge> GetList(){ return(new List<ThinSliceEdge>(edgeList));}

	Polygon2D edges = null;

	public Polygon2D GetEdges() {
		if (edges == null) {
			edges = Polygon2D.CreateFromCollider (gameObject);
		}
		return(edges);
	}


	public bool ItersectsWithMap() {
		Polygon2D edges = GetEdges().ToWorldSpace(gameObject.transform);

		bool intersect = false;
		foreach(Slicer2D slicer in Slicer2D.GetList()) {
			Polygon2D polyB = slicer.GetPolygon().ToWorldSpace(slicer.gameObject.transform);

			if (Math2D.SliceIntersectPoly(edges.pointsList, polyB)) {
				intersect = true;
			}

			foreach(Vector2D p in edges.pointsList) {
				if (Math2D.PointInPoly(p, polyB)) {
					return(true);
				}
			}
		}
		return(intersect);
	}
}
