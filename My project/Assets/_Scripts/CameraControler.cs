using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControler : MonoBehaviour
{
    public Transform target;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float followSpeed;
    [SerializeField] private Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        FollowTarget();
        RotateToTarget();

    }

    public void FollowTarget()
    {
        if(target != null)
        {
            var targetPos = target.TransformPoint(offset);
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        }
    }

    public void RotateToTarget()

    {
        if (target != null)
        {
            var direction = target.position - transform.position;
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        }

    }
}
