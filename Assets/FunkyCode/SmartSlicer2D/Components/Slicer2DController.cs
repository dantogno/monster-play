using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

// Controller
public class Slicer2DController : MonoBehaviour {
	public enum SliceType {Linear, LinearCut, Complex, ComplexCut, ComplexTracked, Point, Polygon, Explode, ExplodeByPoint, Create};
	public enum SliceRotation {Random, Vertical, Horizontal}
	public enum CreateType {Slice, PolygonType}

	public bool addForce = true;
	public float addForceAmount = 5f;

	public bool endSliceIfPossible = false;
	public bool startSliceIfPossible = false;
	public bool startedSlice = false;

	[Tooltip("Slice type represents algorithm complexity")]
	public SliceType sliceType = SliceType.Complex;
	public Slice2DLayer sliceLayer = Slice2DLayer.Create();

	public Polygon2D slicePolygon = Polygon2D.Create (Polygon2D.PolygonType.Pentagon);

	[Tooltip("Minimum distance between points (SliceType: Complex")]
	private float minVertsDistance = 1f;

	// Polygon Destroyer type settings
	public Polygon2D.PolygonType polygonType = Polygon2D.PolygonType.Circle;
	public float polygonSize = 1;
	public bool polygonDestroy = false;
	
	public bool sliceJoints = false;

	// Polygon Creator
	public Material material;
	public CreateType createType = CreateType.Slice;

	// Complex Slicer
	public Slicer2D.SliceType complexSliceType = Slicer2D.SliceType.SliceHole;

	// Linear Cut Slicer
	public float linearCutSize = 0.5f;
	public float complexCutSize = 0.5f;

	// Slicer Visuals
	public bool drawSlicer = true;
	public float visualScale = 1f;
	public float lineWidth = 1.0f;
	public float zPosition = 0f;
	public Color slicerColor = Color.black;

	// Point Slicer
	public SliceRotation sliceRotation = SliceRotation.Random;

	// Events Input Handler
	public static List<Vector2D> complexSlicerPointsList = new List<Vector2D>();
	public static Pair2D linearPair = Pair2D.Zero();
	public static LinearCut linearCutLine = new LinearCut();
	public static ComplexCut complexCutLine = new ComplexCut();
	public static ComplexSlicerTracker complexTracker = new ComplexSlicerTracker();

	public static Slicer2DController instance;
	private bool mouseDown = false;

	public static Color[] slicerColors = {Color.black, Color.green, Color.yellow , Color.red, new Color(1f, 0.25f, 0.125f)};

	public delegate void Slice2DResultEvent(Slice2D slice);
	private event Slice2DResultEvent sliceResultEvent;
	public void AddResultEvent(Slice2DResultEvent e) { sliceResultEvent += e; }

	public void Awake()
	{
		instance = this;
	}

	public static Vector2D GetMousePosition()
	{
		return(new Vector2D (Camera.main.ScreenToWorldPoint (Input.mousePosition)));
	}

	public void SetSliceType(int type)
	{
		sliceType = (SliceType)type;
	}

	public void SetLayerType(int type)
	{
		if (type == 0) {
			sliceLayer.SetLayerType((Slice2DLayerType)0);
		} else {
			sliceLayer.SetLayerType((Slice2DLayerType)1);
			sliceLayer.DisableLayers ();
			sliceLayer.SetLayer (type - 1, true);
		}
	}

	public void SetSlicerColor(int colorInt)
	{
		slicerColor = slicerColors [colorInt];
	}

