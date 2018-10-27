using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class UserMovementInput : MonoBehaviour {

    // Singleton.
    public static UserMovementInput Instance { get; private set; }
    public IObservable<Vector2> Movement { get; private set; }
    public IObservable<Vector2> Mouselook { get; private set; }
    //reactive property, just like normal property but can be observed!
    public ReadOnlyReactiveProperty<bool> Run { get; private set; }

    private void Awake()
    {
        Instance = this;
        // Hide the mouse cursor and lock it in the game window.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Movement inputs tick on FixedUpdate
        Movement = this.FixedUpdateAsObservable()
            .Select(_ =>
            {
                var x = Input.GetAxis("Horizontal");
                var y = Input.GetAxis("Vertical");
                return new Vector2(x, y).normalized;
            });

        // Mouse look ticks on Update
        Mouselook = this.UpdateAsObservable()
            .Select(_ => {
                var x = Input.GetAxis("Mouse X");
                var y = Input.GetAxis("Mouse Y");
                return new Vector2(x, y);
            });

        // Run on shift
        Run = this.UpdateAsObservable()
            .Select(_ => Input.GetButton("Fire3"))
            .ToReadOnlyReactiveProperty();
    }

}
