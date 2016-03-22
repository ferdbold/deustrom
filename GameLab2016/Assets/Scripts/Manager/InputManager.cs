using UnityEngine;
using System.Collections.Generic;

using AxisTuple = Eppy.Tuple<string, InputManager.EventAxis, float, float>;
using ButtonTuple = Eppy.Tuple<string, InputManager.EventButton, bool>;

public class InputManager : MonoBehaviour {

    /// <summary>
    /// Event structure
    /// </summary>
    public delegate void EventAxis(params float[] a);
    public delegate void EventButton();

    //Possible Event
    private EventAxis 
        p1_leftAnalog,
        p1_rightAnalog,
        p1_leftTrigger,
        p1_rightTrigger,
        p2_leftAnalog,
        p2_rightAnalog,
        p2_leftTrigger,
        p2_rightTrigger;

    private EventButton 
        _startButton,
        p1_fireHookButtonDown,
        p2_fireHookButtonDown,
        p1_fireHookButtonUp,
        p2_fireHookButtonUp,
        p1_pushButton,
        p2_pushButton,
        p1_retractHooksButtonDown,
        p2_retractHooksButtonDown,
        p1_retractHooksButtonUp,
        p2_retractHooksButtonUp,
        p1_cutLinkWithChainButton,
        p2_cutLinkWithChainButton;

    #region Add Event

    /// <summary>
    /// The name of every axis
    /// </summary>
    public enum Axis { 
        p1_leftAnalog,
        p1_rightAnalog,
        p1_leftTrigger,
        p1_rightTrigger, 
        p2_leftAnalog, 
        p2_rightAnalog, 
        p2_leftTrigger,
        p2_rightTrigger 
    }

    /// <summary>
    /// Link a function to an event
    /// </summary>
    /// <param name="input">The event to be used</param>
    /// <param name="eventFunction">The function to be call when the event is triggered</param>
    public void AddEvent(Axis axis, EventAxis eventFunction) {
        switch (axis) {
            case Axis.p1_leftAnalog: p1_leftAnalog += eventFunction; break;
            case Axis.p1_rightAnalog: p1_rightAnalog += eventFunction; break;
            case Axis.p1_leftTrigger: p1_leftTrigger += eventFunction; break;
            case Axis.p1_rightTrigger: p1_rightTrigger += eventFunction; break;
            case Axis.p2_leftAnalog: p2_leftAnalog += eventFunction; break;
            case Axis.p2_rightAnalog: p2_rightAnalog += eventFunction; break;
            case Axis.p2_leftTrigger: p2_leftTrigger += eventFunction; break;
            case Axis.p2_rightTrigger: p2_rightTrigger += eventFunction; break;

            //Error
            default:
                Debug.LogError("Tried to add an event to the input manager for an axis that does not exist."+
                               "You might want to use button instead of axis.");
                break;
        }
    }

    /// <summary>
    /// The name of every buttons
    /// </summary>
    public enum Button { 
        start, 
        p1_fireHookDown, 
        p2_fireHookDown, 
        p1_fireHookUp,
        p2_fireHookUp,
        p1_pushButton,
        p2_pushButton,
        p1_retractHooksButtonDown,
        p2_retractHooksButtonDown,
        p1_retractHooksButtonUp,
        p2_retractHooksButtonUp,
        p1_cutLinkWithChainButton,
        p2_cutLinkWithChainButton 
    }

    /// <summary>
    /// Link a function to an event
    /// </summary>
    /// <param name="button">The event to be used</param>
    /// <param name="eventFunction">The function to be call when the event is triggered</param>
    public void AddEvent(Button button, EventButton eventFunction) {
        switch (button) {
            case Button.start: _startButton += eventFunction; break;
            case Button.p1_fireHookDown: p1_fireHookButtonDown += eventFunction; break;
            case Button.p2_fireHookDown: p2_fireHookButtonDown += eventFunction; break;
            case Button.p1_fireHookUp: p1_fireHookButtonUp += eventFunction; break;
            case Button.p2_fireHookUp: p2_fireHookButtonUp += eventFunction; break;
            case Button.p1_pushButton: p1_pushButton += eventFunction; break;
            case Button.p2_pushButton: p2_pushButton += eventFunction; break;
            case Button.p1_retractHooksButtonDown: p1_retractHooksButtonDown += eventFunction; break;
            case Button.p2_retractHooksButtonDown: p2_retractHooksButtonDown += eventFunction; break;
            case Button.p1_retractHooksButtonUp: p1_retractHooksButtonUp += eventFunction; break;
            case Button.p2_retractHooksButtonUp: p2_retractHooksButtonUp += eventFunction; break;
            case Button.p1_cutLinkWithChainButton: p1_cutLinkWithChainButton += eventFunction; break;
            case Button.p2_cutLinkWithChainButton: p2_cutLinkWithChainButton += eventFunction; break;

            //Error
            default:
                Debug.LogError("Tried to add an event to the input manager for a button that does not exist." + 
                               "You might want to use axis instead of button.");
                break;
        }
    }
    #endregion

