using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

namespace Simoncouche.Controller {

    public enum PlayerSounds { PlayerBump, PlayerPush, PlayerGrab, PlayerChainFirst, PlayerChainSecond, PlayerDeath, PlayerRetractChains};

    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAudio : MonoBehaviour {
        #region PrivateVariables
        [Header("Swimming Sound Properties")]
        /// <summary>
        /// This is the maximum pitch of our sound when he goes fast
        /// </summary>
        [Tooltip("maximum pitch of our sound when he goes fast")]
        [SerializeField]
        private float maxPitch = 1.1f;
        /// <summary>
        /// This is the minimum pitch of our sound when he goes slow
        /// </summary>
        [Tooltip("minimum pitch of our sound when he goes slow")]
        [SerializeField]
        private float minPitch = 1.0f;
        /// <summary>
        /// Fade out speed multipler which is used to decreased the volume multiplied with Time.deltaTime
        /// </summary>
        [Tooltip("Fade out speed multipler which is used to decreased the volume gradually until 0.  When 0, we pause the sound being played.")]
        [SerializeField]
        private float volumeFadeOutSpeed = 1.0f;
        /// <summary>
        /// Time in seconds until we stop the sound (not pause !) 
        /// </summary>
        [Tooltip("After this amounte of time, we stop the sound (after being paused precedently).  It's in order to start over completly the sounds when we stop and go quickly with the character")]
        private float timeUntileSoundStops = 0.5f;

        /// <summary>
        /// Ref to the player controller in order to his inputs using getleftanalog functions
        /// </summary>
        private PlayerController playerController;
        private float _currentPitchValue = 1.0f;
        private float _volumeSwimming;
        private AudioSource _swimmingAudioSource;
        private AudioSource _actionAudioSource;

        private bool _isCoroutineRunning;
        private const float _axisMaxValue = 1.0f;
        #endregion

        void Awake() {
            playerController = GetComponent<PlayerController>();
            _swimmingAudioSource = GetComponent<AudioSource>();
            //Create and setup second audiosource exactly like swimming audio source
            _actionAudioSource = gameObject.AddComponent<AudioSource>();
            _actionAudioSource.outputAudioMixerGroup = _swimmingAudioSource.outputAudioMixerGroup;
        }

        void Start() {
            _actionAudioSource.clip = GameManager.audioManager.characterSpecificSound.swimSound;
            _volumeSwimming = _swimmingAudioSource.volume;
        }

        void Update() {
            //We lerp the current added velocity by the player on the character to get a pitch higher or lower depending on velocity
            //magnitude divided by Time.fixedDeltatime is done cause 
            _currentPitchValue = Mathf.Lerp(minPitch, maxPitch, GetMovementValue() / _axisMaxValue);

            _swimmingAudioSource.pitch = _currentPitchValue;

            if (Mathf.Abs(playerController.GetLeftAnalogHorizontal()) > 0.0f
                || Mathf.Abs(playerController.GetLeftAnalogVertical()) > 0.0f) {
                //We reset the volume to the volume specified in the inspector
                _swimmingAudioSource.volume = _volumeSwimming;
                //We stop the coroutine and specify it in a boolean
                StopCoroutine("TimerUntilSoundStop");
                _isCoroutineRunning = false;

                if (!_swimmingAudioSource.isPlaying) _swimmingAudioSource.Play();
            } else {
                FadeAudioSource();
            }
        }

        /// <summary> Plays a sound on the action audio source </summary>
        /// <param name="ac"> audioclip to play </param>
        public void PlaySound(PlayerSounds ac) {
            switch(ac) {
                case PlayerSounds.PlayerBump :
                    _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.bumpSound);
                    break;
                case PlayerSounds.PlayerPush :
                    _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.pushSound);
                    break;
                case PlayerSounds.PlayerGrab :
                    _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.grabSound);
                    break;
                case PlayerSounds.PlayerChainFirst:
                    _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.playerChain_ThrowFirstSound);
                    break;
                case PlayerSounds.PlayerChainSecond:
                    _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.playerChain_ThrowSecondSound);
                    break;
                case PlayerSounds.PlayerDeath:
                    _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.playerDeath);
                    break;
                case PlayerSounds.PlayerRetractChains:
                    _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.playerRetractChains);
                    break;
                default:
                    Debug.LogWarning("Sound " + ac.ToString() + " not yet implemented.");
                    break;
            }
        }

        /// <summary>
        /// We get the hypothenuse value of our 2 vectors 
        /// </summary>
        /// <returns></returns>
        private float GetMovementValue() {
            float x = Mathf.Pow(playerController.GetCurrentPlayerMovementInputs().x, 2.0f);
            float y = Mathf.Pow(playerController.GetCurrentPlayerMovementInputs().y, 2.0f);
            float movementValue = Mathf.Clamp(Mathf.Sqrt(x + y), 0.0f, _axisMaxValue);
            return movementValue;
        }

        /// <summary>
        /// We fade out the sound being played gradually until the volume is almost at 0.  Then we pause the sound and start a coroutine TimeUntilSoundStop
        /// which wait a certain amount of time until we stop the sound.  It's in order the sound to get played back from the beginning when the player is 
        /// doing stop'n goes.
        /// </summary>
        private void FadeAudioSource() {
            _swimmingAudioSource.volume -= Time.deltaTime * volumeFadeOutSpeed;
            if (_swimmingAudioSource.volume < 0.05f) {
                _swimmingAudioSource.Pause();
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
            _swimmingAudioSource.Stop();
            _isCoroutineRunning = false;
        }
    }
}