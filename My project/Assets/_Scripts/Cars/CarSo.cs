using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Car", menuName = "Car/NewCar")]

public class CarSo : ScriptableObject
{

    public float speed;
    public float brakeForce;
    public float angle;
    
}
