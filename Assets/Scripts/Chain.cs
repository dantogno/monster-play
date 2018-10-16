using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chain : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 1, acceleration = 0.5f, friction = 0.5f, maxSpeed = 3, accelerationForce = 200, maxSpeedForce = 10;

    [SerializeField]
    private float edgeOfScreenBuffer = 5;

    [SerializeField]
    private bool shoudlUseAbsoluteMouseFollow = false;

    private bool isLocked;
    private new Rigidbody2D rigidbody2D;
    private Vector2 previousCursorPosition, velocity = Vector2.zero;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        UpdateIsLocked();
    }
    private void FixedUpdate()
    {
        if (!isLocked)
        {
            //FollowMouse();
            if (shoudlUseAbsoluteMouseFollow)
                MoveWithMouseAxesAbsolute();
            else
                MoveWithMouseAxesAcceleration();
                //MoveWithMouseAddForce();
        }
    }


    private void UpdateIsLocked()
    {
        if (Input.GetButtonDown("Jump"))
        {
            isLocked = !isLocked;
        }

        Cursor.visible = isLocked;
        Cursor.lockState = isLocked ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void FollowMouse()
    {
        // TODO: use delta position to move the object rather than cursor position.
        //float adjustedX = Camera.main.WorldToScreenPoint(rigidbody2D.position).x + edgeOfScreenBuffer;
        //float adjustedY = Camera.main.WorldToScreenPoint(rigidbody2D.position).y + edgeOfScreenBuffer;

        float clampedMouseX = Mathf.Clamp(Input.mousePosition.x, edgeOfScreenBuffer, Screen.width - edgeOfScreenBuffer);
        float clampedMouseY = Mathf.Clamp(Input.mousePosition.y, edgeOfScreenBuffer, Screen.height - edgeOfScreenBuffer);

        Vector3 clampedMousePositionInWorld = Camera.main.ScreenToWorldPoint(new Vector2(clampedMouseX, clampedMouseY));

        float speedFactor =
            Vector2.Distance(rigidbody2D.position, clampedMousePositionInWorld) * moveSpeed * Time.deltaTime;

        Vector2 newPosition =
            Vector2.MoveTowards(rigidbody2D.position, clampedMousePositionInWorld, speedFactor);

        Debug.Log($"Screen.width: {Screen.width}, ClampedMouseX = {clampedMouseX}");
        rigidbody2D.MovePosition(newPosition);        

    }

    private void MoveWithMouseDelta()
    {
        Vector2 deltaMousePosition = (Vector2)Input.mousePosition - previousCursorPosition;
        Vector2 targetPosition = rigidbody2D.position + deltaMousePosition;
        targetPosition = Vector2.MoveTowards(rigidbody2D.position, targetPosition, moveSpeed * Time.deltaTime);
        rigidbody2D.MovePosition(targetPosition);
        Debug.Log($"Force: {deltaMousePosition * moveSpeed * Time.deltaTime}");
        previousCursorPosition = Input.mousePosition;
    }

    private void MoveWithMouseAxesAbsolute()
    {
        Vector2 mouseMovementInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        rigidbody2D.velocity = mouseMovementInput * moveSpeed * Time.deltaTime;
    }

    private void MoveWithMouseAxesAcceleration()
    {
        Vector2 mouseMovementInput = Vector2.zero;
        mouseMovementInput.x = Input.GetAxis("Mouse X");
        mouseMovementInput.y = Input.GetAxis("Mouse Y");

        Vector2 targetVelocity = velocity += mouseMovementInput *  acceleration * Time.deltaTime;

        //if (mouseMovementInput.magnitude == 0)
            // targetVelocity = Vector2.MoveTowards(targetVelocity, Vector2.zero, friction * Time.deltaTime);
            targetVelocity = Vector2.Lerp(targetVelocity, Vector2.zero, friction * Time.deltaTime);

        velocity = Vector2.ClampMagnitude(targetVelocity, maxSpeed* Time.deltaTime);
        rigidbody2D.MovePosition(rigidbody2D.position + velocity);             
    }

    private void MoveWithMouseAddForce()
    {
        Vector2 mouseMovementInput = Vector2.zero;
        mouseMovementInput.x = Input.GetAxis("Mouse X");
        mouseMovementInput.y = Input.GetAxis("Mouse Y");

        rigidbody2D.AddForce(mouseMovementInput * accelerationForce * Time.deltaTime, ForceMode2D.Impulse);

       // rigidbody2D.velocity = Vector2.ClampMagnitude(rigidbody2D.velocity, maxSpeedForce);
    }
}
