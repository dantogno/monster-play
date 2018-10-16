using UnityEngine;

[ExecuteInEditMode]
public class Mesh2D : MonoBehaviour {
	public PolygonTriangulator2D.Triangulation triangulation = PolygonTriangulator2D.Triangulation.Advanced;

	// Optionable material
	public Material material;

	public string sortingLayerName; 
	public int sortingLayerID;
	public int sortingOrder;

	void Start () {
		if (GetComponents<Mesh2D>().Length > 1) {
			Slicer2D.Debug.LogError("Multiple 'Mesh2D' components cannot be attached to the same game object");
			return;
		}

		if (GetComponent<Slicer2D>() != null) {
			Slicer2D.Debug.LogError("'Mesh2D' and Slicer2D components cannot be attached to the same game object");
			return;
		}

		// Generate Mesh from collider
		Polygon2D polygon = Polygon2D.CreateFromCollider (gameObject);
		if (polygon != null) {
			polygon.CreateMesh(gameObject, Vector2.zero, Vector2.zero, triangulation);

			// Setting Mesh material
			if (material != null) {
				MeshRenderer meshRenderer = GetComponent<MeshRenderer> ();
				meshRenderer.material = material;
			
				meshRenderer.sortingLayerName = sortingLayerName;
				meshRenderer.sortingLayerID = sortingLayerID;
				meshRenderer.sortingOrder = sortingOrder;
			}
		}
	}
}
