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
        speed = 15.0f;
        rotationSpeed = 5.0f;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        running = false;
    }

    void moveForward()
    {
        // Debug.Log("Speed: " + speed); // ตรวจสอบค่าความเร็วใน Console
        float verticalInput = Input.GetAxis("Vertical");
        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            if (verticalInput > 0.01f)
            {
                // float translation = verticalInput * speed;
                // translation *= Time.fixedDeltaTime;
                // rb.MovePosition(rb.position + this.transform.forward * translation);
                Vector3 moveDir = transform.forward * speed;
                rb.velocity = new Vector3(moveDir.x, rb.velocity.y, moveDir.z); // ให้เคลื่อนที่แบบลื่นไหล

                if (!running)
                {
                    running = true;
                    animator.SetBool("Running", true);
                }
            }
        }
        // else if (running)
        // {
        //     running = false;
        //     animator.SetBool("Running", false);
        // }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0); // หยุดเคลื่อนที่เมื่อไม่มีการกดปุ่ม
            if (running)
            {
                running = false;
                animator.SetBool("Running", false);
            }
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
