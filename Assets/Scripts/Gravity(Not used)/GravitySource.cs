using UnityEngine;

/// <summary>
/// Not really used in the project but i had to use this once 
/// i finished his tutorial
/// This is from Jasper Flicks tutorial series on movement
/// https://catlikecoding.com/unity/tutorials/movement/
/// </summary>
public class GravitySource : MonoBehaviour
{
    void OnEnable()
    {
        CustomGravity.Register(this);    
    }

    void OnDisable()
    {
        CustomGravity.Unregister(this);
    }

    public virtual Vector3 GetGravity(Vector3 position)
    {
        return Physics.gravity;
    }

}
