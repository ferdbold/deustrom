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
	private EventAxis p1_leftAnalog;
	private EventAxis p1_rightAnalog;
    private EventAxis p2_leftAnalog;
    private EventAxis p2_rightAnalog;

    private EventButton _startButton;
	private EventButton p1_fireHookButton;
    private EventButton p2_fireHookButton;

    #region Add Event
    /// <summary>
    /// The name of every axis
    /// </summary>
    public enum Axis { p1_leftAnalog, p1_rightAnalog, p2_leftAnalog, p2_rightAnalog }
	/// <summary>
	/// Link a function to an event
	/// </summary>
	/// <param name="input">The event to be used</param>
	/// <param name="eventFunction">The function to be call when the event is triggered</param>
	public void AddEvent(Axis axis, EventAxis eventFunction) {
		switch (axis) {
			case Axis.p1_leftAnalog: p1_leftAnalog += eventFunction; break;
			case Axis.p1_rightAnalog: p1_rightAnalog += eventFunction; break;
            case Axis.p2_leftAnalog: p2_leftAnalog += eventFunction; break;
            case Axis.p2_rightAnalog: p2_rightAnalog += eventFunction; break;

            //Error
            default:
				Debug.LogError("Tried to add an event to the input manager for a button that does not exist."+
							   "You might want to axis instead of button.");
				break;
		}
	}

	/// <summary>
	/// The name of every buttons
	/// </summary>
	public enum Button { start, p1_fireHook, p2_fireHook }
	/// <summary>
	/// Link a function to an event
	/// </summary>
	/// <param name="button">The event to be used</param>
	/// <param name="eventFunction">The function to be call when the event is triggered</param>
	public void AddEvent(Button button, EventButton eventFunction) {
		switch (button) {
			case Button.start: _startButton += eventFunction; break;
			case Button.p1_fireHook: p1_fireHookButton += eventFunction; break;
            case Button.p2_fireHook: p2_fireHookButton += eventFunction; break;

            //Error
            default:
				Debug.LogError("Tried to add an event to the input manager for a button that does not exist." + 
							   "You might want to axis instead of button.");
				break;
		}
	}
	#endregion

	void Update() {
		List<AxisTuple> axii = new List<AxisTuple>() {
			new AxisTuple("P1 Left Analog", p1_leftAnalog, Input.GetAxis("P1_L_Horizontal"), Input.GetAxis("P1_L_Vertical")),
			new AxisTuple("P1 Right Analog", p1_rightAnalog, Input.GetAxis("P1_R_Horizontal"), Input.GetAxis("P1_R_Vertical")),
            new AxisTuple("P2 Left Analog", p2_leftAnalog, Input.GetAxis("P2_L_Horizontal"), Input.GetAxis("P2_L_Vertical")),
            new AxisTuple("P2 Right Analog", p2_rightAnalog, Input.GetAxis("P2_R_Horizontal"), Input.GetAxis("P2_R_Vertical"))
        };
		List<ButtonTuple> buttons = new List<ButtonTuple>() {
			new ButtonTuple("P1 Fire Hook", p1_fireHookButton, Input.GetButtonDown("P1_FireHook")),
            new ButtonTuple("P2 Fire Hook", p2_fireHookButton, Input.GetButtonDown("P2_FireHook")),
            new ButtonTuple("Start", _startButton, Input.GetButtonDown("Start"))
		};

		foreach(AxisTuple axis in axii) {
			if (axis.Item2 != null) {
				axis.Item2.Invoke(axis.Item3, axis.Item4);
			}
		}

		foreach (ButtonTuple button in buttons) {
			if (button.Item1 != null) {
				if (button.Item3) {
                    button.Item2.Invoke();
				}
			}
		}
	}
}
