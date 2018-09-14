using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chain : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 1f;

    private new Rigidbody2D rigidbody2D;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float speedFactor = 
            Vector2.Distance(rigidbody2D.position, worldPoint) * moveSpeed * Time.deltaTime;

        Vector2 newPosition = 
            Vector2.MoveTowards(rigidbody2D.position, worldPoint, speedFactor);
        rigidbody2D.MovePosition(newPosition);

    }

}
