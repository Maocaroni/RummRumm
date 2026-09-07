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
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }

    void RotateToTarget()
    {
        Vector3 direction = target.position - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void SnapToTarget()
    {
        if (target != null)
        {
            transform.position = target.TransformPoint(offset);
            Vector3 direction = target.position - transform.position;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}