using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

//notice playersignals instead of monobehaviour
public class PlayerController : IPlayerSignals {

    private CharacterController yoCharacter;
    private Camera view;

    private float walkSpeed = 5f;
    public float runSpeed = 10f;
    [Range(-90, 0)] public float minViewAngle = -60f;    
    [Range(0, 90)] public float maxViewAngle = 60f; 
    public float XSensitivity = 1f;
    public float YSensitivity = 1f;

    private Subject<Vector3> walked; // We get to see this as a Subject
    // "Now we see a Subject and everyone else sees an IObservable. Nice and clean!"
    public override IObservable<Vector3> Walked
    {
        get { return walked; } // Everyone else sees it as an IObservable
    }

    public float strideLength = 2.5f;    
    // Implement IPlayerSignals
    public override float StrideLength
    {
        get { return strideLength; }
    }

    private void Awake()
    {
        yoCharacter = GetComponent<CharacterController>();
        view = GetComponentInChildren<Camera>();

        walked = new Subject<Vector3>().AddTo(this); //AddTo: constrain its lifecycle to our game object
    }

    private void Start()
    {
        var inputs = UserMovementInput.Instance;

        inputs.Movement
            .Where(v => v != Vector2.zero)
            .Subscribe(inputMovement =>
            {
                var inputVelocity = inputMovement * (inputs.Run.Value ? runSpeed : walkSpeed);

                var playerVelocity =
                    inputVelocity.x * transform.right +
                    inputVelocity.y * transform.forward;

                var distance = playerVelocity * Time.fixedDeltaTime;

                yoCharacter.Move(distance);

                //signal the move happened
                /* The OnNext method is used to provide a new value to the signal. 
                 * Everyone who is subscribed to the signal will get a notification with the new value.
                 */
                walked.OnNext(yoCharacter.velocity * Time.fixedDeltaTime);
                /* We haven't even reached the camera bob portion yet, but I think we've done something remarkable. 
                 * Our script not only "consumes" signals, it also produces them. 
                 * This is the fundamental glue that lets you integrate systems that aren't based on Observables, like Unity's physics and scene graph.
                 */

            }).AddTo(this); //read up on this, it is said to be "cleaning up" the no-longer-necessary shit

        // Handle mouse input (free mouse look).
        inputs.Mouselook
            .Where(v => v != Vector2.zero) // We can ignore this if mouse look is zero.
            .Subscribe(inputLook => {
                
                inputLook.x *= XSensitivity; //shit doesnt work without that
                                
                var horzLook = inputLook.x * Time.deltaTime * Vector3.up;
                transform.localRotation *= Quaternion.Euler(horzLook);

                inputLook.y *= YSensitivity; //shit doesnt work without that

                var vertLook = inputLook.y * Time.deltaTime * Vector3.left;
                var newQ = view.transform.localRotation * Quaternion.Euler(vertLook);

                view.transform.localRotation = ClampRotationAroundXAxis(newQ, -maxViewAngle, -minViewAngle);
            }).AddTo(this);
    }

    // Ripped straight out of the Standard Assets MouseLook script. (This should really be a standard function...)
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
