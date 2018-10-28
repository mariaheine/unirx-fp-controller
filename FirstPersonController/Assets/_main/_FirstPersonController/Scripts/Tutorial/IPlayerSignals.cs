using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public abstract class IPlayerSignals : MonoBehaviour
{
    public abstract float StrideLength { get; }
    public abstract IObservable<Vector3> Walked { get; }
}
