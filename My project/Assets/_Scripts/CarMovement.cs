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
            _rb.centerOfMass = new Vector3(0, -0.5f, 0);
            originalDrag = _rb.drag; // Guarda el drag original (en versiones anteriores de Unity usa _rb.drag)
        }

        if (car != null)
        {
            speed = car.speed;
        }
        else
        {
            Debug.LogError("¡Falta asignar el Scriptable Object 'CarSo' en el carro!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(_lightToggleKey))
        {
            ToggleHeadlights();
        }
    }

    private void FixedUpdate()
    {
        Motor();
        Brake();
        Steering();
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

    public void ApplyForceBoost(float pushForce)
    {
        if (_rb != null)
        {
            _rb.AddForce(transform.forward * pushForce, ForceMode.VelocityChange);
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

    // **ZONA DE ALGODÓN (RESISTENCIA FÍSICA / DRAG)**

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
            // Aumentamos el drag (resistencia al avance) para frenar al carro de forma natural
            if (_rb != null)
            {
                _rb.drag = cottonDrag; // Si usas una versión anterior a Unity 6, cambia linearDamping por drag
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
        // Mantiene la resistencia durante el tiempo de salida
        yield return new WaitForSeconds(linger);

        if (isCottonSlowed)
        {
            if (_rb != null)
            {
                _rb.drag = originalDrag; // Restaura el drag normal
            }
            isCottonSlowed = false;
        }

        cottonImmunityEndTime = Time.time + immunity;
    }
}