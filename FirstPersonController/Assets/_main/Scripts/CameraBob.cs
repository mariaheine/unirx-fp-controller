using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class CameraBob : MonoBehaviour
{
    // IPlayerSignals reference configured in the Unity Inspector, since we can
    // reasonably expect these game objects to be in the same hierarchy
    public IPlayerSignals player;
    public float walkBobMagnitude = 0.05f;
    public float runBobMagnitude = 0.10f;

    public AnimationCurve bob = new AnimationCurve(
        new Keyframe(0.00f, 0f),
        new Keyframe(0.25f, 1f),
        new Keyframe(0.50f, 0f),
        new Keyframe(0.75f, -1f),
        new Keyframe(1.00f, 0f));

    private Camera view;
    private Vector3 initialPosition;

    private void Awake()
    {
        view = GetComponent<Camera>();
        initialPosition = view.transform.localPosition;
    }

    private void Start()
    {
        var distance = 0f;
        player.Walked.Subscribe(w => {
            // Accumulate distance walked (modulo stride length).
            distance += w.magnitude;
            distance %= player.StrideLength;
            // Use distance to evaluate the bob curve.
            var magnitude = UserMovementInput.Instance.Run.Value ? runBobMagnitude : walkBobMagnitude;
            var deltaPos = magnitude * bob.Evaluate(distance / player.StrideLength) * Vector3.up;
            // Adjust camera position.
            view.transform.localPosition = initialPosition + deltaPos;
        }).AddTo(this);
    }
}