	public void OnRenderObject() {
		Vector2D pos = GetMousePosition ();

		if (drawSlicer == false) {
			return;
		}

		Max2D.SetBorder (true);
		Max2D.SetLineMode(Max2D.LineMode.Smooth);
		Max2D.SetLineWidth (lineWidth * .5f);
		Max2D.SetScale(visualScale);

		if (mouseDown) {
			Max2D.SetColor (slicerColor);

			switch (sliceType) {
				case SliceType.Complex:
					if (complexSlicerPointsList.Count > 0) {
						if (startSliceIfPossible == false || startedSlice == true) {
							Max2D.SetColor (Color.black);
							Max2D.DrawLineSquare (complexSlicerPointsList.Last(), 0.6f * visualScale, zPosition);
							Max2D.DrawLineSquare (complexSlicerPointsList.First (), 0.6f * visualScale, zPosition);
							Max2D.DrawLineSquare (complexSlicerPointsList.Last(), 0.4f * visualScale, zPosition);
							Max2D.DrawLineSquare (complexSlicerPointsList.First (), 0.4f * visualScale, zPosition);
							
							Max2D.SetColor (slicerColor);
							Max2D.DrawStrippedLine (complexSlicerPointsList, minVertsDistance, zPosition);
							Max2D.DrawLineSquare (complexSlicerPointsList.Last(), 0.5f * visualScale, zPosition);
							Max2D.DrawLineSquare (complexSlicerPointsList.First (), 0.5f * visualScale, zPosition);
						}
					}
					break;

				case SliceType.ComplexTracked:
					if (complexSlicerPointsList.Count > 0) {
						Max2D.DrawLineSquare (pos, 0.5f * visualScale, zPosition);

						foreach(ComplexSlicerTrackerObject tracker in complexTracker.trackerList) {
							if (tracker.slicer != null && tracker.tracking) {
								Max2D.DrawSlice(Vector2DList.ToWorldSpace(tracker.slicer.transform, tracker.pointsList), tracker.slicer.transform.position.z - 0.001f);
							}
						}
					}
					break;

				case SliceType.Create:
					if (createType == CreateType.Slice) {
						if (complexSlicerPointsList.Count > 0) {
							Max2D.SetColor (Color.black);
							Max2D.DrawLineSquare (complexSlicerPointsList.Last(), 0.4f * visualScale, zPosition);
							Max2D.DrawLineSquare (complexSlicerPointsList.First (), 0.4f * visualScale, zPosition);
							Max2D.DrawLineSquare (complexSlicerPointsList.Last(), 0.6f * visualScale, zPosition);
							Max2D.DrawLineSquare (complexSlicerPointsList.First (), 0.6f * visualScale, zPosition);
							
							Max2D.SetColor (slicerColor);
							Max2D.DrawLineSquare (complexSlicerPointsList.Last(), 0.5f * visualScale, zPosition);
							Max2D.DrawLineSquare (complexSlicerPointsList.First (), 0.5f * visualScale, zPosition);
							Max2D.DrawStrippedLine (complexSlicerPointsList, minVertsDistance, zPosition, true);
						}
					} else {
						Max2D.DrawStrippedLine (Polygon2D.Create(polygonType, polygonSize).pointsList, minVertsDistance, zPosition, true, pos);
					}
					break;
				
				case SliceType.Linear:
					Max2D.SetColor (Color.black);
					Max2D.DrawLineSquare (linearPair.A, 0.6f * visualScale, zPosition);
					Max2D.DrawLineSquare (linearPair.B, 0.6f * visualScale, zPosition);
					Max2D.DrawLineSquare (linearPair.A, 0.4f * visualScale, zPosition);
					Max2D.DrawLineSquare (linearPair.B, 0.4f * visualScale, zPosition);

					Max2D.SetColor (slicerColor);
					Max2D.DrawLineSquare (linearPair.A, 0.5f * visualScale, zPosition);
					Max2D.DrawLineSquare (linearPair.B, 0.5f * visualScale, zPosition);

					Max2D.DrawLine (linearPair.A, linearPair.B, zPosition);
					break;

				case SliceType.LinearCut:
					linearCutLine = LinearCut.Create(linearPair, linearCutSize);
					Max2D.DrawStrippedLine (linearCutLine.GetPointsList(), 0, zPosition, true);
					break;

				case SliceType.ComplexCut:
					complexCutLine = ComplexCut.Create(complexSlicerPointsList, complexCutSize);
					Max2D.DrawStrippedLine (complexCutLine.GetPointsList(), 0, zPosition, true);
					break;

				case SliceType.Point:
					break;

				case SliceType.Explode:
					break;

				case SliceType.Polygon:
					slicePolygon = Polygon2D.Create (polygonType, polygonSize);
					Max2D.DrawStrippedLine (slicePolygon.pointsList, minVertsDistance, zPosition, true, pos);
					break;
				
				default:
					break; 
			}
		}
		// Reset Graphics
		Max2D.SetScale(1f);
	}

