using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Max2DMatrix {
	const float pi = Mathf.PI;
	const float pi2 = pi / 2;
	const float uv0 = 1f / 32;
	const float uv1 = 1f - uv0;

	public static void DrawSlice(List<Vector2D> list, float z, bool connect = false) {
		foreach (Pair2D p in Pair2D.GetList(list, connect)) {
			GL.Vertex3(p.A.vector.x, p.A.vector.y, z);
			GL.Vertex3(p.B.vector.x, p.B.vector.y, z);
		}
	}

	public static void DrawTriangle(float x0, float y0, float x1, float y1, float x2, float y2, Vector2D offset, float z = 0f) {
		GL.Vertex3(x0 + offset.vector.x, y0 + offset.vector.y, z);
		GL.Vertex3(x1 + offset.vector.x, y1 + offset.vector.y, z);
		GL.Vertex3(x2 + offset.vector.x, y2 + offset.vector.y, z);
	}

	public static void DrawLine(float x0, float y0, float x1, float y1, float z = 0f) {
		GL.Vertex3(x0, y0, z);
		GL.Vertex3(x1, y1, z);
	}

	public static void DrawSliceImage(List<Vector2D> list, float z, bool connect = false) {
		foreach (Pair2D p in Pair2D.GetList(list, connect)) {
			DrawLineImage (p, z);
		}
	}

	public static void DrawLineImage(Pair2D pair, float z = 0f) {
		float size = Max2D.lineWidth * Max2D.setScale;

		float rot = Vector2D.Atan2 (pair.A, pair.B);

		Vector2D A1 = new Vector2D (pair.A);
		Vector2D A2 = new Vector2D (pair.A);
		Vector2D B1 = new Vector2D (pair.B);
		Vector2D B2 = new Vector2D (pair.B);

		A1.Push (rot + pi2, size);
		A2.Push (rot - pi2, size);
		B1.Push (rot + pi2, size);
		B2.Push (rot - pi2, size);

		GL.TexCoord2(0.5f + uv0, 0);
		GL.Vertex3(B1.vector.x, B1.vector.y, z);
		GL.TexCoord2(uv1, 0);
		GL.Vertex3(A1.vector.x, A1.vector.y, z);
		GL.TexCoord2(uv1, 1);
		GL.Vertex3(A2.vector.x, A2.vector.y, z);
		GL.TexCoord2(0.5f + uv0, 1);
		GL.Vertex3(B2.vector.x, B2.vector.y, z);

		A1 = new Vector2D (pair.A);
		A2 = new Vector2D (pair.A);
		Vector2D A3 = new Vector2D (pair.A);
		Vector2D A4 = new Vector2D (pair.A);

		A1.Push (rot + pi2, size);
		A2.Push (rot - pi2, size);

		A3.Push (rot + pi2, size);
		A4.Push (rot - pi2, size);
		A3.Push (rot + pi, -size);
		A4.Push (rot + pi, -size);

		GL.TexCoord2(uv0, 0);
		GL.Vertex3(A3.vector.x, A3.vector.y, z);
		GL.TexCoord2(uv0, 1);
		GL.Vertex3(A4.vector.x, A4.vector.y, z);
		GL.TexCoord2(0.5f - uv0, 1);
		GL.Vertex3(A2.vector.x, A2.vector.y, z);
		GL.TexCoord2(0.5f - uv0, 0);
		GL.Vertex3(A1.vector.x, A1.vector.y, z);

		B1 = new Vector2D (pair.B);
		B2 = new Vector2D (pair.B);
		Vector2D B3 = new Vector2D (pair.B);
		Vector2D B4 = new Vector2D (pair.B);

		B1.Push (rot + pi2, size);
		B2.Push (rot - pi2, size);

		B3.Push (rot + pi2, size);
		B4.Push (rot - pi2, size);
		B3.Push (rot + pi, size);
		B4.Push (rot + pi , size);

		GL.TexCoord2(uv0, 0);
		GL.Vertex3(B4.vector.x, B4.vector.y, z);
		GL.TexCoord2(uv0, 1);
		GL.Vertex3(B3.vector.x, B3.vector.y, z);
		GL.TexCoord2(0.5f - uv0, 1);
		GL.Vertex3(B1.vector.x, B1.vector.y, z);
		GL.TexCoord2(0.5f - uv0, 0);
		GL.Vertex3(B2.vector.x, B2.vector.y, z);
	}

	
	public static void DrawLineImage(Transform transform, Pair2D pair, float z = 0f) {
		float size = Max2D.lineWidth * Max2D.setScale;

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

		GL.TexCoord2(0.5f + uv0, 0);
		GL.Vertex3(B1.vector.x, B1.vector.y, z);
		GL.TexCoord2(uv1, 0);
		GL.Vertex3(A1.vector.x, A1.vector.y, z);
		GL.TexCoord2(uv1, 1);
		GL.Vertex3(A2.vector.x, A2.vector.y, z);
		GL.TexCoord2(0.5f + uv0, 1);
		GL.Vertex3(B2.vector.x, B2.vector.y, z);

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

		GL.TexCoord2(uv0, 0);
		GL.Vertex3(A3.vector.x, A3.vector.y, z);
		GL.TexCoord2(uv0, 1);
		GL.Vertex3(A4.vector.x, A4.vector.y, z);
		GL.TexCoord2(0.5f - uv0, 1);
		GL.Vertex3(A2.vector.x, A2.vector.y, z);
		GL.TexCoord2(0.5f - uv0, 0);
		GL.Vertex3(A1.vector.x, A1.vector.y, z);

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

		GL.TexCoord2(uv0, 0);
		GL.Vertex3(B4.vector.x, B4.vector.y, z);
		GL.TexCoord2(uv0, 1);
		GL.Vertex3(B3.vector.x, B3.vector.y, z);
		GL.TexCoord2(0.5f - uv0, 1);
		GL.Vertex3(B1.vector.x, B1.vector.y, z);
		GL.TexCoord2(0.5f - uv0, 0);
		GL.Vertex3(B2.vector.x, B2.vector.y, z);
	}

	public static void DrawSliceImage(Transform transform, List<Vector2D> list, float z, bool connect = false) {
		foreach (Pair2D p in Pair2D.GetList(list, connect)) {
			DrawLineImage (transform, p, z);
		}
	}
}
