using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
public class PlayerVehicleController : NetworkBehaviour
{
    public class SpringData
    {
        public float _currentLength;
        public float _currentVelocity;
    }

    private static readonly WheelType[] _wheels = new WheelType[]
    {
        WheelType.FrontLeft,
        WheelType.FrontRight,
        WheelType.BackLeft,
        WheelType.BackRight
    };
    private static readonly WheelType[] _backWheels = new WheelType[]
    {
        WheelType.BackLeft,
        WheelType.BackRight
    };

    [Header("References")]
    [SerializeField] private VehicleSettingsSO _vehicleSettings;
    [SerializeField] private Rigidbody _vehicleRigidbody;
    [SerializeField] private BoxCollider _vehicleCollider;

    [Header("Settings")]
    [SerializeField] private float _crashForce;
    [SerializeField] private float _crashTorque;

    public event Action OnVehicleCrashed;

    public VehicleSettingsSO Settings => _vehicleSettings;
    public Vector3 Forward => transform.forward;
    public Vector3 Velocity => _vehicleRigidbody.linearVelocity;

    private Dictionary<WheelType, SpringData> _springDatas;
    private float _steerInput;
    private float _accelerationInput;

    private void Awake()
    {
        _springDatas = new Dictionary<WheelType, SpringData>();

        foreach (var wheelType in _wheels)
        {
            _springDatas.Add(wheelType, new());
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _vehicleRigidbody.isKinematic = true;
        SetOwnerRigidbodyKinematicAsync();
    }
    private void Update()
    {
        if (!IsOwner) { return; }

        SetSteerInput(Input.GetAxis("Horizontal"));
        SetAccelerateInput(Input.GetAxis("Vertical"));

        if (Input.GetKeyDown(KeyCode.T))
        {
            CrashVehicle();
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) { return; }

        // SÜSPANSİYON
        UpdateSuspension();
        // STEERING - YER YÖN
        UpdateSteering();
        // ACCELERATION - HIZLANMA
        UpdateAcceleration();
        // BREAKING - YAVAŞLAMA
        UpdateBreaks();
        // AIR RESISTENCE - HAVADAN DİRENCİ
        UpdateAirResistence();
    }

    private void SetSteerInput(float steerInput)
    {
        _steerInput = Mathf.Clamp(steerInput, -1f, 1f);
    }
    private void SetAccelerateInput(float accelerationInput)
    {
        _accelerationInput = Mathf.Clamp(accelerationInput, -1f, 1f);
    }

    private void UpdateSuspension()
    {
        foreach (var id in _springDatas.Keys)
        {
            CastSpring(id);
            float currentVelocity = _springDatas[id]._currentVelocity;
            float currentLength = _springDatas[id]._currentLength;

            float force = SpringMathExtensions.CalculateForceDamped(
                currentLength, currentVelocity,
                _vehicleSettings.SpringRestLength,
                _vehicleSettings.SpringStrength,
                _vehicleSettings.SpringDamper
            );

            _vehicleRigidbody.AddForceAtPosition(force * transform.up, GetSpringPosition(id));
        }
    }
    private void UpdateSteering()
    {
        foreach (var wheelType in _wheels)
        {
            if (!IsGrounded(wheelType))
            {
                continue;
            }

            Vector3 springPosition = GetSpringPosition(wheelType);
            Vector3 slideDirection = GetWheelSlideDirection(wheelType);

            float slideVelocity = Vector3.Dot(slideDirection, _vehicleRigidbody.GetPointVelocity(springPosition));

            float desiredVelocityChange = GetWheelGripFactor(wheelType) * -slideVelocity;
            float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

            Vector3 force = desiredAcceleration * slideDirection * _vehicleSettings.TireMass;
            _vehicleRigidbody.AddForceAtPosition(force, GetWheelTorquePosition(wheelType));
        }
    }
    private void UpdateAcceleration()
    {
        if (Mathf.Approximately(_accelerationInput, 0f))
        {
            return;
        }

        float forwardSpeed = Vector3.Dot(transform.forward, _vehicleRigidbody.linearVelocity);
        bool movingForward = forwardSpeed > 0f;
        float speed = Mathf.Abs(forwardSpeed);

        if (movingForward && speed > _vehicleSettings.MaxSpeed)
        {
            return;
        }
        else if (!movingForward && speed > _vehicleSettings.MaxReverseSpeed)
        {
            return;
        }

        foreach (var wheelType in _wheels)
        {
            if (!IsGrounded(wheelType)) continue;

            Vector3 position = GetWheelTorquePosition(wheelType);
            Vector3 wheelForward = GetWheelRollDirection(wheelType);
            _vehicleRigidbody.AddForceAtPosition(_accelerationInput * wheelForward * _vehicleSettings.AcceleratePower, position);
        }
    }
    private void UpdateBreaks()
    {
        float forwardSpeed = Vector3.Dot(transform.forward, _vehicleRigidbody.linearVelocity);
        float speed = Mathf.Abs(forwardSpeed);

        float breaksRatio;

        const float ALMOST_STOPPING_SPEED = 2f;
        bool almostStoppping = speed < ALMOST_STOPPING_SPEED;

        if (almostStoppping)
        {
            breaksRatio = 1f;
        }
        else
        {
            bool accelerateContrary = !Mathf.Approximately(_accelerationInput, 0) && Vector3.Dot(_accelerationInput * transform.forward, _vehicleRigidbody.linearVelocity) < 0f;

            if (accelerateContrary)
            {
                breaksRatio = 1f;
            }
            else if (Mathf.Approximately(_accelerationInput, 0f))
            {
                breaksRatio = 0.1f;
            }
            else
            {
                return;
            }
        }
        foreach (var wheelType in _backWheels)
        {
            if (!IsGrounded(wheelType)) continue;

            Vector3 springPosition = GetSpringPosition(wheelType);
            Vector3 rollDirection = GetWheelRollDirection(wheelType);
            float rollVelocity = Vector3.Dot(rollDirection, _vehicleRigidbody.GetPointVelocity(springPosition));

            float desiredVelocityChange = -rollVelocity * breaksRatio * _vehicleSettings.BrakesPower;
            float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

            Vector3 force = desiredAcceleration * _vehicleSettings.TireMass * rollDirection;
            _vehicleRigidbody.AddForceAtPosition(force, GetWheelTorquePosition(wheelType));
        }
    }
    private void UpdateAirResistence()
    {
        _vehicleRigidbody.AddForce(_vehicleCollider.size.magnitude * -_vehicleRigidbody.linearVelocity * _vehicleSettings.AirResistence);
    }

    private void CastSpring(WheelType wheelType)
    {
        Vector3 position = GetSpringPosition(wheelType);

        float previousLength = _springDatas[wheelType]._currentLength;
        float currentLength;

        if (Physics.Raycast(position, -transform.up, out var hit, _vehicleSettings.SpringRestLength))
        {
            currentLength = hit.distance;

        }
        else
        {
            currentLength = _vehicleSettings.SpringRestLength;
        }

        _springDatas[wheelType]._currentVelocity = (currentLength - previousLength) / Time.fixedDeltaTime;
        _springDatas[wheelType]._currentLength = currentLength;

    }
    private Vector3 GetSpringPosition(WheelType wheelType)
    {
        return transform.localToWorldMatrix.MultiplyPoint3x4(GetSpringRelativePosition(wheelType));
    }

    private Vector3 GetSpringRelativePosition(WheelType wheelType)
    {
        Vector3 boxSize = _vehicleCollider.size;
        float boxBottom = boxSize.y * -0.5f;

        float paddingX = _vehicleSettings.WheelsPaddingX;
        float paddingZ = _vehicleSettings.WheelsPaddingZ;

        return wheelType switch
        {
            WheelType.FrontLeft => new Vector3(boxSize.x * (paddingX - 0.5f), boxBottom, boxSize.z * (0.5f - paddingZ)),
            WheelType.FrontRight => new Vector3(boxSize.x * (0.5f - paddingX), boxBottom, boxSize.z * (0.5f - paddingZ)),
            WheelType.BackLeft => new Vector3(boxSize.x * (paddingX - 0.5f), boxBottom, boxSize.z * (paddingZ - 0.5f)),
            WheelType.BackRight => new Vector3(boxSize.x * (0.5f - paddingX), boxBottom, boxSize.z * (paddingZ - 0.5f)),
            _ => default,
        };
    }

    private Vector3 GetWheelTorquePosition(WheelType wheelType)
    {
        return transform.localToWorldMatrix.MultiplyPoint3x4(GetSpringRelativeTorquePosition(wheelType));
    }
    private Vector3 GetSpringRelativeTorquePosition(WheelType wheelType)
    {
        Vector3 boxSize = _vehicleCollider.size;

        float paddingX = _vehicleSettings.WheelsPaddingX;
        float paddingZ = _vehicleSettings.WheelsPaddingZ;

        return wheelType switch
        {
            WheelType.FrontLeft => new Vector3(boxSize.x * (paddingX - 0.5f), 0f, boxSize.z * (0.5f - paddingZ)),
            WheelType.FrontRight => new Vector3(boxSize.x * (0.5f - paddingX), 0f, boxSize.z * (0.5f - paddingZ)),
            WheelType.BackLeft => new Vector3(boxSize.x * (paddingX - 0.5f), 0f, boxSize.z * (paddingZ - 0.5f)),
            WheelType.BackRight => new Vector3(boxSize.x * (0.5f - paddingX), 0f, boxSize.z * (paddingZ - 0.5f)),
            _ => default,
        };
    }
    private Vector3 GetWheelSlideDirection(WheelType wheelType)
    {
        Vector3 forward = GetWheelRollDirection(wheelType);
        return Vector3.Cross(transform.up, forward);
    }
    private Vector3 GetWheelRollDirection(WheelType wheelType)
    {
        bool frontWheels = wheelType == WheelType.FrontLeft || wheelType == WheelType.FrontRight;
        if (frontWheels)
        {
            var steerQuaternion = Quaternion.AngleAxis(_steerInput * _vehicleSettings.SteerAngle, Vector3.up);
            return steerQuaternion * transform.forward;
        }
        else
        {
            return transform.forward;
        }
    }
    private float GetWheelGripFactor(WheelType wheelType)
    {
        bool frontWheels = wheelType == WheelType.FrontLeft || wheelType == WheelType.FrontRight;
        return frontWheels ? _vehicleSettings.FrontWheelsGripFactor : _vehicleSettings.RearWheelsGripFactor;
    }
    private bool IsGrounded(WheelType wheelType)
    {
        return _springDatas[wheelType]._currentLength < _vehicleSettings.SpringRestLength;
    }
    public float GetSpringCurrentLength(WheelType wheelType)
    {
        return _springDatas[wheelType]._currentLength;
    }
    public async void SetOwnerRigidbodyKinematicAsync()
    {
        if (IsOwner)
        {
            await UniTask.DelayFrame(1);
            _vehicleRigidbody.isKinematic = false;

        }
    }
    public void CrashVehicle()
    {
        OnVehicleCrashed?.Invoke();
        _vehicleRigidbody.AddForce(Vector3.up * _crashForce, ForceMode.Impulse);
        _vehicleRigidbody.AddTorque(Vector3.forward * _crashTorque, ForceMode.Impulse);
        enabled = false;
    }
    public static class SpringMathExtensions
    {
        public static float CalculateForceDamped(
            float currentLength, float lengthVelocity, float restLength,
            float strength, float damper)
        {
            float lengthOffset = restLength - currentLength;
            return (lengthOffset * strength) - (lengthVelocity * damper);
        }
    }
}