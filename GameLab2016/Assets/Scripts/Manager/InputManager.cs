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
	private EventAxis _leftAnalog;
	private EventAxis _rightAnalog;

	private EventButton _startButton;
	private EventButton _fireHookButton;

	#region Add Event
	/// <summary>
	/// The name of every axis
	/// </summary>
	public enum Axis { leftAnalog, rightAnalog }
	/// <summary>
	/// Link a function to an event
	/// </summary>
	/// <param name="input">The event to be used</param>
	/// <param name="eventFunction">The function to be call when the event is triggered</param>
	public void AddEvent(Axis axis, EventAxis eventFunction) {
		switch (axis) {
			case Axis.leftAnalog: _leftAnalog += eventFunction; break;
			case Axis.rightAnalog: _rightAnalog += eventFunction; break;

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
	public enum Button { start, fireHook }
	/// <summary>
	/// Link a function to an event
	/// </summary>
	/// <param name="button">The event to be used</param>
	/// <param name="eventFunction">The function to be call when the event is triggered</param>
	public void AddEvent(Button button, EventButton eventFunction) {
		switch (button) {
			case Button.start: _startButton += eventFunction; break;
			case Button.fireHook: _fireHookButton += eventFunction; break;

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
			new AxisTuple("Left Analog", _leftAnalog, Input.GetAxis("L_Horizontal"), Input.GetAxis("L_Vertical")),
			new AxisTuple("Right Analog", _rightAnalog, Input.GetAxis("R_Horizontal"), Input.GetAxis("R_Vertical"))
		};
		List<ButtonTuple> buttons = new List<ButtonTuple>() {
			new ButtonTuple("Fire Hook", _fireHookButton, Input.GetButtonDown("Fire Hook")),
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