	public void LateUpdate()
	{
		Vector2D pos = GetMousePosition ();

		switch (sliceType) {	
			case SliceType.Linear:
				UpdateLinear (pos);
				break;

			case SliceType.LinearCut:
				UpdateLinearCut (pos);
				break;

			case SliceType.ComplexCut:
				UpdateComplexCut (pos);
				break;

			case SliceType.Complex:
				UpdateComplex (pos);
				break;

			case SliceType.ComplexTracked:
				UpdateComplexTracked(pos);
				break;

			case SliceType.Point:
				UpdatePoint (pos);
				break;

				
			case SliceType.Explode:			
				UpdateExplode (pos);
				break;

			case SliceType.ExplodeByPoint:			
				UpdateExplodeByPoint (pos);
				break;

			case SliceType.Create:
				UpdateCreate (pos);
				break;

			case SliceType.Polygon:
				UpdatePolygon (pos);
				break;

			default:
				break; 
		}
	}
		
	private void UpdateLinear(Vector2D pos)
	{
		if (Input.GetMouseButtonDown (0)) {
			linearPair.A.Set (pos);
			mouseDown = true;
		}

		if (mouseDown && Input.GetMouseButton (0)) {
			linearPair.B.Set (pos);
		
			if (endSliceIfPossible) {
				if (LinearSlice (linearPair)) {
					mouseDown = false;
					linearPair.A.Set (pos);
				}
			}
		}

		if (mouseDown == true && Input.GetMouseButton (0) == false) {
			mouseDown = false;

			LinearSlice (linearPair);
		}
	}

	private void UpdateLinearCut(Vector2D pos)
	{
		if (Input.GetMouseButtonDown (0)) {
			linearPair.A.Set (pos);
		}

		if (Input.GetMouseButton (0)) {
			linearPair.B.Set (pos);
			mouseDown = true;
		}

		if (mouseDown == true && Input.GetMouseButton (0) == false) {
			mouseDown = false;
			Slicer2D.LinearCutSliceAll (linearCutLine, sliceLayer);
		}
	}

	private bool InSlicerComponents(Vector2D point) {
		foreach(Slicer2D slicer in Slicer2D.GetList()) {
			Polygon2D poly = slicer.GetPolygon().ToWorldSpace(slicer.transform);
			if (poly.PointInPoly(point)) {
				return(true);
			}
		}
		return(false);
	}

	private void UpdateComplexCut(Vector2D pos)
	{
	if (Input.GetMouseButtonDown (0)) {
			complexSlicerPointsList.Clear ();
			complexSlicerPointsList.Add (pos);
			mouseDown = true;
			startedSlice = false;
		}

		if (complexSlicerPointsList.Count < 1) {
			return;
		}
		
		if (Input.GetMouseButton (0)) {
			Vector2D posMove = new Vector2D (complexSlicerPointsList.Last ());
			bool added = false;
			while ((Vector2D.Distance (posMove, pos) > minVertsDistance * visualScale)) {
				float direction = Vector2D.Atan2 (pos, posMove);
				posMove.Push (direction, minVertsDistance * visualScale);

				if (startSliceIfPossible == true && startedSlice == false) {
					if (InSlicerComponents(new Vector2D (posMove))) {
						while (complexSlicerPointsList.Count > 2) {
							complexSlicerPointsList.RemoveAt(0);
						}

						startedSlice = true;
					}
				}

				complexSlicerPointsList.Add (new Vector2D (posMove));

				added = true;
			}

			if (endSliceIfPossible == true && added) {
				if (ComplexSlice (complexSlicerPointsList) == true) {
					mouseDown = false;
					complexSlicerPointsList.Clear ();
				}
			}
		}

		if (mouseDown == true && Input.GetMouseButton (0) == false) {
			mouseDown = false;
			startedSlice = false;
			Slicer2D.ComplexCutSliceAll (complexCutLine, sliceLayer);
			complexSlicerPointsList.Clear ();
		}
	}

