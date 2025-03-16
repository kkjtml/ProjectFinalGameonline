using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerControllerScript : NetworkBehaviour
{
    public float speed;
    public float rotationSpeed;

    private Animator animator;
    private Rigidbody rb;
    private bool running;

    void Start()
    {
        speed = 20.0f;
        rotationSpeed = 5.0f;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        rb.interpolation = RigidbodyInterpolation.Interpolate; // ตั้งค่า Interpolate เพื่อให้การเคลื่อนที่นุ่มนวลขึ้น
        // rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // ตั้งค่า collision detection
        running = false;
    }

    void moveForward()
    {
        // Debug.Log("Speed: " + speed); // ตรวจสอบค่าความเร็วใน Console
        float verticalInput = Input.GetAxis("Vertical");
        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            // move forward only
            if (verticalInput > 0.01f)
            {
                Vector3 moveDirection = transform.forward * verticalInput * speed;
                moveDirection.y = rb.velocity.y;
                rb.velocity = moveDirection;

                if (!running)
                {
                    running = true;
                    animator.SetBool("Running", true);
                }
            }
        }
        else if (running)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0); 
            running = false;
            animator.SetBool("Running", false);
        }
    }

    void turn()
    {
        float rotation = Input.GetAxis("Horizontal");
        if (rotation != 0)
        {
            rotation *= rotationSpeed;
            Quaternion turn = Quaternion.Euler(0f, rotation, 0f);
            rb.MoveRotation(rb.rotation * turn);
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        moveForward();
        turn();
    }
}
