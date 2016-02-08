using UnityEngine;
using System.Collections;
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PlayerController))]

public class PlayerAudio : MonoBehaviour {
	#region PublicVariables
	[Header("Swimming Sound Properties")]
	/// <summary>
	/// This is the maximum pitch of our sound when he goes fast
	/// </summary>
	[Tooltip("maximum pitch of our sound when he goes fast")]
	public float maxPitch = 1.1f;
	/// <summary>
	/// This is the minimum pitch of our sound when he goes slow
	/// </summary>
	[Tooltip("minimum pitch of our sound when he goes slow")]
	public float minPitch = 1.0f;
	/// <summary>
	/// Fade out speed multipler which is used to decreased the volume multiplied with Time.deltaTime
	/// </summary>
	[Tooltip("Fade out speed multipler which is used to decreased the volume gradually until 0.  When 0, we pause the sound being played.")]
	public float volumeFadeOutSpeed = 1.0f;
	/// <summary>
	/// Time in seconds until we stop the sound (not pause !) 
	/// </summary>
	[Tooltip("After this amounte of time, we stop the sound (after being paused precedently).  It's in order to start over completly the sounds when we stop and go quickly with the character")]
	public float timeUntileSoundStops = 0.5f;
	#endregion

	#region PrivateVariables
	/// <summary>
	/// Ref to the player controller in order to his inputs using getleftanalog functions
	/// </summary>
	private PlayerController _pRef;
	private float _currentPitchValue = 1.0f;
	private float _volumeSwimming;
	private AudioSource _playerAudioSource;
	private bool _isCoroutineRunning;
	#endregion


	void Awake() {
		_playerAudioSource = GetComponent<AudioSource>();
		_pRef = GetComponent<PlayerController>();
	}

	void Start() {
		_volumeSwimming = _playerAudioSource.volume;
	}
	
		
	// Update is called once per frame
	void Update () {
		if (Mathf.Abs(_pRef.GetLeftAnalogHorizontal()) > 0.0f 
			|| Mathf.Abs(_pRef.GetLeftAnalogVertical()) > 0.0f) {
			//We reset the volume to the volume specified in the inspector
			_playerAudioSource.volume = _volumeSwimming;
			//We stop the coroutine and specify it in a boolean
			StopCoroutine("TimerUntilSoundStop");
			_isCoroutineRunning = false;

			//We lerp the current added velocity by the player on the character to get a pitch higher or lower depending on velocity
			//magnitude divided by Time.fixedDeltatime is done cause 
			_currentPitchValue = Mathf.Lerp(minPitch, maxPitch, (_pRef.GetCurrentAddedVelocity().magnitude/Time.fixedDeltaTime)/_pRef.maximumVelocity);

			_playerAudioSource.pitch = _currentPitchValue;

			if (!_playerAudioSource.isPlaying) _playerAudioSource.Play();
		}else {
			FadeAudioSource();
		}
		
	}

	/// <summary>
	/// We fade out the sound being played gradually until the volume is almost at 0.  Then we pause the sound and start a coroutine TimeUntilSoundStop
	/// which wait a certain amount of time until we stop the sound.  It's in order the sound to get played back from the beginning when the player is 
	/// doing stop'n goes.
	/// </summary>
	private void FadeAudioSource() {
		_playerAudioSource.volume -= Time.deltaTime * volumeFadeOutSpeed;
		if (_playerAudioSource.volume < 0.05f) {
			_playerAudioSource.Pause();
			if (_isCoroutineRunning) {
				TimerUntilSoundStop(timeUntileSoundStops);
			}
		}
	}

	/// <summary>
	/// Coroutine which wait a certain amount of time and then stopl our playerAudioSource
	/// </summary>
	/// <param name="time"></param>
	/// <returns></returns>
	private IEnumerator TimerUntilSoundStop(float time) {
		_isCoroutineRunning = true;
		yield return new WaitForSeconds(time);
		_playerAudioSource.Stop();
		_isCoroutineRunning = false;
	}
}
