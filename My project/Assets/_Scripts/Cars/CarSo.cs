using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Car", menuName = "Car/NewCar")]

public class CarSo : ScriptableObject
{

    public float speed;
    public float brakeForce;
    public float angle;
    public Sprite carImage;
    public string carName;
    public GameObject carPrefab;
}