	private void UpdateComplex(Vector2D pos)
	{
		if (Input.GetMouseButtonDown (0)) {
			complexSlicerPointsList.Clear ();
			complexSlicerPointsList.Add (pos);
			mouseDown = true;
			startedSlice = false;
			//if (InSlicerComponents(pos)) {
			//}
		}

		if (complexSlicerPointsList.Count < 1) {
			return;
		}
		
		if (Input.GetMouseButton (0)) {
			Vector2D posMove = new Vector2D (complexSlicerPointsList.Last ());
			bool added = false;
			while ((Vector2D.Distance (posMove, pos) > minVertsDistance * visualScale)) {
				float direction = Vector2D.Atan2 (pos, posMove);
				posMove.Push (direction, minVertsDistance * visualScale);

				if (startSliceIfPossible == true && startedSlice == false) {
					if (InSlicerComponents(new Vector2D (posMove))) {
						while (complexSlicerPointsList.Count > 2) {
							complexSlicerPointsList.RemoveAt(0);
						}

						startedSlice = true;
					}
				}

				complexSlicerPointsList.Add (new Vector2D (posMove));

				added = true;
			}

			if (endSliceIfPossible == true && added) {
				if (ComplexSlice (complexSlicerPointsList) == true) {
					mouseDown = false;
					complexSlicerPointsList.Clear ();
				}
			}
		}

		if (mouseDown == true && Input.GetMouseButton (0) == false) {
			mouseDown = false;
			startedSlice = false;
			Slicer2D.complexSliceType = complexSliceType;
			ComplexSlice (complexSlicerPointsList);
			complexSlicerPointsList.Clear ();
		}
	
	}

	private void UpdateComplexTracked(Vector2D pos)
	{
		if (Input.GetMouseButtonDown (0)) {
			complexSlicerPointsList.Clear ();
			complexTracker.trackerList.Clear ();
			complexSlicerPointsList.Add (pos);
		}
						
		if (Input.GetMouseButton (0) && complexSlicerPointsList.Count > 0) {
			Vector2D posMove = new Vector2D (complexSlicerPointsList.Last ());

			while ((Vector2D.Distance (posMove, pos) > minVertsDistance)) {
				float direction = Vector2D.Atan2 (pos, posMove);
				posMove.Push (direction, minVertsDistance);
				Slicer2D.complexSliceType = complexSliceType;
				complexSlicerPointsList.Add (new Vector2D (posMove));
				complexTracker.Update(posMove.vector, 0);
			}

			mouseDown = true;
			
			complexTracker.Update(posMove.vector, minVertsDistance);

		} else {
			mouseDown = false;
		}
	}

	private void UpdatePoint(Vector2D pos)
	{
		if (Input.GetMouseButtonDown (0)) {
			PointSlice(pos);
		}
	}

	private void UpdatePolygon(Vector2D pos)
	{
		mouseDown = true;

		if (Input.GetMouseButtonDown (0)) {
			PolygonSlice (pos);
		}
	}

	private void UpdateExplode(Vector2D pos)
	{
		if (Input.GetMouseButtonDown (0)) {
			ExplodeInPoint(pos);
		}
	}

	private void UpdateExplodeByPoint(Vector2D pos)
	{
		if (Input.GetMouseButtonDown (0)) {
			ExplodeByPoint(pos);
		}
	}

	private void UpdateCreate(Vector2D pos)
	{
		if (Input.GetMouseButtonDown (0)) {
			complexSlicerPointsList.Clear ();
			complexSlicerPointsList.Add (pos);
		}

		if (createType == CreateType.Slice) {
			if (Input.GetMouseButton (0)) {
				if (complexSlicerPointsList.Count == 0 || (Vector2D.Distance (pos, complexSlicerPointsList.Last ()) > minVertsDistance * visualScale)) {
					complexSlicerPointsList.Add (pos);
				}

				mouseDown = true;
			}

			if (mouseDown == true && Input.GetMouseButton (0) == false) {
				mouseDown = false;
				CreatorSlice (complexSlicerPointsList);
			}
		} else {
			mouseDown = true;
			if (Input.GetMouseButtonDown (0)) {
				PolygonCreator (pos);
			}
		}
	}

	private void LinearSliceJoints(Pair2D slice) {
		foreach(Joint2D joint in Joint2D.GetJointsConnected()) {
			Vector2 localPosA = joint.anchoredJoint2D.connectedAnchor;
			Vector2 worldPosA = joint.anchoredJoint2D.connectedBody.transform.TransformPoint(localPosA);
			Vector2 localPosB = joint.anchoredJoint2D.anchor;
			Vector2 worldPosB = joint.anchoredJoint2D.transform.TransformPoint(localPosB);

			switch (joint.jointType) {
				case Joint2D.Type.HingeJoint2D:
					worldPosA = joint.anchoredJoint2D.connectedBody.transform.position;
					break;
				default:
					break;
			}
			
			Pair2D jointLine = new Pair2D(worldPosA, worldPosB);

			if (Math2D.LineIntersectLine(slice, jointLine)) {
				Destroy(joint.anchoredJoint2D);
			}
		}
	}

