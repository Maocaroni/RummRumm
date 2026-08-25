
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionMenu : MonoBehaviour
{
    [Header("UI")]
    // Almacena los elementos de la interfaz

    // Varaible que almacena la imagen de los carros que aparece en el menu de seleccion
    public Image carImage;

    // Varaible que almacena el nombre de los carros que aparece en el menu de seleccion
    public TextMeshProUGUI carNameText;

    // Los scrollbars que se muestran los stats de los carros
    public Scrollbar speedScrollbar;
    public Scrollbar brakeScrollbar;
    public Scrollbar angleScrollbar;

    [Header("Cars")]

    // Guarda la camara
    public CameraControler cam;

    // Almacena los prefabs de los carros
    public CarSo[] cars;

    // Guarda el punto inicial
    public Transform initialPos;

    // Va a guardar el carro seleccionado
    private CarSo selectedCar;

    // Tamaño del maximo valor posible en el slider 
    [SerializeField] private float maxScrollbar = 2000;
    [SerializeField] private float maxScrollbarAngle = 60;

    // Guarda el indice del arreglo
    private int carIndex;


    private void Start()
    {
        carIndex = 0;
        selectedCar = cars[carIndex];
        UIUpdate();
    }
    // Actualiza los elementos de la interfaz grafica
    public void UIUpdate()
    {
        carImage.sprite = selectedCar.carImage;
        carNameText.text = selectedCar.carName;
        speedScrollbar.size = selectedCar.speed / maxScrollbar;
        brakeScrollbar.size = selectedCar.brakeForce / maxScrollbar;
        angleScrollbar.size = selectedCar.angle / maxScrollbarAngle;
    }

    /// Los metodos de los botones
    /// 

    // Boton de izquierda y derecha

    public void CharacterChange(bool isButtonRight)
    {
        if (isButtonRight)
        {
            // Al presionar el boton de la derecha, el indice avanza
            carIndex = (carIndex + 1) % cars.Length;

        }
        else
        {
            // Al presionar el boton de la izquierda, el indice disminuye
            carIndex = (carIndex - 1 + cars.Length) % cars.Length;
        }

        selectedCar = cars[carIndex];
        UIUpdate() ;
    }

    // Boton de seleccion
    public void SelectCar()
    {
        // Al presionar el boton de seleccionar carro, se crea el elegido en escena y se le asigna a la camara el target
        GameObject prefabSelected = Instantiate(selectedCar.carPrefab, initialPos.position, Quaternion.identity);
        cam.target = prefabSelected.transform;
    }


}
