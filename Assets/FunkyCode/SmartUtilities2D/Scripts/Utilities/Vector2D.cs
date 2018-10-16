using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Representation of 2D points
/// </summary>
public class Vector2D {
	public Vector2 vector;

	/// <summary>
	/// Representation of 2D points
	/// </summary>
	public Vector2D(float x, float y)
	{
		vector = new Vector2 (x, y);
	}

	/// <summary>
	/// Representation of 2D points
	/// </summary>
	public Vector2D(Vector2D point)
	{
		vector = new Vector2(point.vector.x, point.vector.y);
	}
		
	/// <summary>
	/// Representation of 2D points
	/// </summary>
	public Vector2D(Vector2 point)
	{
		vector = new Vector2(point.x, point.y);
	}

	public Vector2D Copy()
	{
		return(new Vector2D(vector));
	}
		
	/// <summary>
	/// Set x and y components of an existing 2D vector
	/// </summary>
	public void Set(float x, float y)
	{
		vector.x = x;
		vector.y = y;
	}

	/// <summary>
	/// Set x and y components of an existing 2D vector
	/// </summary>
	public void Set (Vector2D point)
	{
		vector.x = point.vector.x;
		vector.y = point.vector.y;
	}

	/// <summary>
	/// Push 2D vector coordinates by given rotation and distance 
	/// </summary>
	public void Push(float rot, float distance)
	{
		Inc(Mathf.Cos(rot) * distance, Mathf.Sin(rot) * distance);
	}

	public void Push(float rot, float distance, Vector2 scale)
	{
		Inc(Mathf.Cos(rot) * distance * scale.x, Mathf.Sin(rot) * distance * scale.y);
	}

	/// <summary>
	/// Increase 2D vector coordinates by given x and y coordinates
	/// </summary>
	public void Inc (float x, float y)
	{
		vector.x += x;
		vector.y += y;
	}

	/// <summary>
	/// Decrease 2D vector coordinates by given x and y coordinates
	/// </summary>
	public void Dec (float x, float y)
	{
		vector.x -= x;
		vector.y -= y;
	}

	/// <summary>
	/// Increase 2D vector coordinates by given 2D vector
	/// </summary>
	public void Inc (Vector2D point)
	{
		vector.x += point.vector.x;
		vector.y += point.vector.y;
	}

	/// <summary>
	/// Decrease 2D vector coordinates by given 2D vector
	/// </summary>
	public void Dec (Vector2D point)
	{
		vector.x -= point.vector.x;
		vector.y -= point.vector.y;
	}

	/// <summary>
	/// Distance between given 2D vectors
	/// </summary>
	public static float Distance(Vector2D a, Vector2D b)
	{
		return(Vector2.Distance(a.vector, b.vector));
	}

	/// <summary>
	/// Angle between two given 2D coordinates
	/// </summary>
	public static float Atan2(Vector2D a, Vector2D b)
	{
		return(Mathf.Atan2 (a.vector.y - b.vector.y, a.vector.x - b.vector.x));
	}

	public static float Atan2(Vector2 a, Vector2 b)
	{
		return(Mathf.Atan2 (a.y - b.y, a.x - b.x));
	}

	public static Vector2D RotToVec(float rotation) {
		return(new Vector2D(Mathf.Cos(rotation), Mathf.Sin(rotation)));
	}
}
