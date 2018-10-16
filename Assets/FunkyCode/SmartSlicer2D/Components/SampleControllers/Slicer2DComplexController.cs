﻿using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Slicer2DComplexController : MonoBehaviour {
	// Physics Force
	public bool addForce = true;
	public float addForceAmount = 5f;

	// Controller Visuals
	public bool drawSlicer = true;
	public float lineWidth = 1.0f;
	public float zPosition = 0f;
	public Color slicerColor = Color.black;

	// Mouse Events
	private static List<List<Vector2D>> complexEvents = new List<List<Vector2D>>();
	private static List<Vector2D> complexPairs = new List<Vector2D>();
	private float minVertsDistance = 1f;

	private bool mouseDown = false;

	// Complex Slice Type
	public Slicer2D.SliceType complexSliceType = Slicer2D.SliceType.SliceHole;

	public void OnRenderObject() {
		if (drawSlicer == false)
			return;

		if (mouseDown) {
			Max2D.SetBorder(true);
			Max2D.SetLineMode(Max2D.LineMode.Smooth);
			Max2D.SetLineWidth (lineWidth * .5f);
			Max2D.SetColor (slicerColor);

			if (complexPairs.Count > 0) {
				Max2D.DrawStrippedLine (complexPairs, minVertsDistance, zPosition);
				Max2D.DrawLineSquare (complexPairs.Last(), 0.5f, zPosition);
				Max2D.DrawLineSquare (complexPairs.First (), 0.5f, zPosition);
			}
		}
	}

	public void LateUpdate()
	{
		complexEvents.Clear ();

		// Checking mouse press and release events to get linear slices based on input
		Vector2D pos = new Vector2D (Camera.main.ScreenToWorldPoint (Input.mousePosition));

		if (Input.GetMouseButtonDown (0)) {
			complexPairs.Clear ();
			complexPairs.Add (pos);
		}

		if (Input.GetMouseButton (0)) {
			Vector2D posMove = new Vector2D (complexPairs.Last ());
			while ((Vector2D.Distance (posMove, pos) > minVertsDistance)) {
				float direction = Vector2D.Atan2 (pos, posMove);
				posMove.Push (direction, minVertsDistance);
				complexPairs.Add (new Vector2D (posMove));
			}
			mouseDown = true;
		}

		if (mouseDown == true && Input.GetMouseButton (0) == false) {
			mouseDown = false;
			Slicer2D.complexSliceType = complexSliceType;
			ComplexSlice (complexPairs);
			complexEvents.Add (complexPairs);
		}
	}

	private void ComplexSlice(List <Vector2D> slice)
	{
		List<Slice2D> results = Slicer2D.ComplexSliceAll (slice, null);
		if (addForce == true) {
			foreach (Slice2D id in results) {
				foreach (GameObject gameObject in id.gameObjects) {
					Rigidbody2D rigidBody2D = gameObject.GetComponent<Rigidbody2D> ();
					if (rigidBody2D) {
						foreach (Pair2D p in Pair2D.GetList(id.collisions)) {
							float sliceRotation = Vector2D.Atan2 (p.B, p.A);
							Physics2DHelper.AddForceAtPosition(rigidBody2D, new Vector2 (Mathf.Cos (sliceRotation) * addForceAmount, Mathf.Sin (sliceRotation) * addForceAmount), (p.A.vector + p.B.vector) / 2f);
						}
					}
				}
			}
		}
	}
}