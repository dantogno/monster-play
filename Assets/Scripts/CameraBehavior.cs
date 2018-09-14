using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private float boundingBoxWidth = 10f;

    [SerializeField]
    private float speed = 2f;

    private void FixedUpdate()
    {
        if ( Mathf.Abs(target.position.x - transform.position.x ) > boundingBoxWidth)
        {
            transform.position = 
                new Vector3(Mathf.MoveTowards(
                    transform.position.x, target.position.x, speed * Time.deltaTime),
                    transform.position.y, transform.position.z);
        }
    }
}
