using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityTutorial.Manager;
using static UnityEngine.Rendering.DebugUI.Table;

namespace UnityTutorial.PlayerControl
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float AnimBlendSpeed = 16f;
        [SerializeField] private Transform CameraRoot;
        [SerializeField] private Transform Camera;
        [SerializeField] private float UpperLimit = -40f;
        [SerializeField] private float BottomLimit = 70f;
        [SerializeField] private float MouseSensitivity = 20f;
        [SerializeField] private float JumpFactor = 260f;
        [SerializeField] private float DistanceToGround = 0.8f;
        [SerializeField] private LayerMask GroundCheck;
        [SerializeField] private float AirResistance = 0.8f;

        [SerializeField] private Rigidbody shipRB;

        private Rigidbody _playerRigidbody;
        private InputManager _inputManager;
        private Animator _animator;
        private bool _grounded;
        private bool _hasAnimator;
        private int _xVelHash;
        private int _yVelHash;
        private int _jumpHash;
        private int _groundHash;
        private int _fallingHash;
        private int _zVelHash;
        private float _xRotation;
        //private float _yRotation;

        private const float _walkSpeed = 3f;
        private const float _runSpeed = 6f;

        private Vector2 _currentVelocity;

        bool rotateLeft = false;
        bool rotateRight = false;

        void Start()
        {
            _hasAnimator = TryGetComponent<Animator>(out _animator);
            _playerRigidbody = GetComponent<Rigidbody>();
            _inputManager = GetComponent<InputManager>();

            _xVelHash = Animator.StringToHash("X_Velocity");
            _yVelHash = Animator.StringToHash("Y_Velocity");
            _jumpHash = Animator.StringToHash("Jump");
            _groundHash = Animator.StringToHash("Grounded");
            _fallingHash = Animator.StringToHash("Falling");
            _zVelHash = Animator.StringToHash("Z_Velocity");
        }
        void Update()
        {
            //CamMovement();

            if (Input.GetKey(KeyCode.E))
            {
                rotateRight = true;
            }
            else 
            {
                rotateRight = false;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                rotateLeft = true;
            }
            else
            {
                rotateLeft = false;
            }
        }

        void FixedUpdate()
        {
            //_playerRigidbody.AddForce(Physics.gravity * _playerRigidbody.mass);
            CamMovement();

            SampleGround();
            Move();
            // HandleJump();

            //if(Input.GetKeyDown(KeyCode.E)) 
            //{
            //    shipRB.rotation = new Quaternion(shipRB.rotation.x + 1, shipRB.rotation.y, shipRB.rotation.z, shipRB.rotation.w);
            //}
            //if (Input.GetKeyDown(KeyCode.Q))
            //{
            //    shipRB.rotation = new Quaternion(shipRB.rotation.x - 1, shipRB.rotation.y, shipRB.rotation.z, shipRB.rotation.w);
            //}

            if (rotateLeft)
            {
                shipRB.angularVelocity = new Vector3(1 * Time.fixedDeltaTime, 0, 0);
            }
            else if (rotateRight)
            {
                shipRB.angularVelocity = new Vector3(-1 * Time.fixedDeltaTime, 0, 0);
            }
            //else
            //{
            //    shipRB.angularVelocity = new Vector3(0, 0, 0);
            //}
        }

        private void Move()
        {
            if (!_hasAnimator) return;

            float targetSpeed = _inputManager.Run ? _runSpeed : _walkSpeed;
            if (_inputManager.Move == Vector2.zero) targetSpeed = 0;

            if (_grounded)
            {

                if (targetSpeed == _runSpeed)
                {
                    if (Mathf.Abs(_currentVelocity.y) < _walkSpeed)
                    {
                        _currentVelocity.x = Mathf.Lerp(_currentVelocity.x, _inputManager.Move.x * targetSpeed, AnimBlendSpeed / 2 * Time.fixedDeltaTime);
                        _currentVelocity.y = Mathf.Lerp(_currentVelocity.y, _inputManager.Move.y * targetSpeed, AnimBlendSpeed / 2 * Time.fixedDeltaTime);
                    }
                    else
                    {
                        _currentVelocity.x = Mathf.Lerp(_currentVelocity.x, _inputManager.Move.x * targetSpeed, AnimBlendSpeed * Time.fixedDeltaTime);
                        _currentVelocity.y = Mathf.Lerp(_currentVelocity.y, _inputManager.Move.y * targetSpeed, AnimBlendSpeed / 2 * Time.fixedDeltaTime);
                    }
                }
                else
                {
                    _currentVelocity.x = Mathf.Lerp(_currentVelocity.x, _inputManager.Move.x * targetSpeed, AnimBlendSpeed * Time.fixedDeltaTime);
                    _currentVelocity.y = Mathf.Lerp(_currentVelocity.y, _inputManager.Move.y * targetSpeed, AnimBlendSpeed * Time.fixedDeltaTime);
                }

                var xVelDifference = _currentVelocity.x - _playerRigidbody.velocity.x;
                var zVelDifference = _currentVelocity.y - _playerRigidbody.velocity.z;

                _playerRigidbody.AddForce(transform.TransformVector(new Vector3(xVelDifference, 0, zVelDifference)), ForceMode.VelocityChange);

                //_playerRigidbody.AddForce(transform.TransformVector(new Vector3(_currentVelocity.x, 0, _currentVelocity.y)), ForceMode.VelocityChange);

            }
            else
            {
                _playerRigidbody.AddForce(transform.TransformVector(new Vector3(_currentVelocity.x * AirResistance, 0, _currentVelocity.y * AirResistance)), ForceMode.VelocityChange);
            }

            _animator.SetFloat(_xVelHash, _currentVelocity.x);
            _animator.SetFloat(_yVelHash, _currentVelocity.y);

            //_animator.SetFloat(_xVelHash, _playerRigidbody.velocity.x);
            //_animator.SetFloat(_yVelHash, _playerRigidbody.velocity.z);
        }

        private void CamMovement()
        {
            if (!_hasAnimator) return;

            var Mouse_X = _inputManager.Look.x;
            var Mouse_Y = _inputManager.Look.y;
            Camera.position = CameraRoot.position;

            _xRotation -= Mouse_Y * MouseSensitivity * Time.smoothDeltaTime;
            _xRotation = Mathf.Clamp(_xRotation, UpperLimit, BottomLimit);

            Camera.localRotation = Quaternion.Euler(_xRotation, 0, 0);
            //transform.Rotate(Vector3.up, Mouse_X * MouseSensitivity * Time.deltaTime);

            if (_grounded)
            {
                _playerRigidbody.MoveRotation(_playerRigidbody.rotation * Quaternion.Euler(0, Mouse_X * MouseSensitivity * Time.smoothDeltaTime, 0));
            }
        }

        private void HandleJump()
        {
            if (!_hasAnimator) return;
            if (!_inputManager.Jump) return;
            if (!_grounded)
            {
                _animator.ResetTrigger(_jumpHash);
                return;
            }
            _animator.SetTrigger(_jumpHash);

            //B-hops
            //_playerRigidbody.AddForce(-_playerRigidbody.velocity.y * Vector3.up, ForceMode.VelocityChange);
            //_playerRigidbody.AddForce(Vector3.up * JumpFactor, ForceMode.Impulse);
            //_animator.ResetTrigger(_jumpHash);
        }

        //Comment out if B-hops implemented
        public void JumpAddForce()
        {
            _playerRigidbody.AddForce(-_playerRigidbody.velocity.y * Vector3.up, ForceMode.VelocityChange);
            _playerRigidbody.AddForce(Vector3.up * JumpFactor, ForceMode.Impulse);
            _animator.ResetTrigger(_jumpHash);
        }

        private void SampleGround()
        {
            if (!_hasAnimator) return;

            RaycastHit hitInfo;
            if (Physics.Raycast(_playerRigidbody.worldCenterOfMass, Vector3.down, out hitInfo, DistanceToGround + 0.1f, GroundCheck))
            {
                //Collided with something
                //Grounded
                _grounded = true;
                SetAnimationGrounding();
                return;
            }
            //Falling
            //Debug.Log(_grounded);
            _grounded = false;
            _animator.SetFloat(_zVelHash, _playerRigidbody.velocity.y);
            SetAnimationGrounding();
            _animator.ResetTrigger(_jumpHash);

            /// Disable Grounded
            _grounded = true;

            return;
        }

        private void SetAnimationGrounding()
        {
            _animator.SetBool(_fallingHash, !_grounded);
            _animator.SetBool(_groundHash, _grounded);
        }

    }
}