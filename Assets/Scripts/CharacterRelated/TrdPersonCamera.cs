using UnityEngine;

/// <summary>
/// Followed a tutorial and wanted to use it in this assignment
/// i learned a lot but some things i still dont understand (math)
/// This makes the camera follow the player at any angle and will
/// smooth itself out if the player is flipped upside down
/// This is from Jasper Flicks tutorial series on movement
/// https://catlikecoding.com/unity/tutorials/movement/
/// </summary>
[RequireComponent(typeof(Camera))]
public class TrdPersonCamera : MonoBehaviour
{
    [SerializeField] Transform focus = default;

    [SerializeField, Range(1f, 20f)]
    float distanceFromFocus = 5f;

    [SerializeField, Min(0f)]
    float focusRadius = 1f;

    [SerializeField, Range(0f, 1f)]
    float focusCentering = 0.5f;

    [SerializeField, Range(1f, 360f)]
    float rotationSpeed = 90f;

    [SerializeField, Range(-89f, 89f)]
    float minVerticalAngle = -30f, maxVerticalAngle = 60f;

    [SerializeField, Min(0f)]
    float alignDelay = 5f;

    [SerializeField, Range(0f, 90f)]
    float alignSmoothRange = 45f;

    [SerializeField, Min(0f)]
    float upAlignmentSpeed = 360f;

    [SerializeField]
    LayerMask obstructionMask = -1;


    Camera cameraReference;
    Vector3 focusPoint, previousFocusPoint;
    Vector2 orbitAngles = new Vector2(45f, 0f);
    float lastManualRotationTime;
    Quaternion gravityAlignment = Quaternion.identity;
    Quaternion orbitRotation;

    void Awake()
    {
        cameraReference = GetComponent<Camera>();
        focusPoint = focus.position;
        transform.localRotation = orbitRotation = Quaternion.Euler(orbitAngles);
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
    }

    void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle)
        {
            maxVerticalAngle = minVerticalAngle;
        }
    }

    void LateUpdate()
    {
        UpdateGravityAlignment();
        UpdateFocuspoint();
        if (ManualRotation() || AutomaticRotation())
        {
            ConstrainAngles();
            orbitRotation = Quaternion.Euler(orbitAngles);
        }
        
        Vector3 lookDirection = orbitRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distanceFromFocus;

        lookPosition = CamerObstruction(orbitRotation, lookDirection, lookPosition);

        transform.SetPositionAndRotation(lookPosition, orbitRotation);
    }

    /// <summary>
    /// Used for keeping the camera unobstructed by returning a vector position
    /// </summary>
    /// <param name="lookRotation"></param>
    /// <param name="lookDirection"></param>
    /// <param name="lookPosition"></param>
    /// <returns></returns>
    private Vector3 CamerObstruction(Quaternion lookRotation, Vector3 lookDirection, Vector3 lookPosition)
    {
        Vector3 rectOffset = lookDirection * cameraReference.nearClipPlane;
        Vector3 rectPosition = lookPosition + rectOffset;
        Vector3 castFrom = focus.position;
        Vector3 castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;

        if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out RaycastHit hit, lookRotation, castDistance, obstructionMask))
        {
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }

        return lookPosition;
    }

    /// <summary>
    /// If the player encounters gravity the camera will align so that it is still following
    /// </summary>
    void UpdateGravityAlignment()
    {
        Vector3 fromUp = gravityAlignment * Vector3.up;
        Vector3 toUp = CustomGravity.GetUpAxis(focusPoint);
        float dot = Mathf.Clamp(Vector3.Dot(fromUp, toUp), -1f, 1f);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        float maxAngle = upAlignmentSpeed * Time.deltaTime;

        Quaternion newAlignment = Quaternion.FromToRotation(fromUp, toUp) * gravityAlignment;
        if (angle <= maxAngle)
        {
            gravityAlignment = newAlignment;
        }
        else
        {
            gravityAlignment = Quaternion.SlerpUnclamped(gravityAlignment, newAlignment, maxAngle / angle);
        }
        gravityAlignment = newAlignment;
    }

    /// <summary>
    /// Keeping the camera at the same focus point and centers if the 
    /// player is standing still
    /// </summary>
    void UpdateFocuspoint()
    {
        previousFocusPoint = focusPoint;
        Vector3 targetPoint = focus.position;
        if (focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, focusPoint);
            float t = 1f;
            if (distance > 0.01f && focusCentering > 0f)
            {
                t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
            }
            if (distance > focusRadius)
            {
                t = Mathf.Min(t, focusRadius / distance);
            }
            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }
        else
        {
            focusPoint = targetPoint;
        }
    }

    /// <summary>
    /// Contrains the angles that the camera can rotate
    /// </summary>
    void ConstrainAngles()
    {
        orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);
        if (orbitAngles.y < 0f)
        {
            orbitAngles.y += 360f;
        }
        else if (orbitAngles.y >= 360f)
        {
            orbitAngles.y -= 360f;
        }
    }

    /// <summary>
    /// Checks in the player has moves the mouse
    /// </summary>
    /// <returns></returns>
    bool ManualRotation()
    {
        Vector2 input = new Vector2(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
        const float e = float.Epsilon;
        if (input.x < -e || input.x > e || input.y < -e || input.y > e)
        {
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
            lastManualRotationTime = Time.unscaledTime;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks in the player has moves the mouse
    /// and if he hasn't and is still moving it will autocorrect
    /// so that it follows the player
    /// </summary>
    /// <returns></returns>
    bool AutomaticRotation()
    {
        if (Time.unscaledTime - lastManualRotationTime < alignDelay)
        {
            return false;
        }
        Vector3 alignedDelta = Quaternion.Inverse(gravityAlignment) * (focusPoint - previousFocusPoint);
        Vector2 movement = new Vector2(alignedDelta.x, alignedDelta.z);
        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.00001f)
        {
            return false;
        }

        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
        float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
        if (deltaAbs < alignSmoothRange)
        {
            rotationChange *= deltaAbs / alignSmoothRange;
        }
        else if (180f - deltaAbs < alignSmoothRange)
        {
            rotationChange *= (180f - deltaAbs) / alignSmoothRange;
        }
        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
        return true;
    }

    static float GetAngle(Vector2 direction)
    {
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle;
    }

    Vector3 CameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;
            halfExtends.y = cameraReference.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * cameraReference.fieldOfView);
            halfExtends.x = halfExtends.y * cameraReference.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }
}
