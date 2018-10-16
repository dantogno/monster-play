using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle2D : MonoBehaviour {
	float speed = 1f;
	float rotation = 0;
	
	void Update () {
		speed = speed * 0.9f;

		Vector3 pos = transform.position;
		Vector2 vec = Vector2D.RotToVec(rotation * Mathf.Deg2Rad).vector;
		pos += new Vector3(vec.x, vec.y, 0) * speed;
		transform.position = pos;

		Vector3 scale = transform.localScale;
		scale.x = scale.x * 0.9f;
		scale.y = scale.y * 0.9f;

		if (scale.y < 0.05) {
			Destroy(gameObject);
		}
		
		transform.localScale = scale;
	}

	static public void Create(float rotation, Vector3 position) {
		GameObject particle = new GameObject();
		particle.name = "Particle2D";

		Particle2D p = particle.AddComponent<Particle2D>();
		p.speed = 0.1f;
		p.rotation = rotation;

		particle.transform.localScale = new Vector3(Random.Range(5, 15), Random.Range(5, 15f), 1);
		particle.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
		particle.transform.position = position;

		SpriteRenderer spriteRenderer = particle.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Flare");
		spriteRenderer.material = new Material (Shader.Find ("Particles/Additive"));
	}
}
