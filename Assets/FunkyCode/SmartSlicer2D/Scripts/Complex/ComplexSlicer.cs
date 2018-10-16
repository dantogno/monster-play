using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class ComplexSlicer {
	public static float precision = 0.005f;

	static public Slice2D Slice(Polygon2D polygon, List<Vector2D> slice)
	{
		Slice2D result = Slice2D.Create (slice);

		if (slice.Count < 2) {
			return(result);	
		}

		// Normalize into clockwise
		polygon.Normalize ();

		// Optimize slicing for polygons that are far away
		Rect sliceRect = Math2D.GetBounds(slice);
		Rect polygonRect = polygon.GetBounds();

		if (sliceRect.Overlaps(polygonRect) == false) {
			return(result);
		}

		if (Slicer2D.complexSliceType != Slicer2D.SliceType.Regular) {
			result = SlicePolygonInside (polygon, slice);
			if (result.polygons.Count > 0) {
				return(result);
			}
		}

		// Optimization (holes?)
		// if (polygon.SliceIntersectPoly (slice) == false)
		//	return(result);

		List<List<Vector2D>> slices = new List<List<Vector2D>>();

		bool entered = polygon.PointInPoly (slice.First ());
	
		List<Vector2D> currentSlice = new List<Vector2D> ();

		foreach (Pair2D pair in Pair2D.GetList(slice, false)) {
			List<Vector2D> stackList = polygon.GetListLineIntersectPoly(pair);
			stackList = Vector2DList.GetListSortedToPoint (stackList, pair.A);

			foreach (Vector2D id in stackList) {
				if (entered == true) {
					currentSlice.Add (id);
					slices.Add (currentSlice);
				} else {
					currentSlice = new List<Vector2D> ();
					currentSlice.Add (id);
				}
				entered = !entered;
			}

			if (entered == true)
				currentSlice.Add (pair.B);
		}

		// Adjusting split lines before performing convex split
		result.AddPolygon(polygon);

		foreach (List<Vector2D> id in slices) 
			if (id.Count > 1) {
				foreach (Vector2D p in id) {
					result.AddCollision (p);
				}

				// Sclice line points generated from intersections list
				Vector2D vec0 = id.First();
				vec0.Push (Vector2D.Atan2 (vec0, id[1]), precision);

				Vector2D vec1 = id.Last();
				vec1.Push (Vector2D.Atan2 (vec1, id[id.Count - 2]), precision);

				// For each in polygons list attempt convex split
				List<Polygon2D> temp = new List<Polygon2D>(result.polygons); // necessary?
				foreach (Polygon2D poly in temp) {
					Slice2D resultList = SingleSlice(poly, id); 

					if (resultList.slices.Count > 0) {
						foreach (List<Vector2D> i in resultList.slices) {
							result.AddSlice(i);
						}
					}

					if (resultList.polygons.Count > 0) {
						foreach (Polygon2D i in resultList.polygons) {
							result.AddPolygon(i);
						}

						// If it's possible to perform convex split, remove parent polygon from result list
						result.RemovePolygon(poly);
					}
				}
			}
		result.RemovePolygon (polygon);
		return(result);
	}

	//if (slice.Count == 2) return(PolygonSimpleSlicer.SplitLineConcave (polygon, new Pair2D (slice.First (), slice.Last ())));
	static private Slice2D SingleSlice(Polygon2D polygon, List<Vector2D> slice)
	{
		Slice2D result = Slice2D.Create (slice);

		if (Slicer2D.complexSliceType == Slicer2D.SliceType.Regular) {
			if (Math2D.SliceIntersectItself(slice)) {
				Slicer2D.Debug.LogError("Slice Intersect Itself In Regular Mode");
				return(result);
			}
		}

		if (polygon.PointInPoly (slice.First ()) == true || polygon.PointInPoly (slice.Last ()) == true) {
			return(result);
		}

		slice = new List<Vector2D> (slice);

		List <Vector2D> collisionSlice = new List<Vector2D> ();

		int collisionCount = 0;
		bool enterCollision = false;

		// Generate correct intersected slice
		foreach (Pair2D pair in Pair2D.GetList(slice, false)) {
			List<Vector2D> intersections = polygon.GetListLineIntersectPoly(pair);

			if (intersections.Count > 0) {
				if (intersections.Count == 1) {
					collisionCount += 1;
					enterCollision = !enterCollision;
				}
				if (intersections.Count == 2) {
					collisionCount += intersections.Count; // Check if it's okay
				}

				if (intersections.Count > 2) {
					//Debug.LogError ("Slicer2D: Unexpected Error 2"); // When this happens?
					return(result);
				}
			}

			if (enterCollision == true) {
				collisionSlice.Add (pair.B);
			}
		}

		List<Polygon2D> intersectHoles = polygon.SliceIntersectHoles (slice);

		switch (intersectHoles.Count) {
			case 0:
				if (collisionCount == 2) {
					return(SliceWithoutHoles (polygon, slice, collisionSlice));
				}
				break;

			case 1:
				return(SliceWithOneHole(polygon, slice, collisionSlice));

			case 2:
				return(SliceWithTwoHoles (polygon, slice, collisionSlice));

			default:
				break;
			}

		return(result);
	}

	static private Slice2D SliceWithOneHole(Polygon2D polygon, List<Vector2D> slice, List<Vector2D> collisionSlice) {
		Slice2D result = Slice2D.Create (slice);
	
		Polygon2D holeA = polygon.PointInHole (slice.First ());
		Polygon2D holeB = polygon.PointInHole (slice.Last ());
		Polygon2D holePoly = (holeA != null) ? holeA : holeB;

		if (polygon.PointInPoly (slice.First ()) == false || polygon.PointInPoly (slice.Last ()) == false) { 
			if (holeA == holeB) { 

				if (Slicer2D.complexSliceType == Slicer2D.SliceType.Regular) {
					return(result);
				}

				if (holeA == null) {
					Slicer2D.Debug.LogError ("This happened when collider had a lot of paths but they were not holes");
					return(result);
				}

				List<Vector2D> slice2 = new List<Vector2D> (slice);

				Polygon2D polyA = new Polygon2D (polygon.pointsList);
				Polygon2D polyB = new Polygon2D (slice);
				Polygon2D polyC = new Polygon2D (slice2);

				// Get First Point - NOT FINISHED WITH INTERSECTION
				int Add;
				List<Vector2D> list;
				List<Pair2D> iterateList = Pair2D.GetList (holeA.pointsList);

				Add = 0;
				list = new List<Vector2D> ();
				foreach (Pair2D pair in iterateList) {
					List<Vector2D> intersect = Math2D.GetListLineIntersectSlice (pair, slice);
					if (intersect.Count > 0) {
						Add += intersect.Count;
					}

					if (Add == 1) {
						list.Add (pair.B);
					}
				}

				if (list.Count > 0) {
					if (Vector2D.Distance (list.First (), slice.First ()) < Vector2D.Distance (list.First (), slice.Last ())) {
						slice.Reverse ();
					}

					polyB.AddPoints (list);
				}

				Add = 0;
				list = new List<Vector2D> ();
				foreach (Pair2D pair in iterateList) {
					List<Vector2D> intersect = Math2D.GetListLineIntersectSlice (pair, slice2);
					if (intersect.Count > 0) {
						Add += intersect.Count;
					}

					if (Add == 2) {
						list.Add (pair.B);
					}
				}

				foreach (Pair2D pair in iterateList) {
					List<Vector2D> intersect = Math2D.GetListLineIntersectSlice (pair, slice2);
					if (intersect.Count > 0) {
						Add += intersect.Count;
					}

					if (Add == 2) {
						list.Add (pair.B);
					}
				}

				if (list.Count > 0) {
					if (Vector2D.Distance (list.First (), slice2.First ()) < Vector2D.Distance (list.First (), slice2.Last ())) {
						slice2.Reverse ();
					}

					polyC.AddPoints (list); 
				}

				if (polyB.GetArea () > polyC.GetArea ()) {
					Polygon2D swap = polyB;
					polyB = polyC;
					polyC = swap;
				}

				// Add holes to new created polygon
				foreach (Polygon2D poly in polygon.holesList) {
					if (poly != holeA && polyB.PolyInPoly(poly) == true) {
						polyB.AddHole (poly);
					}
				}

				if (Slicer2D.complexSliceType == Slicer2D.SliceType.FillSlicedHole) {
					result.AddPolygon (polyB);
				}

				polyA.AddHole (polyC);

				// Adds polygons if they are not in the hole
				foreach (Polygon2D poly in polygon.holesList) { // Check for errors?
					if (poly != holeA && polyC.PolyInPoly(poly) == false) {
						polyA.AddHole (poly);
					}
				}

				result.AddPolygon (polyA);

				result.AddSlice(collisionSlice);
				return(result);

			} else if (holePoly != null) {
				Polygon2D polyA = new Polygon2D ();
				Polygon2D polyB = new Polygon2D (holePoly.pointsList);
				polyB.pointsList.Reverse ();

				List<Vector2D> pointsA = Vector2DList.GetListStartingIntersectSlice (polygon.pointsList, slice);
				List<Vector2D> pointsB = Vector2DList.GetListStartingIntersectSlice (polyB.pointsList, slice);

				if (pointsA.Count < 1) {
					Slicer2D.Debug.LogError (pointsA.Count + " " + polygon.pointsList.Count);
				}

				polyA.AddPoints (pointsA);

				if (collisionSlice.Count > 0) {
					// pointsA empty
					if (Vector2D.Distance (pointsA.Last (), collisionSlice.Last ()) < Vector2D.Distance (pointsA.Last (), collisionSlice.First ())) {
						collisionSlice.Reverse ();
					}

					polyA.AddPoints (collisionSlice);
				}

				polyA.AddPoints (pointsB);

				if (collisionSlice.Count > 0) {
					collisionSlice.Reverse ();
					polyA.AddPoints (collisionSlice);
				}

				foreach (Polygon2D poly in polygon.holesList) {// Check for errors?
					if (poly != holePoly) {
						polyA.AddHole (poly);
					}
				}

				result.AddPolygon (polyA);

				result.AddSlice(collisionSlice);
				return(result);
			}
		}
			
		return(result);
	}

	static private Slice2D SliceWithTwoHoles(Polygon2D polygon, List<Vector2D> slice, List<Vector2D> collisionSlice) {
		Slice2D result = Slice2D.Create (slice);

		Polygon2D polyA = new Polygon2D ();
		Polygon2D polyB = new Polygon2D (polygon.pointsList);

		Polygon2D holeA = polygon.PointInHole (slice.First ());
		Polygon2D holeB = polygon.PointInHole (slice.Last ());

		if (holeA == null || holeB == null) {
			Slicer2D.Debug.LogError ("ERROR Split"); // Shouldn't really happen
			return(result);
		}

		List<Vector2D> pointsA = Vector2DList.GetListStartingIntersectSlice (holeA.pointsList, slice);
		List<Vector2D> pointsB = Vector2DList.GetListStartingIntersectSlice (holeB.pointsList, slice);

		polyA.AddPoints (pointsA);

		if (collisionSlice.Count > 0) {
			if (Vector2D.Distance (pointsA.Last (), collisionSlice.Last ()) < Vector2D.Distance (pointsA.Last (), collisionSlice.First ()))
				collisionSlice.Reverse ();

			polyA.AddPoints (collisionSlice);
		}

		polyA.AddPoints (pointsB);

		if (collisionSlice.Count > 0) {
			collisionSlice.Reverse ();

			polyA.AddPoints (collisionSlice);
		}

		foreach (Polygon2D poly in polygon.holesList) {
			if (poly != holeA && poly != holeB) {
				polyB.AddHole (poly);
			}
		}

		polyB.AddHole (polyA);
		result.AddPolygon (polyB);

		result.AddSlice(collisionSlice);
		return(result);
	}

	static private Slice2D SliceWithoutHoles(Polygon2D polygon, List<Vector2D> slice, List<Vector2D> collisionSlice) {
		Slice2D result = Slice2D.Create (slice);

		// Simple non-hole slice
		Polygon2D polyA = new Polygon2D();
		Polygon2D polyB = new Polygon2D();

		Polygon2D currentPoly = polyA;

		foreach (Pair2D p in Pair2D.GetList(polygon.pointsList)) {
			List <Vector2D> intersections = Math2D.GetListLineIntersectSlice (p, slice);

			Vector2D intersection = null;
			if (intersections.Count () > 0) {
				intersection = intersections.First ();
			}

			if (intersections.Count > 0) { // Only if 1 or 2
				if (intersections.Count == 2) { 
					Vector2D first = intersections.First ();
					Vector2D last = intersections.Last ();

					if (Vector2D.Distance (last, p.A) < Vector2D.Distance (first, p.A)) {
						first = intersections.Last ();
						last = intersections.First ();
					}

					currentPoly.AddPoint (first);

					if (collisionSlice.Count > 0) {
						if (Vector2D.Distance (first, collisionSlice.Last ()) < Vector2D.Distance (first, collisionSlice.First ())) {
							collisionSlice.Reverse ();
						}

						currentPoly.AddPoints (collisionSlice);
					}

					currentPoly.AddPoint (last);

					currentPoly = polyB;

					if (collisionSlice.Count > 0) {
						currentPoly.AddPoints (collisionSlice);
					}
	
					currentPoly.AddPoint (last);
					currentPoly.AddPoint (first);

					currentPoly = polyA;
				}

				if (intersections.Count == 1) {
					currentPoly.AddPoint (intersection);

					if (collisionSlice.Count > 0) {
						if (Vector2D.Distance (intersection, collisionSlice.Last ()) < Vector2D.Distance (intersection, collisionSlice.First ())) {
							collisionSlice.Reverse ();
						}

						currentPoly.AddPoints (collisionSlice);
					}
						
					currentPoly = (currentPoly == polyA) ? polyB : polyA;

					currentPoly.AddPoint (intersection);

				}
			}

			currentPoly.AddPoint (p.B);
		}

		result.AddPolygon (polyA);
		result.AddPolygon (polyB);

		foreach (Polygon2D poly in result.polygons) {
			foreach (Polygon2D hole in polygon.holesList) {
				if (poly.PolyInPoly (hole) == true) {
					poly.AddHole (hole);	
				}
			}
		}

		result.AddSlice(collisionSlice);
		return(result);
	}


	static private Slice2D SlicePolygonInside(Polygon2D polygon, List<Vector2D> slice) // Create Polygon Inside?
	{
		Slice2D result = Slice2D.Create (slice);

		Polygon2D newPoly = new Polygon2D ();

		bool createPoly = false;
		foreach (Pair2D pairA in Pair2D.GetList(slice, false)) {
			foreach (Pair2D pairB in Pair2D.GetList(slice, false)) {
				Vector2D intersection = Math2D.GetPointLineIntersectLine (pairA, pairB);
				if (intersection != null && (pairA.A != pairB.A && pairA.B != pairB.B && pairA.A != pairB.B && pairA.B != pairB.A)) {
					createPoly = !createPoly;
					newPoly.AddPoint (intersection);
				}
			}
			if (createPoly == true) {
				newPoly.AddPoint (pairA.B);
			}
		}

		bool inHoles = false;
		foreach (Polygon2D hole in polygon.holesList) {
			if (hole.PolyInPoly (newPoly) == true) {
				inHoles = true;
			}
		}

		if (inHoles == false && newPoly.pointsList.Count > 2 && polygon.PolyInPoly (newPoly) == true) {
			polygon.AddHole (newPoly);
			
			List <Polygon2D> polys = new List<Polygon2D> (polygon.holesList);
			foreach (Polygon2D hole in polys) {
				if (newPoly.PolyInPoly (hole) == true) {
					polygon.holesList.Remove (hole);
					newPoly.AddHole (hole);
				}
			}

			result.AddPolygon (polygon);

			if (Slicer2D.complexSliceType == Slicer2D.SliceType.FillSlicedHole) {
				result.AddPolygon (newPoly);
			}
			
			return(result);
		}

		return(result);
	}
}