	private void ComplexSliceJoints(List<Vector2D> slice) {
		foreach(Joint2D joint in Joint2D.GetJointsConnected()) {
			Vector2 localPosA = joint.anchoredJoint2D.connectedAnchor;
			Vector2 worldPosA = joint.anchoredJoint2D.connectedBody.transform.TransformPoint(localPosA);
			Vector2 localPosB = joint.anchoredJoint2D.anchor;
			Vector2 worldPosB = joint.anchoredJoint2D.transform.TransformPoint(localPosB);

			switch (joint.jointType) {
				case Joint2D.Type.HingeJoint2D:
					worldPosA = joint.anchoredJoint2D.connectedBody.transform.position;
					break;
				default:
					break;
			}

			Pair2D jointLine = new Pair2D(worldPosA, worldPosB);

			foreach(Pair2D pair in Pair2D.GetList(slice, false)) {
				if (Math2D.LineIntersectLine(pair, jointLine)) {
					Destroy(joint.anchoredJoint2D);
				}
			}
		}	
	}

	private bool LinearSlice(Pair2D slice)
	{
		if (sliceJoints) {
			LinearSliceJoints(slice);
		}
		
		List<Slice2D> results = Slicer2D.LinearSliceAll (slice, sliceLayer);

		bool result = false;

		foreach (Slice2D id in results)  {
			if (id.gameObjects.Count > 0) {
				result = true;
			}
		}

		if (addForce == true) {
			float sliceRotation = Vector2D.Atan2 (slice.B, slice.A);

			foreach (Slice2D id in results) {
				foreach (GameObject gameObject in id.gameObjects) {
					Rigidbody2D rigidBody2D = gameObject.GetComponent<Rigidbody2D> ();
					if (rigidBody2D) {
						foreach (Vector2D p in id.collisions) {
							Vector2 force = new Vector2 (Mathf.Cos (sliceRotation) * addForceAmount, Mathf.Sin (sliceRotation) * addForceAmount);
							Physics2DHelper.AddForceAtPosition(rigidBody2D, force, p.vector);
						}
					}
				}
				if (sliceResultEvent != null) {
					sliceResultEvent(id);
				}
			}
		}

		return(result);
	}

	private bool ComplexSlice(List <Vector2D> slice)
	{
		if (sliceJoints) {
			ComplexSliceJoints(slice);
		}

		List<Slice2D> results = Slicer2D.ComplexSliceAll (slice, sliceLayer);
		bool result = false;

		foreach (Slice2D id in results) {
			if (id.gameObjects.Count > 0) {
				result = true;
			}
		}

		if (addForce == true)
			foreach (Slice2D id in results) {
				foreach (GameObject gameObject in id.gameObjects) {
					Rigidbody2D rigidBody2D = gameObject.GetComponent<Rigidbody2D> ();
					if (rigidBody2D) {
						List<Pair2D> list = Pair2D.GetList (id.collisions);
						float forceVal = 1.0f / list.Count;
						foreach (Pair2D p in list) {
							float sliceRotation = -Vector2D.Atan2 (p.B, p.A);
							Vector2 force = new Vector2 (Mathf.Cos (sliceRotation) * addForceAmount, Mathf.Sin (sliceRotation) * addForceAmount);
							Physics2DHelper.AddForceAtPosition(rigidBody2D, forceVal * force, (p.A.vector + p.B.vector) / 2f);
						}
					}
				}
				if (sliceResultEvent != null) {
					sliceResultEvent(id);
				}
			}
		return(result);
	}

	private void PointSlice(Vector2D pos)
	{
		float rotation = 0;

		switch (sliceRotation) {	
			case SliceRotation.Random:
				rotation = UnityEngine.Random.Range (0, Mathf.PI * 2);
				break;

			case SliceRotation.Vertical:
				rotation = Mathf.PI / 2f;
				break;

			case SliceRotation.Horizontal:
				rotation = Mathf.PI;
				break;
		}

		List<Slice2D> results = Slicer2D.PointSliceAll (pos, rotation, sliceLayer);
		foreach (Slice2D id in results) {
			if (sliceResultEvent != null) {
				sliceResultEvent(id);
			}
		}
	}
		
