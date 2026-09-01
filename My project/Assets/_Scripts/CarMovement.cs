using System.Collections;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    // Almacena el componente Rigidbody para controlar las físicas y masa del vehículo.
    private Rigidbody _rb;

    [Header("Values")]
    // Scriptable Object que contiene las estadísticas base del carro (velocidad, fuerza de frenado, ángulo).
    // Se usa un SO para poder reutilizar datos en diferentes tipos de carros sin modificar el código.
    public CarSo car;

    // Velocidad final actual del carro, oculta en el inspector pero accesible para otros scripts (por eso [HideInInspector]).
    [HideInInspector] public float speed;

    // Ángulo de giro actual calculado en base al input del usuario.
    private float _steeringAngle;

    [Header("Wheels")]
    // Arreglo de WheelColliders: necesarios para simular la suspensión y tracción física de cada llanta con el terreno.
    [SerializeField] private WheelCollider[] _wheelCollider;

    // Arreglo de Transforms para las llantas físicas (los modelos 3D visibles), para que giren y se muevan junto a los colliders.
    [SerializeField] private Transform[] _wheelTransform;

    [Header("Lights")]
    // Arreglo para almacenar los componentes Light (faros) y poder encenderlos/apagarlos en lote.
    [SerializeField] private Light[] _headlights;

    // Tecla configurable desde el Inspector para alternar las luces sin modificar código fuente.
    [SerializeField] private KeyCode _lightToggleKey = KeyCode.L;

    void Start()
    {
        // Obtenemos el Rigidbody al iniciar para evitar buscarlo en cada frame (optimización).
        _rb = GetComponent<Rigidbody>();

        if (_rb != null)
        {
            // Bajamos artificialmente el centro de masa para evitar que el carro se voltee fácilmente al tomar curvas cerradas.
            _rb.centerOfMass = new Vector3(0, -0.5f, 0);
        }

        // Inicializamos la velocidad del carro desde el Scriptable Object de forma segura.
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
        // Update se ejecuta cada frame. Es el lugar ideal para leer inputs de teclado 
        // porque FixedUpdate puede perder pulsaciones rápidas de teclas.
        if (Input.GetKeyDown(_lightToggleKey))
        {
            ToggleHeadlights();
        }
    }

    private void FixedUpdate()
    {
        // FixedUpdate se usa exclusivamente para cálculos de físicas (Rigidbody, WheelColliders) 
        // para asegurar que el movimiento sea estable y dependa del tiempo de física de Unity.
        Motor();
        Brake();
        Steering();
        UpdateWheels();
    }

    public void Motor()
    {
        // Validamos que el InputController exista y el arreglo no esté vacío para evitar errores de NullReferenceException.
        if (InputController.instance == null || _wheelCollider == null) return;

        // Recorremos todas las llantas para aplicar la fuerza del motor (torque) según la entrada vertical del usuario.
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

        // Si el usuario presiona el freno, aplicamos la fuerza de frenado del Scriptable Object a todas las llantas.
        if (InputController.instance.isBraking)
        {
            foreach (var wheel in _wheelCollider)
            {
                if (wheel != null) wheel.brakeTorque = car.brakeForce;
            }
        }
        else
        {
            // Si no frena, liberamos el freno poniendo el torque en 0 para que el carro ruede libremente.
            foreach (var wheel in _wheelCollider)
            {
                if (wheel != null) wheel.brakeTorque = 0;
            }
        }
    }

    public void Steering()
    {
        // Validamos que existan al menos 4 llantas para evitar errores al intentar girar las delanteras.
        if (InputController.instance == null || car == null || _wheelCollider == null || _wheelCollider.Length < 4) return;

        // Calculamos el ángulo de giro multiplicando el ángulo máximo del carro por la entrada horizontal del usuario.
        _steeringAngle = car.angle * InputController.instance.movementVector.x;

        // Aplicamos el ángulo únicamente a las ruedas delanteras (índices 2 y 3 del arreglo de colliders).
        if (_wheelCollider[2] != null) _wheelCollider[2].steerAngle = _steeringAngle;
        if (_wheelCollider[3] != null) _wheelCollider[3].steerAngle = _steeringAngle;
    }

    public void UpdateWheels()
    {
        if (_wheelCollider == null || _wheelTransform == null) return;

        // Sincronizamos la posición y rotación física del collider con el modelo visual 3D de cada llanta.
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
        // Obtenemos la posición y rotación exacta calculada por la física del WheelCollider.
        wheelCollider.GetWorldPose(out pos, out rot);
        // Asignamos esos valores al Transform del modelo 3D de la llanta para que se mueva visualmente.
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    // **SISTEMA DE LUCES**
    public void ToggleHeadlights()
    {
        // Validamos que el arreglo de luces no sea nulo antes de iterar para prevenir errores.
        if (_headlights == null) return;

        // Recorremos cada componente Light dentro del arreglo.
        foreach (var light in _headlights)
        {
            if (light != null)
            {
                // Invertimos el estado actual: si estaba encendido (true) pasa a apagado (false) y viceversa.
                light.enabled = !light.enabled;
            }
        }
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        // Iniciamos una Corrutina para aplicar un aumento temporal de velocidad sin congelar el juego.
        StartCoroutine(SpeedBoostRoutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostRoutine(float multiplier, float duration)
    {
        float originalSpeed = speed;
        speed *= multiplier; // Multiplicamos la velocidad
        yield return new WaitForSeconds(duration); // Esperamos el tiempo indicado
        speed = originalSpeed; // Restauramos la velocidad original
    }
}