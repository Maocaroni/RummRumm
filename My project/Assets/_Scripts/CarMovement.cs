using System.Collections;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private Rigidbody _rb;

    [Header("Values")]
    public CarSo car;
    [HideInInspector] public float speed;
    private float _steeringAngle;

    [Header("Wheels")]
    [SerializeField] private WheelCollider[] _wheelCollider;
    [SerializeField] private Transform[] _wheelTransform;

    [Header("Lights")]
    [SerializeField] private Light[] _headlights;
    [SerializeField] private KeyCode _lightToggleKey = KeyCode.L;

    [Header("Center of Mass Setup")]
    [SerializeField] private Transform centerOfMassObject; 

    [Header("Jump, Grip & Stability")]
    [SerializeField] private float gravityMultiplier = 3.0f; 
    [SerializeField] private float sidewaysGrip = 2.5f;       
    [SerializeField] private float forwardGrip = 2.0f;        
    [SerializeField] private float downforce = 50f;         

    [Header("Checkpoint & Respawn Setup")]
    [SerializeField] private float checkpointInterval = 3f; 
    [SerializeField] private LayerMask groundLayer;          
    [SerializeField] private float raycastDistance = 1.5f;   
    [SerializeField] private float fallLimitY = -50f;        

    private Vector3 lastCheckpointPos;
    private Quaternion lastCheckpointRot;
    private bool isGroundedForCheckpoint = false;

    // Variables de control para el Algodón
    private bool isInsideCotton = false;
    private bool isCottonSlowed = false;
    private float cottonImmunityEndTime = 0f;
    private Coroutine cottonLingeringCoroutine;
    private float originalDrag;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        if (_rb != null)
        {
            if (centerOfMassObject != null)
            {
                _rb.centerOfMass = transform.InverseTransformPoint(centerOfMassObject.position);
            }
            else
            {
                _rb.centerOfMass = new Vector3(0, -0.5f, -0.2f);
            }

            originalDrag = _rb.drag; 
        }

        if (car != null)
        {
            speed = car.speed;
        }
        else
        {
            Debug.LogError("¡Falta asignar el Scriptable Object 'CarSo' en el carro!");
        }

        AdjustWheelFriction();

        SaveCurrentCheckpoint();
        StartCoroutine(AutoCheckpointRoutine());
    }

    void Update()
    {
        if (Input.GetKeyDown(_lightToggleKey))
        {
            ToggleHeadlights();
        }

        CheckFallLimit();
    }

    private void FixedUpdate()
    {
        Motor();
        Brake();
        Steering();
        ApplyCustomGravity(); 
        ApplyDownforce();     
    }

    private void LateUpdate()
    {
        UpdateWheels();
    }

    public void Motor()
    {
        if (InputController.instance == null || _wheelCollider == null) return;

        foreach (var wheel in _wheelCollider)
        {
            if (wheel != null)
            {
                wheel.motorTorque = InputController.instance.movementVector.y * speed;
            }
        }
    }

    public void Brake()
    {
        if (InputController.instance == null || car == null || _wheelCollider == null) return;

        if (InputController.instance.isBraking)
        {
            foreach (var wheel in _wheelCollider)
            {
                if (wheel != null) wheel.brakeTorque = car.brakeForce;
            }
        }
        else
        {
            foreach (var wheel in _wheelCollider)
            {
                if (wheel != null) wheel.brakeTorque = 0;
            }
        }
    }

    public void Steering()
    {
        if (InputController.instance == null || car == null || _wheelCollider == null || _wheelCollider.Length < 4) return;

        _steeringAngle = car.angle * InputController.instance.movementVector.x;

        if (_wheelCollider[2] != null) _wheelCollider[2].steerAngle = _steeringAngle;
        if (_wheelCollider[3] != null) _wheelCollider[3].steerAngle = _steeringAngle;
    }

    public void UpdateWheels()
    {
        if (_wheelCollider == null || _wheelTransform == null) return;

        for (int i = 0; i < _wheelCollider.Length; i++)
        {
            if (_wheelCollider[i] != null && i < _wheelTransform.Length && _wheelTransform[i] != null)
            {
                UpdateSingleWheel(_wheelCollider[i], _wheelTransform[i]);
            }
        }
    }

    public void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    public void ToggleHeadlights()
    {
        if (_headlights == null) return;

        foreach (var light in _headlights)
        {
            if (light != null)
            {
                light.enabled = !light.enabled;
            }
        }
    }

    public void ApplyForceBoost(Vector3 finalBoostVector)
    {
        if (_rb != null)
        {
            _rb.AddForce(finalBoostVector, ForceMode.VelocityChange);
        }
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        StartCoroutine(SpeedBoostRoutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostRoutine(float multiplier, float duration)
    {
        float originalSpeed = speed;
        speed *= multiplier; 
        yield return new WaitForSeconds(duration); 
        speed = originalSpeed; 
    }

    void AdjustWheelFriction()
    {
        if (_wheelCollider == null) return;

        foreach (var wheel in _wheelCollider)
        {
            if (wheel != null)
            {
                WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
                sidewaysFriction.extremumValue = sidewaysGrip;
                sidewaysFriction.asymptoteValue = sidewaysGrip * 0.8f;
                wheel.sidewaysFriction = sidewaysFriction;

                WheelFrictionCurve forwardFriction = wheel.forwardFriction;
                forwardFriction.extremumValue = forwardGrip;
                forwardFriction.asymptoteValue = forwardGrip * 0.75f;
                wheel.forwardFriction = forwardFriction;
            }
        }
    }

    void ApplyCustomGravity()
    {
        if (_rb == null || _wheelCollider == null) return;

        bool isGrounded = false;
        foreach (var wheel in _wheelCollider)
        {
            if (wheel != null && wheel.isGrounded)
            {
                isGrounded = true;
                break;
            }
        }

        if (!isGrounded)
        {
            _rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    void ApplyDownforce()
    {
        if (_rb == null) return;
        _rb.AddForce(-transform.up * downforce * _rb.velocity.magnitude);
    }

    // --- SISTEMA DE CHECKPOINTS Y RESPAWN ---

    IEnumerator AutoCheckpointRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkpointInterval);
            CheckIfGroundedForCheckpoint();
            if (isGroundedForCheckpoint)
            {
                SaveCurrentCheckpoint();
            }
        }
    }

    void CheckIfGroundedForCheckpoint()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        isGroundedForCheckpoint = Physics.Raycast(origin, Vector3.down, raycastDistance, groundLayer);
    }

    void SaveCurrentCheckpoint()
    {
        lastCheckpointPos = transform.position;
        lastCheckpointRot = transform.rotation;
    }

    public void RespawnAtLastCheckpoint()
    {
        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        transform.position = lastCheckpointPos;
        transform.rotation = lastCheckpointRot;

        StartCoroutine(ResetPhysicsNextFrame());
    }

    IEnumerator ResetPhysicsNextFrame()
    {
        if (_rb != null) _rb.isKinematic = true;
        yield return new WaitForFixedUpdate();
        if (_rb != null) _rb.isKinematic = false;
    }

    void CheckFallLimit()
    {
        if (transform.position.y < fallLimitY)
        {
            RespawnAtLastCheckpoint();
        }
    }

    // --- MÉTODOS PARA EL ALGODÓN (Requeridos por ItemAndPlatform) ---

    public void EnterCottonZone(float cottonDrag)
    {
        if (Time.time < cottonImmunityEndTime) return;

        isInsideCotton = true;

        if (cottonLingeringCoroutine != null)
        {
            StopCoroutine(cottonLingeringCoroutine);
            cottonLingeringCoroutine = null;
        }

        if (!isCottonSlowed)
        {
            if (_rb != null)
            {
                _rb.drag = cottonDrag;
            }
            isCottonSlowed = true;
        }
    }

    public void ExitCottonZone(float lingerDuration, float immunityDuration)
    {
        if (!isInsideCotton) return;
        isInsideCotton = false;

        if (cottonLingeringCoroutine != null)
        {
            StopCoroutine(cottonLingeringCoroutine);
        }
        cottonLingeringCoroutine = StartCoroutine(CottonLingeringRoutine(lingerDuration, immunityDuration));
    }

    private IEnumerator CottonLingeringRoutine(float linger, float immunity)
    {
        yield return new WaitForSeconds(linger);

        if (isCottonSlowed)
        {
            if (_rb != null)
            {
                _rb.drag = originalDrag;
            }
            isCottonSlowed = false;
        }

        cottonImmunityEndTime = Time.time + immunity;
    }

    // --- DETECCIÓN DE OBSTÁCULOS ---

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Obstacle"))
        {
            RespawnAtLastCheckpoint();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle") || other.CompareTag("DeathZone") || other.CompareTag("Water"))
        {
            RespawnAtLastCheckpoint();
        }
    }
}