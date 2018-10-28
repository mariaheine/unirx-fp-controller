using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class RigidbodyPlayerController : MonoBehaviour {

    private Rigidbody _rBody;
    private Camera _view;
    
    [Header("Mouse Settings")]
    public float XSensitivity = 50f;
    public float YSensitivity = 50f;
    [Range(-90, 0)] public float minViewAngle = -60f;    
    [Range(0, 90)] public float maxViewAngle = 60f;

    [Header("Speed & Jump")]
    public int walkSpeed;
    public int runSpeed;
    public int jumpHeight;

    private ReadOnlyReactiveProperty<bool> _isGrounded;
    private float _distanceToGround;
    private float _groundDistanceMargin = .2f;

    private void Awake() {

        _rBody = GetComponent<Rigidbody>();
        _view = GetComponentInChildren<Camera>();

        _distanceToGround = GetComponent<CapsuleCollider>().height/2;
        _isGrounded = this.UpdateAsObservable()
            .Select(_ => {
                // Debug.Log(Physics.Raycast(transform.position, -Vector3.up, distanceToGround + .2f));
                return Physics.Raycast(transform.position, -Vector3.up, _distanceToGround + _groundDistanceMargin);
            })
            .ToReadOnlyReactiveProperty();
    }

    private void Start() 
    {
        var inputs = UserMovementInput.Instance;

        inputs.Movement
            .Where(v => v != Vector2.zero)
            .Subscribe(inputMovement => {    
                var inputVelocity = inputs.Run.Value ? runSpeed : walkSpeed ;          
                var movementDirection = inputMovement.y * transform.forward + inputMovement.x * transform.right;
                _rBody.MovePosition(_rBody.position + movementDirection * inputVelocity * Time.deltaTime);
            })
            .AddTo(this);

        inputs.Mouselook
            .Where(v => v != Vector2.zero) // We can ignore this if mouse look is zero.
            .Subscribe(inputLook => {
                
                inputLook.x *= XSensitivity; //shit doesnt work without that
                                
                var horzLook = inputLook.x * Time.deltaTime * Vector3.up;
                transform.localRotation *= Quaternion.Euler(horzLook);

                inputLook.y *= YSensitivity; //shit doesnt work without that

                var vertLook = inputLook.y * Time.deltaTime * Vector3.left;
                var newQ = _view.transform.localRotation * Quaternion.Euler(vertLook);

                _view.transform.localRotation = ClampRotationAroundXAxis(newQ, -maxViewAngle, -minViewAngle);
            }).AddTo(this);

        inputs.Jump
            .Where(j => j == true && _isGrounded.Value == true)
            .Subscribe(_ => {
                _rBody.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y), ForceMode.Impulse);
            });

    }

    private static Quaternion ClampRotationAroundXAxis(Quaternion q, float minAngle, float maxAngle)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, minAngle, maxAngle);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }
}