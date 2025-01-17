﻿using UnityEngine;

/// <summary>
/// A zone that accelerates a ridgidbody that enters it  upwards
/// /// This is from Jasper Flicks tutorial series on movement
/// https://catlikecoding.com/unity/tutorials/movement/
/// </summary>
public class JumpPad : MonoBehaviour
{
    [SerializeField, Min(0f)]
    float acceleration = 10f, speed = 10f;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        if (body)
        {
            Accelerate(body);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        if (body)
        {
            Accelerate(body);
        }
    }

    private void Accelerate(Rigidbody body)
    {
        Vector3 velocity = transform.InverseTransformDirection(body.velocity);
        if (velocity.y >= speed)
        {
            return;
        }
        if (acceleration > 0f)
        {
            velocity.y = Mathf.MoveTowards(velocity.y, speed, acceleration * Time.deltaTime);
        }
        else
        {
            velocity.y = speed;
        }
        body.velocity = transform.TransformDirection(velocity);
        if (body.TryGetComponent(out PlayerMovement movement))
        {
            movement.PreventSnapToGround();
        }
    }
}
