using UnityEngine;

/// <summary>
/// Makes the turrets rotate correctly
/// </summary>
public class TurretRotation : MonoBehaviour
{
    Transform rotationFocus;

    void Start()
    {
        rotationFocus = Camera.main.transform;    
    }
    void LateUpdate()
    {
        transform.rotation = rotationFocus.rotation;
    }
}
