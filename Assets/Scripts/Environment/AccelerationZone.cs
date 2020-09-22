using UnityEngine;

/// <summary>
/// Zone that accelerates a ridgidbody that walk into it
/// This is from Jasper Flicks tutorial series on movement
/// https://catlikecoding.com/unity/tutorials/movement/
/// </summary>
public class AccelerationZone : MonoBehaviour
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
    /// <summary>
    /// If a ridgid body stays in the trigger it accelerates it
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        if (body)
        {
            Accelerate(body);
        }
    }
    /// <summary>
    /// Increases the velocity of a ridged body in the x direction
    /// </summary>
    /// <param name="body"></param>
    private void Accelerate(Rigidbody body)
    {
        Vector3 velocity = transform.InverseTransformDirection(body.velocity);
        if (velocity.x >= speed)
        {
            return;
        }
        if (acceleration > 0f)
        {
            velocity.x = Mathf.MoveTowards(velocity.y, speed, acceleration * Time.deltaTime);
        }
        else
        {
            velocity.x = speed;
        }
        body.velocity = transform.TransformDirection(velocity);
        if (body.TryGetComponent(out PlayerMovement movement))
        {
            movement.PreventSnapToGround();
        }
    }
}
