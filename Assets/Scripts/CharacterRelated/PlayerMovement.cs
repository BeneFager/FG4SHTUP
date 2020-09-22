using UnityEngine;

/// <summary>
/// Followed a tutorial and wanted to use it in this assignment
/// i learned a lot but some things i still dont understand(math)
/// This is a charactercontroller that makes it possible for the player to 
/// climb, jump, airjump, 
/// This is from Jasper Flicks tutorial series on movement
/// https://catlikecoding.com/unity/tutorials/movement/
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    Material normalMaterial = default, climbingMaterial = default;

    [SerializeField]
    Transform playerInputSpace = default; //Camera so that the movement is consitent with where you look

    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f, maxClimbSpeed = 2f;

    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f, maxClimbAcceleration = 20f;

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 3f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 1;

    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f, maxStairsAngle = 50f;

    [SerializeField, Range(90f, 180f)]
    float maxClimpAngle = 140f;

    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;

    [SerializeField, Min(0f)]
    float probeDistance = 1f;

    [SerializeField]
    LayerMask probeMask = -1, stairsMask = -1, climbMask = -1;

    Rigidbody body, connectedBody, previousConnectedBody; //connectedbodies are for moving terrain
    Vector3 velocity, connectionVelocity; //connectedVelocity is for moving terrain
    Vector3 contactNormal, steepNormal, climbNormal, lastClimbNormal;
    Vector3 upAxis, rightAxis, forwardAxis;
    Vector3 connectionWorldPosition, connectionLocalPosition;//for moving terrain
    Vector2 playerInput;
    bool desiredJump, desiresClimbing;
    bool OnGround => groundContactCount > 0;
    bool OnSteep => steepContactCount > 0;
    bool Climbing => climbContactCount > 0 && stepSinceLastJump > 2;
    int jumpPhase;
    int groundContactCount, steepContactCount, climbContactCount;
    int stepSinceLastGrounded, stepSinceLastJump;
    float minGroundDotProduct, minStairsDotProduct, minClimbDotProduct; //dot product vector stuff used for slopes
    MeshRenderer meshRenderer;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        OnValidate();
        if (playerInputSpace == null)
        {
            playerInputSpace = Camera.main.transform;
        }
    }

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        minClimbDotProduct = Mathf.Cos(maxClimpAngle * Mathf.Deg2Rad);
    }

    void Update()
    {
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        if (playerInputSpace)
        {
            rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
        }
        else
        {
            rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
        }
        desiredJump |= Input.GetButtonDown("Jump");
        desiresClimbing = Input.GetButton("Climb");

        meshRenderer.material = Climbing ? climbingMaterial : normalMaterial;
    }

    void FixedUpdate()
    {
        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);
        velocity = body.velocity;
        UpdateState();
        AdjustVelocity();

        if (desiredJump)
        {
            desiredJump = false;
            Jump(gravity);
        }
        if (Climbing)
        {
            velocity -= contactNormal * (maxClimbAcceleration * 0.9f * Time.deltaTime); //0.9f so that climbing around corners is possible
        }
        else if (OnGround && velocity.sqrMagnitude < 0.01f)//for standing still on slopes
        {
            velocity += contactNormal * (Vector3.Dot(gravity, contactNormal) * Time.deltaTime);
        }
        else if (desiresClimbing && OnGround)// slows you down if youre not climing
        {
            velocity += (gravity - contactNormal * (maxClimbSpeed * 0.9f)) * Time.deltaTime;
        }
        else
        {
            velocity += gravity * Time.deltaTime;
        }
        body.velocity = velocity;
        ClearState();
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    /// <summary>
    /// Used for evaluating collision and checks if the play is on a
    /// slope of atempting to climb
    /// </summary>
    /// <param name="collision"></param>
    void EvaluateCollision(Collision collision)
    {
        int layer = collision.gameObject.layer;
        float minDot = GetMinDot(layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);
            if (upDot >= minDot)
            {
                groundContactCount += 1;
                contactNormal += normal;
                connectedBody = collision.rigidbody;
            }
            else
            {
                if (upDot > -0.01f)
                {
                    steepContactCount += 1;
                    steepNormal += normal;
                    if (groundContactCount == 0)
                    {
                        connectedBody = collision.rigidbody;
                    }
                }
                if (desiresClimbing && upDot >= minClimbDotProduct && (climbMask & (1 << layer)) != 0 )
                {
                    climbContactCount += 1;
                    climbNormal += normal;
                    lastClimbNormal = normal;
                    connectedBody = collision.rigidbody;
                }
            }
        }
    }

    /// <summary>
    /// Updates the states that the player can be in
    /// </summary>
    void UpdateState()
    {
        stepSinceLastGrounded += 1;
        stepSinceLastJump += 1;
        body.velocity = velocity;
        if (CheckClimbing() || OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepSinceLastGrounded = 0;
            if (stepSinceLastJump > 0)
            {
                jumpPhase = 0;
            }
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = upAxis;
        }
        if (connectedBody) //for moving terrain
        {
            if (connectedBody.isKinematic || connectedBody.mass >= body.mass)
            {
                UpdateConnectionState();
            }
        }
    }

    /// <summary>
    /// Clears the states the player is in every frame
    /// </summary>
    void ClearState()
    {
        groundContactCount = steepContactCount = climbContactCount = 0;
        contactNormal = steepNormal = climbNormal = connectionVelocity = Vector3.zero;
        previousConnectedBody = connectedBody;
        connectedBody = null;
    }

    /// <summary>
    /// This is used for moving terrain with a ridgidbody
    /// Not used in the project
    /// </summary>
    void UpdateConnectionState()
    {
        if (connectedBody == previousConnectedBody)
        {
            Vector3 connectionMovement = connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
            connectionVelocity = connectionMovement / Time.deltaTime;
        }
        connectionWorldPosition = body.position;
        connectionLocalPosition = connectedBody.transform.InverseTransformDirection(connectionWorldPosition);
    }

    /// <summary>
    /// Method for adjusting velocity of player
    /// Starts by giving us vectors aligned with the ground
    /// Projects the velocity on both axes and relative to x & z speed
    /// Calculates the speed relative to the ground
    /// All this is done to stop the sphere from losing contact with the ground
    /// when reversing on a slope
    /// </summary>
    void AdjustVelocity()
    {
        float acceleration, speed;
        Vector3 xAxis, zAxis;

        if (Climbing)
        {
            acceleration = maxClimbAcceleration;
            speed = maxClimbSpeed;
            xAxis = Vector3.Cross(contactNormal, upAxis);
            zAxis = upAxis;
        }
        else
        {
            acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            speed = OnGround && desiresClimbing ? maxClimbSpeed : maxSpeed;
            xAxis = rightAxis;
            zAxis = forwardAxis;
        }
        xAxis = ProjectDirectionOnPlane(xAxis, contactNormal);
        zAxis = ProjectDirectionOnPlane(zAxis, contactNormal);


        Vector3 relativeVelocity = velocity - connectionVelocity;
        float currentX = Vector3.Dot(relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeVelocity, zAxis);

        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, playerInput.x * speed, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, playerInput.y * speed, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    /// <summary>
    /// Method for jumping.
    /// JumpSpeed variable is to ensure we do not exceed max speed but
    /// if to not slow us down when we jump it is evaluated in mathf.max
    /// </summary>
    void Jump(Vector3 gravity)
    {
        Vector3 jumpDirection;

        if (OnGround)
        {
            jumpDirection = contactNormal;

        }
        else if (OnSteep)
        {
            jumpDirection = steepNormal;
            jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
        {
            if (jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        }
        else
        {
            return;
        }

        stepSinceLastJump = 0;
        jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight); //2 because of math
        jumpDirection = (jumpDirection + upAxis).normalized; //upward jump bias
        float alignedSpeed = Vector3.Dot(velocity, contactNormal);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        velocity += jumpDirection * jumpSpeed;
    }

    /// <summary>
    /// used for projecting on plane relative to the gameobject
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="normal"></param>
    /// <returns></returns>
    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    /// <summary>
    /// Snaps the player to the ground 
    /// </summary>
    /// <returns></returns>
    bool SnapToGround()
    {
        if (stepSinceLastGrounded > 1 || stepSinceLastJump <= 2)
        {
            return false;
        }
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }
        if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit, probeDistance, probeMask))
        {
            return false;
        }
        float upDot = Vector3.Dot(upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        groundContactCount = 1;
        contactNormal = hit.normal;
        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }
        connectedBody = hit.rigidbody;
        return true;

    }

    /// <summary>
    /// Checks if the player is on a steep surface
    /// </summary>
    /// <returns></returns>
    bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            float upDot = Vector3.Dot(upAxis, steepNormal);
            if (upDot >= minGroundDotProduct)
            {
                groundContactCount += 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the player is climbing
    /// </summary>
    /// <returns></returns>
    bool CheckClimbing()
    {
        if (Climbing)
        {
            if (climbContactCount > 1)
            {
                climbNormal.Normalize();
                float upDot = Vector3.Dot(upAxis, climbNormal);
                if (upDot >= minGroundDotProduct)
                {
                    climbNormal = lastClimbNormal;
                }
            }
            groundContactCount = 1;
            contactNormal = climbNormal;
            return true;
        }
        return false;
    }

    float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ? minGroundDotProduct : minStairsDotProduct;
    }

    /// <summary>
    /// Used in jumppads
    /// </summary>
    public void PreventSnapToGround()
    {
        stepSinceLastJump = -1;
    }

}
