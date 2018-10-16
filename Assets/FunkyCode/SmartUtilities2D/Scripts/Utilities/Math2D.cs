using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Math2D {

	public static Rect GetBounds(List<Vector2D> pointsList)
	{
		float rMinX = 1e+10f;
		float rMinY = 1e+10f;
		float rMaxX = -1e+10f;
		float rMaxY = -1e+10f;

		foreach (Vector2D id in pointsList) {
			rMinX = Mathf.Min (rMinX, id.vector.x);
			rMinY = Mathf.Min (rMinY, id.vector.y);
			rMaxX = Mathf.Max (rMaxX, id.vector.x);
			rMaxY = Mathf.Max (rMaxY, id.vector.y);
		}

		return(new Rect(rMinX, rMinY, Mathf.Abs(rMinX - rMaxX), Mathf.Abs(rMinY - rMaxY))); 
	}

	public static Rect GetBounds(Pair2D pair)
	{
		float rMinX = 1e+10f;
		float rMinY = 1e+10f;
		float rMaxX = -1e+10f;
		float rMaxY = -1e+10f;

		Vector2D id = pair.A;
		rMinX = Mathf.Min (rMinX, id.vector.x);
		rMinY = Mathf.Min (rMinY, id.vector.y);
		rMaxX = Mathf.Max (rMaxX, id.vector.x);
		rMaxY = Mathf.Max (rMaxY, id.vector.y);

	
		id = pair.B;
		rMinX = Mathf.Min (rMinX, id.vector.x);
		rMinY = Mathf.Min (rMinY, id.vector.y);
		rMaxX = Mathf.Max (rMaxX, id.vector.x);
		rMaxY = Mathf.Max (rMaxY, id.vector.y);

		return(new Rect(rMinX, rMinY, Mathf.Abs(rMinX - rMaxX), Mathf.Abs(rMinY - rMaxY))); 
	}


	public static bool PolyInPoly(Polygon2D polyA, Polygon2D polyB)
	{
		foreach (Pair2D p in Pair2D.GetList(polyB.pointsList)) {
			if (PointInPoly (p.A, polyA) == false) {
				return(false);
			}
		}

		if (PolyIntersectPoly (polyA, polyB) == true) {
			return(false);
		}
		
		return(true);
	}

	// Is it not finished?
	public static bool PolyCollidePoly(Polygon2D polyA, Polygon2D polyB)
	{
		if (PolyIntersectPoly (polyA, polyB) == true) {
			return(true);
		}

		if (PolyInPoly (polyA, polyB) == true) {
			return(true);
		}

		if (PolyInPoly (polyB, polyA) == true) {
			return(true);
		}
		
		return(false);
	}

	public static bool PolyIntersectPoly(Polygon2D polyA, Polygon2D polyB)
	{
		foreach (Pair2D a in Pair2D.GetList(polyA.pointsList)) {
			foreach (Pair2D b in Pair2D.GetList(polyB.pointsList)) {
				if (LineIntersectLine (a, b)) {
					return(true);
				}
			}
		}

		return(false);
	}

	public static bool SliceIntersectPoly(List <Vector2D> slice, Polygon2D poly)
	{
		Pair2D pairA = new Pair2D(null,  null);
		foreach (Vector2D pointA in slice) {
			pairA.B = pointA;
			
			if (pairA.A != null && pairA.B != null) {
				Pair2D pairB = new Pair2D(new Vector2D(poly.pointsList.Last()),  null);
				foreach (Vector2D pointB in poly.pointsList) {
					pairB.B = pointB;

					if (LineIntersectLine (pairA, pairB)) {
						return(true);
					}

					pairB.A = pointB;
				}
			}

			pairA.A = pointA;
		}

		return(false);
	}

	public static bool SliceIntersectSlice(List <Vector2D> sliceA, List <Vector2D> sliceB)
	{
		Pair2D pairA = new Pair2D(null,  null);
		foreach (Vector2D pointA in sliceA) {
			pairA.B = pointA;

			if (pairA.A != null && pairA.B != null) {

				Pair2D pairB = new Pair2D(null,  null);
				foreach (Vector2D pointB in sliceB) {
					pairB.B = pointB;

					if (pairB.A != null && pairB.B != null) {
						if (LineIntersectLine (pairA, pairB)) {
							return(true);
						}
					}

					pairB.A = pointB;
				}
			}

			pairA.A = pointA;
		}

		return(false);
	}
		
	public static bool LineIntersectPoly(Pair2D line, Polygon2D poly)
	{
		Pair2D pair = new Pair2D(new Vector2D(poly.pointsList.Last()), new Vector2D(Vector2.zero));
		foreach (Vector2D point in poly.pointsList) {
			pair.B = point;

			if (LineIntersectLine (line, pair)) {
				return(true);
			}
			
			pair.A = point;
		}
		
		return(false);
	}

	public static bool LineIntersectLine(Pair2D lineA, Pair2D lineB)
	{
		if (GetPointLineIntersectLine (lineA, lineB) != null) {
			return(true);
		}

		return(false);
	}

	public static bool SliceIntersectItself(List<Vector2D> slice)
	{
		Pair2D pairA = new Pair2D(null,  null);
		foreach (Vector2D va in slice) {
			pairA.B = va;

			if (pairA.A != null && pairA.B != null) {

				Pair2D pairB = new Pair2D(null,  null);
				foreach (Vector2D vb in slice) {
					pairB.B = vb;

					if (pairB.A != null && pairB.B != null) {
						if (GetPointLineIntersectLine (pairA, pairB) != null) {
							if (pairA.A != pairB.A && pairA.B != pairB.B && pairA.A != pairB.B && pairA.B != pairB.A) {
								return(true);
							}
						}
					}
					pairB.A = vb;
				}
			}
			
			pairA.A = va;
		}
		
		return(false);
	}

	public static Vector2D GetPointLineIntersectLine(Pair2D lineA, Pair2D lineB)
	{
		float ay_cy, ax_cx, px, py;
		float dx_cx = lineB.B.vector.x - lineB.A.vector.x;
		float dy_cy = lineB.B.vector.y - lineB.A.vector.y;
		float bx_ax = lineA.B.vector.x - lineA.A.vector.x;
		float by_ay = lineA.B.vector.y - lineA.A.vector.y;
		float de = bx_ax * dy_cy - by_ay * dx_cx;
		float tor = 1E-10f;

		if (Mathf.Abs(de) < 0.01f) {
			return(null);
		}	

		if (de > - tor && de < tor) {
			return(null);
		}

		ax_cx = lineA.A.vector.x - lineB.A.vector.x;
		ay_cy = lineA.A.vector.y - lineB.A.vector.y;

		float r = (ay_cy * dx_cx - ax_cx * dy_cy) / de;
		float s = (ay_cy * bx_ax - ax_cx * by_ay) / de;

		px = lineA.A.vector.x + r * bx_ax;
		py = lineA.A.vector.y + r * by_ay;

		if ((r < 0) || (r > 1) || (s < 0)|| (s > 1)) {
			return(null);
		}

		return(new Vector2D (px, py));
	}

	public static bool PointInPoly(Vector2D point, Polygon2D poly)
	{
		if (poly.pointsList.Count < 3) {
			return(false);
		}

		int total = 0;
		int diff = 0;

		Pair2D id = new Pair2D(new Vector2D(poly.pointsList.Last()), null);
		foreach (Vector2D p in poly.pointsList) {
			id.B = p;

			diff = (GetQuad (point, id.A) - GetQuad (point, id.B));

			switch (diff) {
				case -2: case 2:
					if ((id.B.vector.x - (((id.B.vector.y - point.vector.y) * (id.A.vector.x - id.B.vector.x)) / (id.A.vector.y - id.B.vector.y))) < point.vector.x)
						diff = -diff;

					break;

				case 3:
					diff = -1;
					break;

				case -3:
					diff = 1;
					break;

				default:
					break;   
			}

			total += diff;

			id.A = id.B;
		}

		return(Mathf.Abs(total) == 4);
	}

	private static int GetQuad(Vector2D axis, Vector2D vert)
	{
		if (vert.vector.x < axis.vector.x) {
			if (vert.vector.y < axis.vector.y) {
				return(1);
			}
			return(4);
		}
		if (vert.vector.y < axis.vector.y) {
			return(2);
		}
		return(3);
	}
		
	// Getting List is Slower
	public static List <Vector2D> GetListLineIntersectPoly(Pair2D line, Polygon2D poly)
	{
		List <Vector2D> result = new List <Vector2D>() ;

		Pair2D pair = new Pair2D(new Vector2D(poly.pointsList.Last()),  null);
		foreach (Vector2D point in poly.pointsList) {
			pair.B = point;

			Vector2D intersection = GetPointLineIntersectLine (line, pair);
			if (intersection != null) {
				result.Add(intersection);
			}

			pair.A = point;
		}
		return(result);
	}

	public static List<Vector2D> GetListLineIntersectSlice(Pair2D pair, List<Vector2D> slice)
	{
		List<Vector2D> resultList = new List<Vector2D> ();
		
		Pair2D id = new Pair2D(null,  null);
		foreach (Vector2D point in slice) {
			id.B = point;

			if (id.A != null && id.B != null) {
				Vector2D result = GetPointLineIntersectLine(id, pair);
				if (result != null) {
					resultList.Add(result);
				}
			}

			id.A = point;
		}
		return(resultList);
	}

	public static Vector2 ReflectAngle(Vector2 v, float wallAngle)
	{
		//normal vector to the wall
		Vector2 n = new Vector2(Mathf.Cos(wallAngle + Mathf.PI / 2), Mathf.Sin(wallAngle + Mathf.PI / 2));

		// p is the projection of V onto the normal
		float dotproduct = v.x * n.x + v.y * n.y;

		// the velocity after hitting the wall is V - 2p, so just subtract 2*p from V
		return(new Vector2(v.x - 2f * (dotproduct * n.x), v.y - 2f * (dotproduct * n.y)));
	}

	static public bool PolygonIntersectCircle(Polygon2D poly, Vector2D circle, float radius) {
		foreach (Pair2D id in Pair2D.GetList(poly.pointsList)) {
			if (LineIntersectCircle(id, circle, radius) == true) {
				return(true);
			}
		}
		return(false);
	}
	
	static public bool SliceIntersectCircle(List<Vector2D> points, Vector2D circle, float radius) {
		foreach (Pair2D id in Pair2D.GetList(points, false)) {
			if (LineIntersectCircle(id, circle, radius) == true) {
				return(true);
			}
		}
		return(false);
	}

	static public bool LineIntersectCircle(Pair2D line, Vector2D circle, float radius)
	{
		float sx = line.B.vector.x - line.A.vector.x;
		float sy = line.B.vector.y - line.A.vector.y;

		float q = ((circle.vector.x - line.A.vector.x) * (line.B.vector.x - line.A.vector.x) + (circle.vector.y - line.A.vector.y) * (line.B.vector.y - line.A.vector.y)) / (sx * sx + sy * sy);
			
		if (q < 0.0f) {
			q = 0.0f;
		} else if (q > 1.0) {
			q = 1.0f;
		}

		float dx = circle.vector.x - ( (1.0f - q) * line.A.vector.x + q * line.B.vector.x );
		float dy = circle.vector.y - ( (1.0f - q) * line.A.vector.y + q * line.B.vector.y );

		if (dx * dx + dy * dy < radius * radius) {
			return(true);
		} else {
			return(false);
		}
	}
}
