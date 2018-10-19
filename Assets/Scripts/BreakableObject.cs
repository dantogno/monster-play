using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [SerializeField]
    private FixedJoint2D fixedJoint;

    [SerializeField]
    private float requiredForceForBreak = 14, playerBreakForce = 30;

    [SerializeField]
    private float addedExplosionForceOnBreak = 2;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var requiredForce = collision.gameObject.CompareTag("Player") ? playerBreakForce : requiredForceForBreak;
        //Debug.Log($"Collision relative velocity magnitude: {collision.relativeVelocity.magnitude}");
        if (collision.relativeVelocity.magnitude > requiredForce)
        {
            Explode(collision.GetContact(0).point);
        }
    }

    private void Start()
    {
        //StartCoroutine(DelayedExplosionCoroutine());
    }
    private void Explode(Vector2 position)
    {


        if (fixedJoint != null)
            Destroy(fixedJoint);

        transform.SetParent(null);
        List<Slice2D> results = Slicer2D.ExplodeAll(BreakableManager.Instance.Slice2DLayer);

        if (addedExplosionForceOnBreak > 0)
        {
            foreach (Slice2D id in results)
            {                
                foreach (GameObject gameObject in id.gameObjects)
                {
                    var joint = gameObject.GetComponent<FixedJoint2D>();
                    if (joint != null)
                        Destroy(joint);
                    Rigidbody2D rigidBody2D = gameObject.GetComponent<Rigidbody2D>();
                    if (rigidBody2D)
                    {
                        float sliceRotation = Vector2D.Atan2(new Vector2D(position), 
                            new Vector2D(gameObject.transform.position));
                        Rect rect = Polygon2D.CreateFromCollider(gameObject).GetBounds();
                        Physics2DHelper.AddForceAtPosition(rigidBody2D, new Vector2(Mathf.Cos(sliceRotation) * 
                            addedExplosionForceOnBreak, Mathf.Sin(sliceRotation) * addedExplosionForceOnBreak), rect.center);
                    }
                }
            }
        }
    }

    private IEnumerator DelayedExplosionCoroutine()
    {
        yield return new WaitForSeconds(1);
        Explode(transform.position);
    }
}
