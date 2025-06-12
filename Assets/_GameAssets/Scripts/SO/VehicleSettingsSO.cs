using UnityEngine;

[CreateAssetMenu(fileName = "Vehicle Settings", menuName = "Scriptable Objects/Vehicle Settings")]
public class VehicleSettingsSO : ScriptableObject
{
    [Header("Wheel Paddings")]
    [SerializeField] private float _wheelsPaddingX;
    [SerializeField] private float _wheelsPaddingZ;

    [Header("Suspension")]
    [SerializeField] private float _springRestLength;
    [SerializeField] private float _springStrength;
    [SerializeField] private float _springDamper;

    [Header("Handling")]
    [SerializeField] private float _steerAngle;
    [SerializeField] private float _frontWheelsGripFactor;
    [SerializeField] private float _rearWheelsGripFactor;

    [Header("Body")]
    [SerializeField] private float _tireMass;


    [Header("Power")]
    [SerializeField] private float _acceleratePower;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _maxReverseSpeed;
    [SerializeField] private float _brakesPower;

    [Header("Air Resistence")]
    [SerializeField] private float _airResistence;

    public float WheelsPaddingX => _wheelsPaddingX;
    public float WheelsPaddingZ => _wheelsPaddingZ;
    public float SpringRestLength => _springRestLength;
    public float SpringStrength => _springStrength;
    public float SpringDamper => _springDamper;
    public float SteerAngle => _steerAngle;
    public float FrontWheelsGripFactor => _frontWheelsGripFactor;
    public float RearWheelsGripFactor => _rearWheelsGripFactor;
    public float TireMass => _tireMass;
    public float AcceleratePower => _acceleratePower;
    public float MaxSpeed => _maxSpeed;
    public float MaxReverseSpeed => _maxReverseSpeed;
    public float BrakesPower => _brakesPower;
    public float AirResistence => _airResistence;
}
