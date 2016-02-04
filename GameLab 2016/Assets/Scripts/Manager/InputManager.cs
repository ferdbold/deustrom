using UnityEngine;
using System;

public class InputManager : MonoBehaviour {

	/// <summary>
	/// Event structure
	/// </summary>
	public delegate void EventAxis(params float[] a);
	public delegate void EventButton();

	//Possible Event
	public EventAxis leftAnalog { get; private set; }

	#region Add Event
	/// <summary>
	/// The name of every axis
	/// </summary>
	public enum Axis { leftAnalog }
	/// <summary>
	/// Link a function to an event
	/// </summary>
	/// <param name="input">The event to be used</param>
	/// <param name="eventFunction">The function to be call when the event is triggered</param>
	public void AddEvent(Axis axis, EventAxis eventFunction) {
		switch (axis) {
			case Axis.leftAnalog: leftAnalog += eventFunction; break;

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
	public enum Button { }
	/// <summary>
	/// Link a function to an event
	/// </summary>
	/// <param name="button">The event to be used</param>
	/// <param name="eventFunction">The function to be call when the event is triggered</param>
	public void AddEvent(Button button, EventButton eventFunction) {
		switch (button) {

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
			//Called function linked to 
			leftAnalog.Invoke(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

		}
		catch (NullReferenceException e) {
			Debug.LogWarning("One or more inputs are not linked to any event. " + e.Message);
		}
	}
}