	private void PolygonSlice(Vector2D pos)
	{
		Polygon2D slicePolygonDestroy = null;
		if (polygonDestroy == true) {
			slicePolygonDestroy = Polygon2D.Create (polygonType, polygonSize * 1.1f);
		}
		Slicer2D.PolygonSliceAll(pos, Polygon2D.Create (polygonType, polygonSize), slicePolygonDestroy, sliceLayer);
	}

	private void ExplodeByPoint(Vector2D pos)
	{
		List<Slice2D> results =	Slicer2D.ExplodeByPointAll (pos, sliceLayer);
		if (addForce == true) {
			foreach (Slice2D id in results) {
				foreach (GameObject gameObject in id.gameObjects) {
					Rigidbody2D rigidBody2D = gameObject.GetComponent<Rigidbody2D> ();
					if (rigidBody2D) {
						float sliceRotation = Vector2D.Atan2 (pos, new Vector2D (gameObject.transform.position));
						Rect rect = Polygon2D.CreateFromCollider (gameObject).GetBounds ();
						Physics2DHelper.AddForceAtPosition(rigidBody2D, new Vector2 (Mathf.Cos (sliceRotation) * addForceAmount, Mathf.Sin (sliceRotation) * addForceAmount), rect.center);
					}
				}
				if (sliceResultEvent != null) {
					sliceResultEvent(id);
				}
			}
		}
	}

	private void ExplodeInPoint(Vector2D pos)
	{
		List<Slice2D> results =	Slicer2D.ExplodeInPointAll (pos, sliceLayer);
		if (addForce == true) {
			foreach (Slice2D id in results) {
				foreach (GameObject gameObject in id.gameObjects) {
					Rigidbody2D rigidBody2D = gameObject.GetComponent<Rigidbody2D> ();
					if (rigidBody2D) {
						float sliceRotation = Vector2D.Atan2 (pos, new Vector2D (gameObject.transform.position));
						Rect rect = Polygon2D.CreateFromCollider (gameObject).GetBounds ();
						Physics2DHelper.AddForceAtPosition(rigidBody2D, new Vector2 (Mathf.Cos (sliceRotation) * addForceAmount, Mathf.Sin (sliceRotation) * addForceAmount), rect.center);
					}
				}
				if (sliceResultEvent != null) {
					sliceResultEvent(id);
				}
			}
		}
	}

	private void ExplodeAll()
	{
		List<Slice2D> results =	Slicer2D.ExplodeAll (sliceLayer);
		if (addForce == true) {
			foreach (Slice2D id in results) {
				foreach (GameObject gameObject in id.gameObjects) {
					Rigidbody2D rigidBody2D = gameObject.GetComponent<Rigidbody2D> ();
					if (rigidBody2D) {
						float sliceRotation = Vector2D.Atan2 (new Vector2D(0, 0), new Vector2D (gameObject.transform.position));
						Rect rect = Polygon2D.CreateFromCollider (gameObject).GetBounds ();
						Physics2DHelper.AddForceAtPosition(rigidBody2D, new Vector2 (Mathf.Cos (sliceRotation) * addForceAmount / 10f, Mathf.Sin (sliceRotation) * addForceAmount/ 10f), rect.center);
					}
				}
				if (sliceResultEvent != null) {
					sliceResultEvent(id);
				}
			}
		}
	}

	private void CreatorSlice(List <Vector2D> slice)
	{
		Polygon2D newPolygon = Slicer2D.API.CreatorSlice (slice);
		if (newPolygon != null) {
			CreatePolygon (newPolygon);
		}
	}

	private void PolygonCreator(Vector2D pos)
	{
		Polygon2D newPolygon = Polygon2D.Create (polygonType, polygonSize).ToOffset (pos);
		CreatePolygon (newPolygon);
	}

	private void CreatePolygon(Polygon2D newPolygon)
	{
		GameObject newGameObject = new GameObject ();
		newGameObject.transform.parent = transform;
		newGameObject.AddComponent<Rigidbody2D> ();
		newGameObject.AddComponent<ColliderLineRenderer2D> ().color = Color.black;

		Slicer2D smartSlicer = newGameObject.AddComponent<Slicer2D> ();
		smartSlicer.textureType = Slicer2D.TextureType.Mesh2D;
		smartSlicer.material = material;

		newPolygon.CreateCollider (newGameObject);
		newPolygon.CreateMesh (newGameObject, new Vector2 (1, 1), Vector2.zero, PolygonTriangulator2D.Triangulation.Advanced);
	}
}