using UnityEngine;
using System;

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
		try {
			/* Called function linked to */
			//Axis
			_leftAnalog.Invoke(Input.GetAxis("L_Horizontal"), Input.GetAxis("L_Vertical"));
			_rightAnalog.Invoke(Input.GetAxis("R_Horizontal"), Input.GetAxis("R_Horizontal"));

			//Button
			if (Input.GetButtonDown("Fire Hook")) _fireHookButton.Invoke();
			if (Input.GetButtonDown("Start")) _startButton.Invoke();

		}
		catch (NullReferenceException e) {
			Debug.LogWarning("One or more inputs are not linked to any event. " + e.Message);
		}
	}
}
