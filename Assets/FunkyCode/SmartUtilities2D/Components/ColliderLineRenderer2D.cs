using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ColliderLineRenderer2D : MonoBehaviour {
	public Max2D.LineMode lineMode = Max2D.LineMode.Smooth;
	public Color color = Color.white;
	public float lineWidth = 1;

	private bool connectedLine = true; // Prototype

	private float lineOffset = -0.01f;
	private Polygon2D polygon = null;
	private Mesh mesh = null;

	const float pi = Mathf.PI;
	const float pi2 = pi / 2;
	const float uv0 = 1f / 32;
	const float uv1 = 1f - uv0;

	private float lineWidthSet = 1;

	public Polygon2D GetPolygon() {
		if (polygon == null) {
			polygon = Polygon2D.CreateFromCollider (gameObject);
		}
		return(polygon);
	}

	private void GenerateMesh() {
		mesh = new Mesh();

		lineWidthSet = lineWidth;

		List<LineTriangle> l = new List<LineTriangle>();

		foreach(Pair2D p in Pair2D.GetList(GetPolygon().pointsList, connectedLine)) {
			l.Add(DrawLineImage(p, lineOffset));
		}
		foreach(Polygon2D poly in GetPolygon().holesList) {
			foreach(Pair2D p in Pair2D.GetList(poly.pointsList, connectedLine)) {
				l.Add(DrawLineImage(p, lineOffset));
			}
		}
		
		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();
		List<int> triangles = new List<int>();

		int count = 0;
		foreach(LineTriangle t in l) {
			foreach(Vector3 v in t.vertices) {
				vertices.Add(v);
			}
			foreach(Vector2 u in t.uv) {
				uv.Add(u);
			}
			
			triangles.Add(count + 0);
			triangles.Add(count + 1);
			triangles.Add(count + 2);

			triangles.Add(count + 4);
			triangles.Add(count + 5);
			triangles.Add(count + 6);

			triangles.Add(count + 8);
			triangles.Add(count + 9);
			triangles.Add(count + 10);

			triangles.Add(count + 3);
			triangles.Add(count + 0);
			triangles.Add(count + 2);

			triangles.Add(count + 7);
			triangles.Add(count + 4);
			triangles.Add(count + 6);

			triangles.Add(count + 11);
			triangles.Add(count + 8);
			triangles.Add(count + 10);
			
			count += 12;
		}

		mesh.vertices = vertices.ToArray();
		mesh.uv = uv.ToArray();
		mesh.triangles = triangles.ToArray();
	}

	public void Start() {
		if (GetComponent<EdgeCollider2D>() != null) {
			connectedLine = false;
		}

		GenerateMesh();
	}

	public void Update() {
		if (lineWidth != lineWidthSet) {
			GenerateMesh();
		}

		Vector3 position = transform.position;
		Quaternion rotation = transform.rotation;
		Vector3 scale = transform.lossyScale ;
		Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);

		Max2D.Check();
		Graphics.DrawMesh(mesh, matrix, Max2D.lineMaterial, 0);
	}

	public LineTriangle DrawLineImage(Pair2D pair, float z = 0f) {
		float size = lineWidth / 4;
		LineTriangle result = new LineTriangle();

		float rot = Vector2D.Atan2 (pair.A, pair.B);

		Vector2D A1 = new Vector2D (pair.A);
		Vector2D A2 = new Vector2D (pair.A);
		Vector2D B1 = new Vector2D (pair.B);
		Vector2D B2 = new Vector2D (pair.B);

		Vector2 scale = new Vector2(1f / transform.localScale.x, 1f / transform.localScale.y);

		A1.Push (rot + pi2, size, scale);
		A2.Push (rot - pi2, size, scale);
		B1.Push (rot + pi2, size, scale);
		B2.Push (rot - pi2, size, scale);


		result.uv.Add(new Vector2(0.5f + uv0, 0));
		result.vertices.Add(new Vector3(B1.vector.x, B1.vector.y, z));
		result.uv.Add(new Vector2(uv1, 0));
		result.vertices.Add(new Vector3(A1.vector.x, A1.vector.y, z));
		result.uv.Add(new Vector2(uv1, 1));
		result.vertices.Add(new Vector3(A2.vector.x, A2.vector.y, z));
		result.uv.Add(new Vector2(0.5f + uv0, 1));
		result.vertices.Add(new Vector3(B2.vector.x, B2.vector.y, z));
	
		A1 = new Vector2D (pair.A);
		A2 = new Vector2D (pair.A);
		Vector2D A3 = new Vector2D (pair.A);
		Vector2D A4 = new Vector2D (pair.A);

		A1.Push (rot + pi2, size, scale);
		A2.Push (rot - pi2, size, scale);

		A3.Push (rot + pi2, size, scale);
		A4.Push (rot - pi2, size, scale);
		A3.Push (rot + pi, -size, scale);
		A4.Push (rot + pi, -size, scale);

		result.uv.Add(new Vector2(uv0, 0));
		result.vertices.Add(new Vector3(A3.vector.x, A3.vector.y, z));
		result.uv.Add(new Vector2(uv0, 1));
		result.vertices.Add(new Vector3(A4.vector.x, A4.vector.y, z));
		result.uv.Add(new Vector2(0.5f - uv0, 1));
		result.vertices.Add(new Vector3(A2.vector.x, A2.vector.y, z));
		result.uv.Add(new Vector2(0.5f - uv0, 0));
		result.vertices.Add(new Vector3(A1.vector.x, A1.vector.y, z));

		B1 = new Vector2D (pair.B);
		B2 = new Vector2D (pair.B);
		Vector2D B3 = new Vector2D (pair.B);
		Vector2D B4 = new Vector2D (pair.B);

		B1.Push (rot + pi2, size, scale);
		B2.Push (rot - pi2, size, scale);

		B3.Push (rot + pi2, size, scale);
		B4.Push (rot - pi2, size, scale);
		B3.Push (rot + pi, size, scale);
		B4.Push (rot + pi , size, scale);

		result.uv.Add(new Vector2(uv0, 0));
		result.vertices.Add(new Vector3(B4.vector.x, B4.vector.y, z));
		result.uv.Add(new Vector2(uv0, 1));
		result.vertices.Add(new Vector3(B3.vector.x, B3.vector.y, z));
		result.uv.Add(new Vector2(0.5f - uv0, 1));
		result.vertices.Add(new Vector3(B1.vector.x, B1.vector.y, z));
		result.uv.Add(new Vector2(0.5f - uv0, 0));
		result.vertices.Add(new Vector3(B2.vector.x, B2.vector.y, z));

		return(result);
	}


}

public class LineTriangle {
	public List<Vector2> uv = new List<Vector2>();
	public List<Vector3> vertices = new List<Vector3>();
}