    #region Remove Event

    /// <summary>Resets every inputs to null. Used when  reloading a scene</summary>
    public void ResetInputs() {
        p1_leftAnalog = null;
        p1_rightAnalog = null;
        p1_leftTrigger = null;
        p1_rightTrigger = null;
        p2_leftAnalog = null;
        p2_rightAnalog = null;
        p2_leftTrigger = null;
        p2_rightTrigger = null;

        _startButton = null;
        p1_fireHookButtonDown = null;
        p2_fireHookButtonDown = null;
        p1_fireHookButtonUp = null;
        p2_fireHookButtonUp = null;
        p1_pushButton = null;
        p2_pushButton = null;
        p1_retractHooksButtonDown = null;
        p2_retractHooksButtonDown = null;
        p1_retractHooksButtonUp = null;
        p2_retractHooksButtonUp = null;
        p1_cutLinkWithChainButton = null;
        p2_cutLinkWithChainButton = null;
    }

    #endregion

    #region Utils

    private bool _isDisabled = false;
    public bool isDisabled {
        get {
            return _isDisabled;
        }
        set {
            _isDisabled = value;
            if (_isDisabled) Debug.LogWarning("Input have been disabled");
        }
    }

    #endregion

    void Update() {
        if (!_isDisabled) {
            List<AxisTuple> axii = new List<AxisTuple>() {
            new AxisTuple("P1 Left Analog", p1_leftAnalog, Input.GetAxis("P1_L_Horizontal"), Input.GetAxis("P1_L_Vertical")),
            new AxisTuple("P1 Right Analog", p1_rightAnalog, Input.GetAxis("P1_R_Horizontal"), Input.GetAxis("P1_R_Vertical")),
            new AxisTuple("P2 Left Trigger", p1_leftTrigger, Input.GetAxis("P1_L_Trigger"), 0),                                     //This axis has only 1 direction
            new AxisTuple("P2 Right Trigger", p1_rightTrigger, Input.GetAxis("P1_R_Trigger"), 0),                                   //This axis has only 1 direction
            new AxisTuple("P2 Left Analog", p2_leftAnalog, Input.GetAxis("P2_L_Horizontal"), Input.GetAxis("P2_L_Vertical")),
            new AxisTuple("P2 Right Analog", p2_rightAnalog, Input.GetAxis("P2_R_Horizontal"), Input.GetAxis("P2_R_Vertical")),
            new AxisTuple("P2 Left Trigger", p2_leftTrigger, Input.GetAxis("P2_L_Trigger"), 0),                                     //This axis has only 1 direction
            new AxisTuple("P2 Right Trigger", p2_rightTrigger, Input.GetAxis("P2_R_Trigger"), 0)                                    //This axis has only 1 direction
        };

            List<ButtonTuple> buttons = new List<ButtonTuple>() {
            new ButtonTuple("P1 Push", p1_pushButton, Input.GetButtonDown("P1_Push")),
            new ButtonTuple("P2 Push", p2_pushButton, Input.GetButtonDown("P2_Push")),
            new ButtonTuple("P1 Fire Hook Down", p1_fireHookButtonDown, Input.GetButtonDown("P1_FireHook")),
            new ButtonTuple("P2 Fire Hook Down", p2_fireHookButtonDown, Input.GetButtonDown("P2_FireHook")),
            new ButtonTuple("P1 Fire Hook Up", p1_fireHookButtonUp, Input.GetButtonUp("P1_FireHook")),
            new ButtonTuple("P2 Fire Hook Up", p2_fireHookButtonUp, Input.GetButtonUp("P2_FireHook")),
            new ButtonTuple("Start", _startButton, Input.GetButtonDown("Start")),
            new ButtonTuple("P1 Retract Hooks Down", p1_retractHooksButtonDown, Input.GetButtonDown("P1_Retract_Hooks")),
            new ButtonTuple("P2 Retract Hooks Down", p2_retractHooksButtonDown,Input.GetButtonDown("P2_Retract_Hooks")),
            new ButtonTuple("P1 Retract Hooks Up", p1_retractHooksButtonUp, Input.GetButtonUp("P1_Retract_Hooks")),
            new ButtonTuple("P2 Retract Hooks Up", p2_retractHooksButtonUp,Input.GetButtonUp("P2_Retract_Hooks")),
            new ButtonTuple("P1 Cut Link Chain", p1_cutLinkWithChainButton, Input.GetButtonDown("P1_Cut_Link_Chain")),
            new ButtonTuple("P2 Cut Link Chain", p2_cutLinkWithChainButton,Input.GetButtonDown("P2_Cut_Link_Chain"))
        };

            foreach (AxisTuple axis in axii) {
                if (axis.Item2 != null) {
                    axis.Item2.Invoke(axis.Item3, axis.Item4);
                }
            }

            foreach (ButtonTuple button in buttons) {
                if (button.Item1 != null) {
                    if (button.Item3 && button.Item2 != null) {
                        button.Item2.Invoke();
                    }
                }
            }
        }
    }
}
