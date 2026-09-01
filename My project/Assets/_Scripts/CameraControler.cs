using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControler : MonoBehaviour
{
    public Transform target;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Vector3 offset;

    void LateUpdate()
    {
        if (target != null)
        {
            FollowTarget();
            RotateToTarget();
        }
    }

    void FollowTarget()
    {
        Vector3 targetPos = target.TransformPoint(offset);
        // Usamos Time.deltaTime (o Time.fixedDeltaTime si lo pasas a FixedUpdate)
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }

    void RotateToTarget()
    {
        // Si quieres que mire al objetivo:
        Vector3 direction = target.position - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // *Alternativa si quieres que rote junto con el personaje en vez de mirarlo fijamente:*
        // transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, rotationSpeed * Time.deltaTime);
    }
}