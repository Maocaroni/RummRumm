using System.Collections;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    // Para almacenar el Rigidbody
    private Rigidbody _rb;

    // Variable del Scriptable Object del carro.
    [Header("Values")]
    public CarSo car;

    // Variable para almacenar la velocidad final del carro
    [HideInInspector] public float speed;

    // Variable para almacenar el angulo del carro
    private float _steeringAngle;

    // Arreglo para almacenar los colisionadores de llanta
    [Header("Wheels")]
    [SerializeField] private WheelCollider[] _wheelCollider;

    // Arreglo para almacenar las llantas fisicas
    [SerializeField] private Transform[] _wheelTransform;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        if (_rb != null)
        {
            _rb.centerOfMass = new Vector3(0, -0.5f, 0);
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

    private void FixedUpdate()
    {
        Motor();
        Brake();
        Steering();
        UpdateWheels();
    }

    public void Motor()
    {
        // Seguridad: Si no hay InputController o el arreglo de ruedas está vacío, no hace nada para evitar el error
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
}