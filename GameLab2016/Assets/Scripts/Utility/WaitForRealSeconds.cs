using System;
using UnityEngine;

public class WaitForRealSeconds : CustomYieldInstruction {

    private float _finishTime;

    public WaitForRealSeconds(float seconds) {
        _finishTime = seconds + Time.realtimeSinceStartup;
    }

    public override bool keepWaiting {
        get {
            return _finishTime > Time.realtimeSinceStartup;
        }
    }
}
