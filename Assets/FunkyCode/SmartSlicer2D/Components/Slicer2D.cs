using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class Slicer2D : MonoBehaviour {
	public enum SlicerType {Collider};
	public enum SliceType {Regular, SliceHole, FillSlicedHole};
	public enum TextureType {Sprite, Mesh2D, Mesh3D, SpriteAnimation, None};
	public enum CenterOfMass {Default, CenterOfOriginal};

	public static SliceType complexSliceType = SliceType.Regular;
	public static int explosionPieces = 15; // In Use?

	//[Tooltip("Type of texture to generate")]
	public TextureType textureType = TextureType.Sprite;

	public SlicingLayer slicingLayer = SlicingLayer.Layer1;

	// Polygon and Mesh Fields
	public PolygonTriangulator2D.Triangulation triangulation = PolygonTriangulator2D.Triangulation.Advanced;
	public Material material;
	private Polygon2D polygon = null;

	public SlicerType slicerType = SlicerType.Collider; // Not Finished
	
	public bool slicingLimit = false;
	public int sliceCounter = 0;
	public int maxSlices = 10;
	
	public CenterOfMass centerOfMass = CenterOfMass.Default;

	public bool recalculateMass = false;

	public VirtualSpriteRenderer spriteRenderer = null;
	static private Material spriteMaterial;

	// Joint Support
	private Rigidbody2D body2D;
	private List<Joint2D> joints = new List<Joint2D>();

	// Event Handling
	public delegate bool Slice2DEvent(Slice2D slice);
	public delegate void Slice2DResultEvent(Slice2D slice);

	private event Slice2DEvent sliceEvent;
	private event Slice2DResultEvent sliceResultEvent;

	public void AddEvent(Slice2DEvent e) { sliceEvent += e; }
	public void AddResultEvent(Slice2DResultEvent e) { sliceResultEvent += e; }

	static private List<Slicer2D> slicer2DList = new List<Slicer2D>();

	static public List<Slicer2D> GetList() {
		return(new List<Slicer2D>(slicer2DList));
	}

	static public List<Slicer2D> GetListLayer(Slice2DLayer layer) {
		List<Slicer2D> result = new List<Slicer2D> ();

		foreach (Slicer2D id in slicer2DList) {
			if (id.MatchLayers (layer)) {
				result.Add(id);
			}
		}
		
		return(result);
	}

	static public int GetListCount() {
		return(slicer2DList.Count);
	}
		
	public int GetLayerID() {
		return((int)slicingLayer);
	}

	public Polygon2D GetPolygon() {
		if (polygon == null) {
			polygon = Polygon2D.CreateFromCollider (gameObject);
		}
		return(polygon);
	}

	// Update loop enables ".enabled" component field	
	void Update() {}
	void OnEnable() { slicer2DList.Add (this);}
	void OnDisable() { slicer2DList.Remove (this);}

	void Start()
	{
		Initialize ();
		RecalculateJoints();
	}

	// Check Before Each Function - Then This Could Be Private
	public void Initialize() {
		if (spriteMaterial == null) {
			spriteMaterial = new Material (Shader.Find ("Sprites/Default"));
		}

		List<Polygon2D> result = Polygon2D.GetListFromCollider (gameObject);

		// Split collider if there are more polygons than 1
		if (result.Count > 1) {
			PerformResult(result, new Slice2D());
		}

		body2D = GetComponent<Rigidbody2D> ();

		MeshRenderer meshRenderer;

		switch (textureType) {
			case TextureType.Mesh2D:
				// Needs Mesh UV Options
				GetPolygon().CreateMesh (gameObject, new Vector2 (1, 1), Vector2.zero, triangulation);
				
				meshRenderer = GetComponent<MeshRenderer> ();
				meshRenderer.material = material;

				break;

			case TextureType.Mesh3D:
				GetPolygon().CreateMesh3D (gameObject, 1, new Vector2 (1, 1), Vector2.zero, triangulation);
				
				meshRenderer = GetComponent<MeshRenderer> ();
				meshRenderer.material = material;

				break;

			case TextureType.Sprite: case TextureType.SpriteAnimation:
				if (spriteRenderer == null) {
					spriteRenderer = new VirtualSpriteRenderer(GetComponent<SpriteRenderer>());
				}
				break;

			default:
				break;
			}
	}
		
	public Slice2D LinearSlice(Pair2D slice) {
		Slice2D slice2D = Slice2D.Create (slice);

		if (this.isActiveAndEnabled == false) {
			return(slice2D);
		}

		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Slice2D sliceResult = Slicer2D.API.LinearSlice (colliderPolygon, slice);	
			sliceResult.AddGameObjects (PerformResult (sliceResult.polygons, sliceResult));

			return(sliceResult);
		}
			
		return(slice2D);
	}

	public Slice2D LinearCutSlice(LinearCut slice) {
		Slice2D slice2D = Slice2D.Create (slice);

		if (this.isActiveAndEnabled == false) {
			return(slice2D);
		}

		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Polygon2D slicePoly = new Polygon2D(slice.GetPointsList(1.01f));
			
			if (Math2D.PolyInPoly(slicePoly, colliderPolygon) == true) {
				Destroy (gameObject);
				return(slice2D);

			} else {
				Slice2D sliceResult = Slicer2D.API.LinearCutSlice (colliderPolygon, slice);
				
				foreach(Polygon2D poly in new List<Polygon2D> (sliceResult.polygons)) {
					if (Math2D.PolyInPoly(slicePoly, poly)) {
						sliceResult.RemovePolygon(poly);
					}
				}

				sliceResult.AddGameObjects (PerformResult (sliceResult.polygons, sliceResult));

				return(sliceResult);
			}
		}
		
		return(Slice2D.Create (slice));
	}
			
	public Slice2D ComplexSlice(List<Vector2D> slice) {
		Slice2D slice2D = Slice2D.Create (slice);

		if (this.isActiveAndEnabled == false) {
			return(slice2D);
		}

		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Slice2D sliceResult = Slicer2D.API.ComplexSlice (colliderPolygon, slice);
			sliceResult.AddGameObjects (PerformResult (sliceResult.polygons, sliceResult));

			return(sliceResult);
		}
		
		return(slice2D);
	}

	public Slice2D ComplexCutSlice(ComplexCut slice) {
		Slice2D slice2D = Slice2D.Create (slice);

		if (this.isActiveAndEnabled == false) {
			return(slice2D);
		}

		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Polygon2D slicePoly = new Polygon2D(slice.GetPointsList(1.01f));
			
			if (Math2D.PolyInPoly(slicePoly, colliderPolygon) == true) {
				Destroy (gameObject);
				return(slice2D);

			} else {
				Slice2D sliceResult = Slicer2D.API.ComplexCutSlice (colliderPolygon, slice);
				
				foreach(Polygon2D poly in new List<Polygon2D> (sliceResult.polygons)) {
					if (Math2D.PolyInPoly(slicePoly, poly)) {
						sliceResult.RemovePolygon(poly);
					}
				}

				sliceResult.AddGameObjects (PerformResult (sliceResult.polygons, sliceResult));

				return(sliceResult);
			}
		}
		
		return(Slice2D.Create (slice));
	}

	public Slice2D PointSlice(Vector2D point, float rotation) {
		Slice2D slice2D = Slice2D.Create (point, rotation);

		if (this.isActiveAndEnabled == false) {
			return(slice2D);
		}

		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Slice2D sliceResult = Slicer2D.API.PointSlice (colliderPolygon, point, rotation);
			sliceResult.AddGameObjects (PerformResult (sliceResult.polygons, slice2D));
			
			return(sliceResult);
		}

		return(slice2D);
	}

	public Slice2D PolygonSlice(Polygon2D slice, Polygon2D sliceDestroy, Polygon2D slicePolygonDestroy) {
		Slice2D slice2D = Slice2D.Create (slice);

		if (this.isActiveAndEnabled == false) {
			return(slice2D);
		}
		
		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Slice2D sliceResult = Slicer2D.API.PolygonSlice (colliderPolygon, slice);

			if (sliceResult.polygons.Count > 0) { //  || ComplexSlicer.success == true !!!!!!!!!!!!!!!!!!!
				if (slicePolygonDestroy != null) {
					foreach (Polygon2D p in new List<Polygon2D>(sliceResult.polygons)) {
						if (sliceDestroy.PolyInPoly (p) == true) {
							sliceResult.RemovePolygon (p);
						}
					}
				}
				// Check If Slice Result Is Correct
				if (sliceResult.polygons.Count > 0) {
					sliceResult.AddGameObjects (PerformResult (sliceResult.polygons, slice2D));
				} else if (slicePolygonDestroy != null) {
					Destroy (gameObject);
				}
	
				return(sliceResult);
			}
		}

		return(slice2D);
	}

	public Slice2D ExplodeByPoint(Vector2D point) {
		Slice2D slice2D = Slice2D.Create (point);
		
		if (this.isActiveAndEnabled == false) {
			return(slice2D);
		}

		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Slice2D sliceResult = Slicer2D.API.ExplodeByPoint (colliderPolygon, point);
			sliceResult.AddGameObjects (PerformResult (sliceResult.polygons, slice2D));
			
			return(sliceResult);
		}

		return(slice2D);
	}

	public Slice2D ExplodeInPoint(Vector2D point) {
		Slice2D slice2D = Slice2D.Create (point);
		
		if (this.isActiveAndEnabled == false) {
			return(slice2D);
		}

		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Slice2D sliceResult = Slicer2D.API.ExplodeInPoint (colliderPolygon, point);
			sliceResult.AddGameObjects (PerformResult (sliceResult.polygons, slice2D));
			
			return(sliceResult);
		}

		return(slice2D);
	}


	public Slice2D Explode() {
		Slice2D slice2D = Slice2D.Create (Slice2DType.Explode);

		if (this.isActiveAndEnabled == false) {
			return(slice2D);
		}

		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Slice2D sliceResult = Slicer2D.API.Explode (colliderPolygon);
			sliceResult.AddGameObjects (PerformResult (sliceResult.polygons, slice2D));
			
			return(sliceResult);
		}

		return(slice2D);
	}

	// Does not return GameObjects
	// Necessary?
	public Slice2D PolygonSlice2(Polygon2D slice) {
		Polygon2D colliderPolygon = GetPolygonToSlice ();
		
		if (colliderPolygon != null) {
			return(Slicer2D.API.PolygonSlice (colliderPolygon, slice));
		}

		return(Slice2D.Create (slice));
	}

	public List<GameObject> PerformResult(List<Polygon2D> result, Slice2D slice)
	{
		List<GameObject> resultGameObjects = new List<GameObject> ();

		if (result.Count < 1) {
			return(resultGameObjects);
		}

		if (sliceEvent != null) {
			slice.polygons = result;

			if (sliceEvent (slice) == false) {
				return(resultGameObjects);
			}
		}

		float originArea = 1f;

		if (recalculateMass) {
			originArea = GetPolygon().GetArea();
		}

		Rigidbody2D originalRigidBody = GetComponent<Rigidbody2D>();

		int name_id = 1;
		foreach (Polygon2D id in result) {
			GameObject gObject = new GameObject();

			Component[] scriptList = gameObject.GetComponents<Component>();
			foreach (Component script in scriptList) {
				if (script.GetType().ToString() == "UnityEngine.BoxCollider2D" || script.GetType().ToString() == "UnityEngine.CircleCollider2D" || script.GetType().ToString() == "UnityEngine.CapsuleCollider2D") {
					continue;
				}

				switch (textureType) {
					case TextureType.SpriteAnimation:
						if (script.GetType().ToString() == "UnityEngine.SpriteRenderer" || script.GetType().ToString() == "UnityEngine.Animator") {
							continue;
						}
						break;
					
					case TextureType.Sprite:
						if (script.GetType().ToString() == "UnityEngine.SpriteRenderer") {
							continue;
						}
						break;

					default:
						break;
					}

				if (script.GetType().ToString() != "UnityEngine.Transform") {
					gObject.AddComponent(script.GetType());
					System.Reflection.FieldInfo[] fields = script.GetType().GetFields();

					foreach (System.Reflection.FieldInfo field in fields) {
						field.SetValue(gObject.GetComponent(script.GetType()), field.GetValue(script));
					}
				}
			}

			resultGameObjects.Add (gObject);
	
			foreach (Behaviour childCompnent in gObject.GetComponentsInChildren<Behaviour>()) {
				foreach (Behaviour child in GetComponentsInChildren<Behaviour>()) {
					if (child.GetType() == childCompnent.GetType()) {
						childCompnent.enabled = child.enabled;
						break;
					}
				}
			}

			Slicer2D slicer = gObject.GetComponent<Slicer2D> ();
			slicer.sliceCounter = sliceCounter + 1;
			slicer.maxSlices = maxSlices;

			gObject.name = name + " (" + name_id + ")";
			gObject.transform.parent = transform.parent;
			gObject.transform.position = transform.position;
			gObject.transform.rotation = transform.rotation;
			gObject.transform.localScale = transform.localScale;

			if (originalRigidBody) {
				Rigidbody2D newRigidBody = gObject.GetComponent<Rigidbody2D> ();

				newRigidBody.isKinematic = originalRigidBody.isKinematic;
				newRigidBody.velocity = originalRigidBody.velocity;
				newRigidBody.angularVelocity = originalRigidBody.angularVelocity;
				newRigidBody.angularDrag = originalRigidBody.angularDrag;
				newRigidBody.constraints = originalRigidBody.constraints;
				newRigidBody.gravityScale = originalRigidBody.gravityScale;
				newRigidBody.collisionDetectionMode = originalRigidBody.collisionDetectionMode;
				//newRigidBody.sleepMode = originalRigidBody.sleepMode;
				//newRigidBody.inertia = originalRigidBody.inertia;

				// Center of Mass : Auto / Center
				if (centerOfMass == CenterOfMass.CenterOfOriginal) {
					newRigidBody.centerOfMass = Vector2.zero;
				}
				
				if (recalculateMass) {
					float newArea = id.ToLocalSpace(transform).GetArea ();
					newRigidBody.mass = originalRigidBody.mass * (newArea / originArea);
				}
			}

			PhysicsMaterial2D material = gameObject.GetComponent<Collider2D> ().sharedMaterial;
			bool isTrigger = gameObject.GetComponent<Collider2D>().isTrigger;	

			PolygonCollider2D collider = id.ToLocalSpace (gObject.transform).CreateCollider (gObject);
			collider.sharedMaterial = material;
			collider.isTrigger = isTrigger;

			switch (textureType) {
				case TextureType.SpriteAnimation:
					gObject.GetComponent<Slicer2D>().textureType = TextureType.Sprite;
					Polygon2D.SpriteToMesh(gObject, spriteRenderer);
					break;

				case TextureType.Sprite:
					Polygon2D.SpriteToMesh(gObject, spriteRenderer);
					break;
					
				default:
					break;
				}

			name_id += 1;
		}
			
		Destroy (gameObject);

		if (resultGameObjects.Count > 0) {
			slice.gameObjects = resultGameObjects;

			SliceJointEvent (slice);

			if ((sliceResultEvent != null)) {
				sliceResultEvent (slice);
			}
		}

		return(resultGameObjects);
	}

	public bool MatchLayers(Slice2DLayer sliceLayer) {
		return((sliceLayer == null || sliceLayer.GetLayerType() == Slice2DLayerType.All) || sliceLayer.GetLayerState(GetLayerID ()));
	}
		
	static public List<Slice2D> LinearSliceAll(Pair2D slice, Slice2DLayer layer) {
		List<Slice2D> result = new List<Slice2D> ();

		foreach (Slicer2D id in GetListLayer(layer)) {
			Slice2D sliceResult = id.LinearSlice (slice);
		
			if (sliceResult.gameObjects.Count > 0) {
				result.Add (sliceResult);
			}
		}

		return(result);
	}
	
	static public List<Slice2D> LinearCutSliceAll(LinearCut linearCut, Slice2DLayer layer) {
		List<Slice2D> result = new List<Slice2D> ();

		foreach (Slicer2D id in GetListLayer(layer)) {
			Slice2D sliceResult = id.LinearCutSlice (linearCut);
			if (sliceResult.gameObjects.Count > 0) {
				result.Add (sliceResult);
			}
		}
				
		return(result);
	}

	static public List<Slice2D> ComplexSliceAll(List<Vector2D> slice, Slice2DLayer layer) {
		List<Slice2D> result = new List<Slice2D> ();

		foreach (Slicer2D id in GetListLayer(layer)) {
			Slice2D sliceResult = id.ComplexSlice (slice);
			if (sliceResult.gameObjects.Count > 0) {
				result.Add (sliceResult);
			}
		}
				
		return(result);
	}

	static public List<Slice2D> ComplexCutSliceAll(ComplexCut complexCut, Slice2DLayer layer) {
		List<Slice2D> result = new List<Slice2D> ();

		foreach (Slicer2D id in GetListLayer(layer)) {
			Slice2D sliceResult = id.ComplexCutSlice (complexCut);
			if (sliceResult.gameObjects.Count > 0) {
				result.Add (sliceResult);
			}
		}
				
		return(result);
	}

	static public List<Slice2D> PointSliceAll(Vector2D slice, float rotation, Slice2DLayer layer) {
		List<Slice2D> result = new List<Slice2D> ();

		foreach (Slicer2D id in GetListLayer(layer)) {
			Slice2D sliceResult = id.PointSlice (slice, rotation);
			if (sliceResult.gameObjects.Count > 0) {
				result.Add (sliceResult);
			}
		}

		return(result);
	}

	// Remove Position
	static public List<Slice2D> PolygonSliceAll(Vector2D position, Polygon2D slicePolygon, Polygon2D slicePolygonDestroy, Slice2DLayer layer) {
		Polygon2D sliceDestroy = null;
		Polygon2D slice = slicePolygon.ToOffset (position);

		if (slicePolygonDestroy != null) {
			sliceDestroy = slicePolygonDestroy.ToOffset (position);
		}

		List<Slice2D> result = new List<Slice2D> ();
		foreach (Slicer2D id in GetListLayer(layer)) {
			result.Add (id.PolygonSlice (slice, slicePolygon, sliceDestroy));
		}
		
		return(result);
	}
	
	static public List<Slice2D> ExplodeByPointAll(Vector2D point, Slice2DLayer layer) {
		List<Slice2D> result = new List<Slice2D> ();
		
		foreach (Slicer2D id in GetListLayer(layer)) {
			Slice2D sliceResult = id.ExplodeByPoint (point);
			if (sliceResult.gameObjects.Count > 0) {
				result.Add (sliceResult);
			}
		}

		return(result);
	}

	static public List<Slice2D> ExplodeInPointAll(Vector2D point, Slice2DLayer layer) {
		List<Slice2D> result = new List<Slice2D> ();
		
		foreach (Slicer2D id in GetListLayer(layer)) {
			Slice2D sliceResult = id.ExplodeInPoint (point);
			if (sliceResult.gameObjects.Count > 0) {
				result.Add (sliceResult);
			}
		}

		return(result);
	}

	static public List<Slice2D> ExplodeAll(Slice2DLayer layer) {
		List<Slice2D> result = new List<Slice2D> ();

		foreach (Slicer2D id in	GetListLayer(layer)) {
			Slice2D sliceResult = id.Explode ();
			if (sliceResult.gameObjects.Count > 0) {
				result.Add (sliceResult);
			}
		}

		return(result);
	}
		
	private Polygon2D GetPolygonToSlice() {
		if (sliceCounter >= maxSlices && slicingLimit) {
			return(null);
		}

	    return(GetPolygon().ToWorldSpace (gameObject.transform));
	}

	public void RecalculateJoints() {
		if (body2D) {
			joints = Joint2D.GetJointsConnected (body2D);
		}
	}

	void SliceJointEvent(Slice2D sliceResult) {
		RecalculateJoints() ;

		// Remove Slicer Component Duplicated From Sliced Components
		foreach (GameObject g in sliceResult.gameObjects) {
			List<Joint2D> joints = Joint2D.GetJoints(g);
			foreach(Joint2D joint in joints) {
				if (Polygon2D.CreateFromCollider (g).PointInPoly (new Vector2D (joint.anchoredJoint2D.anchor)) == false) {
					Destroy (joint.anchoredJoint2D);
				} else {
					if (joint.anchoredJoint2D != null && joint.anchoredJoint2D.connectedBody != null) {
						Slicer2D slicer2D = joint.anchoredJoint2D.connectedBody.gameObject.GetComponent<Slicer2D>();
						if (slicer2D != null) {
							slicer2D.RecalculateJoints();
						}
					}
				}
			}
		}
	
		if (body2D == null) {
			return;
		}

		// Reconnect Joints To Sliced Bodies
		foreach(Joint2D joint in joints) {
			if (joint.anchoredJoint2D == null) {
				continue;
			}
			
			foreach (GameObject g in sliceResult.gameObjects) {
				Polygon2D poly = Polygon2D.CreateFromCollider (g);

				switch (joint.jointType) {
					case Joint2D.Type.HingeJoint2D:
						if (poly.PointInPoly (new Vector2D (Vector2.zero))) {
							joint.anchoredJoint2D.connectedBody = g.GetComponent<Rigidbody2D> ();
						}
						break;

					default:
						if (poly.PointInPoly (new Vector2D (joint.anchoredJoint2D.connectedAnchor))) {
							joint.anchoredJoint2D.connectedBody = g.GetComponent<Rigidbody2D> ();
						}	
						break;
				}
			}
		}
	}

	public class API {
		static public Slice2D LinearSlice(Polygon2D polygon, Pair2D slice) {
			return(LinearSlicer.Slice (polygon, slice));
		}
		static public Slice2D LinearCutSlice(Polygon2D polygon, LinearCut linearCut) {
			return(ComplexSlicerExtended.LinearCutSlice (polygon, linearCut));
		}
		static public Slice2D ComplexSlice(Polygon2D polygon, List<Vector2D> slice) {
			return(ComplexSlicer.Slice (polygon, slice));
		}
		static public Slice2D ComplexCutSlice(Polygon2D polygon, ComplexCut complexCut) {
			return(ComplexSlicerExtended.ComplexCutSlice (polygon, complexCut));
		}
		static public Slice2D PointSlice(Polygon2D polygon, Vector2D point, float rotation) {
			return(LinearSlicerExtended.SliceFromPoint (polygon, point, rotation));
		}
		static public Slice2D PolygonSlice(Polygon2D polygon, Polygon2D polygonB) {
			return(ComplexSlicerExtended.Slice (polygon, polygonB)); 
		}
		static public Slice2D ExplodeByPoint(Polygon2D polygon, Vector2D point) {
			return(LinearSlicerExtended.ExplodeByPoint (polygon, point));
		}
		static public Slice2D ExplodeInPoint(Polygon2D polygon, Vector2D point) {
			return(LinearSlicerExtended.ExplodeInPoint (polygon, point));
		}
		static public Slice2D Explode(Polygon2D polygon) {
			return(LinearSlicerExtended.Explode (polygon));
		}
		static public Polygon2D CreatorSlice(List<Vector2D> slice) {
			return(ComplexSlicerExtended.CreateSlice (slice));
		}
	}

	public class Debug {
		public static bool enabled = true;

		public static void Log(string message) {
			if (enabled) {
				UnityEngine.Debug.Log("Slicer2D: " + message);
			}
		}

		public static void LogError(string message) {
			if (enabled) {
				UnityEngine.Debug.LogWarning("Slicer2D: " + message);
			}
		}
	}
}