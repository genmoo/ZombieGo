using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public Transform Target;
  
    void LateUpdate()
    {
        if (Target == null)
        {
            return;
        }

        Vector3 newPosition = Target.position;
        newPosition.z -= 10f;
        transform.position = newPosition;
    }
}
