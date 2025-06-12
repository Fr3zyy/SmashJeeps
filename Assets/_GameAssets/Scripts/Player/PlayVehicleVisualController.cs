using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayVehicleVisualController : NetworkBehaviour
{
    [SerializeField] private PlayerVehicleController _playerVehicleController;
    [SerializeField] private Transform _wheelFrontLeft, _wheelFrontRight, _wheelBackLeft, _wheelBackRight;
    [SerializeField] private float _wheelsSpinSpeed, _wheelsYWhenSpringMin, _wheelsYWhenSpringMax;


    private Quaternion _wheelFrontLeftRoll;
    private Quaternion _wheelFrontRightRoll;

    private float _springRestLength;
    private float _forwardSpeed;
    private float _steerInput;
    private float _steerAngle;

    private Dictionary<WheelType, float> _springsCurrentLengths = new()
    {
        { WheelType.FrontLeft, 0f },
        { WheelType.FrontRight, 0f },
        { WheelType.BackLeft, 0f },
        { WheelType.BackRight, 0f  }
    };

    private void Start()
    {

        _wheelFrontLeftRoll = _wheelFrontLeft.localRotation;
        _wheelFrontRightRoll = _wheelFrontRight.localRotation;

        _springRestLength = _playerVehicleController.Settings.SpringRestLength;
        _steerAngle = _playerVehicleController.Settings.SteerAngle;
    }
    private void Update()
    {
        if (!IsOwner) { return; }


        UpdateVisualStates();
        RotateWheels();
        SetSuspension();
    }

    private void UpdateVisualStates()
    {
        _steerInput = Input.GetAxis("Horizontal");


        _forwardSpeed = Vector3.Dot(_playerVehicleController.Forward, _playerVehicleController.Velocity);

        _springsCurrentLengths[WheelType.FrontLeft] = _playerVehicleController.GetSpringCurrentLength(WheelType.FrontLeft);
        _springsCurrentLengths[WheelType.FrontRight] = _playerVehicleController.GetSpringCurrentLength(WheelType.FrontRight);
        _springsCurrentLengths[WheelType.BackLeft] = _playerVehicleController.GetSpringCurrentLength(WheelType.BackLeft);
        _springsCurrentLengths[WheelType.BackRight] = _playerVehicleController.GetSpringCurrentLength(WheelType.BackRight);
    }

    private void RotateWheels()
    {
        if (_springsCurrentLengths[WheelType.FrontLeft] < _springRestLength)
        {
            _wheelFrontLeftRoll *= Quaternion.AngleAxis(_forwardSpeed * _wheelsSpinSpeed * Time.deltaTime, Vector3.right);
        }
        if (_springsCurrentLengths[WheelType.FrontRight] < _springRestLength)
        {
            _wheelFrontRightRoll *= Quaternion.AngleAxis(_forwardSpeed * _wheelsSpinSpeed * Time.deltaTime, Vector3.right);
        }
        if (_springsCurrentLengths[WheelType.BackLeft] < _springRestLength)
        {
            _wheelBackLeft.localRotation *= Quaternion.AngleAxis(_forwardSpeed * _wheelsSpinSpeed * Time.deltaTime, Vector3.right);
        }
        if (_springsCurrentLengths[WheelType.BackRight] < _springRestLength)
        {
            _wheelBackRight.localRotation *= Quaternion.AngleAxis(_forwardSpeed * _wheelsSpinSpeed * Time.deltaTime, Vector3.right);
        }

        _wheelFrontLeft.localRotation = Quaternion.AngleAxis(_steerInput * _steerAngle, Vector3.up) * _wheelFrontLeftRoll;
        _wheelFrontRight.localRotation = Quaternion.AngleAxis(_steerInput * _steerAngle, Vector3.up) * _wheelFrontRightRoll;
    }

    private void SetSuspension()
    {
        float springFrontLeftRatio = _springsCurrentLengths[WheelType.FrontLeft] / _springRestLength;
        float springFrontRightRatio = _springsCurrentLengths[WheelType.FrontRight] / _springRestLength;
        float springBackLeftRatio = _springsCurrentLengths[WheelType.BackLeft] / _springRestLength;
        float springBackRightRatio = _springsCurrentLengths[WheelType.BackRight] / _springRestLength;

        _wheelFrontLeft.localPosition = new Vector3(_wheelFrontLeft.localPosition.x, _wheelsYWhenSpringMin + (_wheelsYWhenSpringMax - _wheelsYWhenSpringMin) * springFrontLeftRatio, _wheelFrontLeft.localPosition.z);
        _wheelFrontRight.localPosition = new Vector3(_wheelFrontRight.localPosition.x, _wheelsYWhenSpringMin + (_wheelsYWhenSpringMax - _wheelsYWhenSpringMin) * springFrontRightRatio, _wheelFrontRight.localPosition.z);
        _wheelBackLeft.localPosition = new Vector3(_wheelBackLeft.localPosition.x, _wheelsYWhenSpringMin + (_wheelsYWhenSpringMax - _wheelsYWhenSpringMin) * springBackLeftRatio, _wheelBackLeft.localPosition.z);
        _wheelBackRight.localPosition = new Vector3(_wheelBackRight.localPosition.x, _wheelsYWhenSpringMin + (_wheelsYWhenSpringMax - _wheelsYWhenSpringMin) * springBackRightRatio, _wheelBackRight.localPosition.z);
    }
}
