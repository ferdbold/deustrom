using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

namespace Simoncouche.Controller {

    public enum PlayerSounds { PlayerBump, PlayerPush, PlayerGrab, PlayerChainFirst, PlayerChainSecond, PlayerDeath, PlayerRetractChains, PlayerCooldown, PlayerRespawn};

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

        [Tooltip("maximum volume of our sound when he goes fast")]
        [SerializeField]
        private float maxVolume = 1.0f;

        [Tooltip("minimum volume of our sound when he goes slow")]
        [SerializeField]
        private float minVolume = 0.6f;
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
        private bool _isSobek;

        private float _swimSoundGap = 0.50f;
        private float _currentSwimSoundGap = 0f;
        private float _currentSwimSoundTime = 0f;
        private Vector2 _swimSoundGapVariation = new Vector2(-0.05f, 0.05f);
        
        #endregion

        void Awake() {
            playerController = GetComponent<PlayerController>();
            _swimmingAudioSource = GetComponent<AudioSource>();
            //Create and setup second audiosource exactly like swimming audio source
            _actionAudioSource = gameObject.AddComponent<AudioSource>();
            _actionAudioSource.outputAudioMixerGroup = _swimmingAudioSource.outputAudioMixerGroup;
            _isSobek = gameObject.name == "Sobek" ? true : false;
        }

        void Start() {
            //if (_isSobek) _actionAudioSource.clip = GameManager.audioManager.characterSpecificSound.sobekSpecificSound.swimSound;
            //else _actionAudioSource.clip = GameManager.audioManager.characterSpecificSound.cthuluSpecificSound.swimSound;
            _volumeSwimming = _swimmingAudioSource.volume;
        }

        void Update() {
            //We lerp the current added velocity by the player on the character to get a pitch higher or lower depending on velocity
            //magnitude divided by Time.fixedDeltatime is done cause 
            //TODO : This lerp doesn't seem to be working as intended : 
            //_currentPitchValue = Mathf.Lerp(minPitch, maxPitch, GetMovementValue() / _axisMaxValue);
            //_swimmingAudioSource.pitch = _currentPitchValue;
            _swimmingAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, playerController.GetRatioOfMaxSpeed());
            _swimmingAudioSource.volume = Mathf.Lerp(minVolume, maxVolume, playerController.GetRatioOfMaxSpeed());
            Debug.Log("step : " + playerController.GetRatioOfMaxSpeed()  + "  pitch : " + _swimmingAudioSource.pitch +  " volume : " + _swimmingAudioSource.volume);


            //Increase time for next swim sound
            _currentSwimSoundTime += Time.deltaTime + (Time.deltaTime * playerController.GetRatioOfMaxSpeed());
            //check if the player is moving
            if (Mathf.Abs(playerController.GetLeftAnalogHorizontal()) > 0.0f || Mathf.Abs(playerController.GetLeftAnalogVertical()) > 0.0f) {
                if (!_swimmingAudioSource.isPlaying) _swimmingAudioSource.Play();
                //Play Random Swim Sounds           
                if (_currentSwimSoundTime > _currentSwimSoundGap) {
                    _currentSwimSoundTime = 0;
                    _currentSwimSoundGap = _swimSoundGap + Random.Range(_swimSoundGapVariation.x, _swimSoundGapVariation.y);
                    _swimmingAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.sobekSpecificSound.swimSound);
                }
            }
        }

        /// <summary> Plays a sound on the action audio source </summary>
        /// <param name="ac"> audioclip to play </param>
        public void PlaySound(PlayerSounds ac) {
            if(_isSobek) {
                switch (ac) {
                    case PlayerSounds.PlayerBump:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.sobekSpecificSound.bumpSound);
                        break;
                    case PlayerSounds.PlayerPush:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.sobekSpecificSound.pushSound);
                        break;
                    case PlayerSounds.PlayerGrab:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.sobekSpecificSound.grabSound);
                        break;
                    case PlayerSounds.PlayerChainFirst:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.sobekSpecificSound.playerChain_ThrowFirstSound);
                        break;
                    case PlayerSounds.PlayerChainSecond:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.sobekSpecificSound.playerChain_ThrowSecondSound);
                        break;
                    case PlayerSounds.PlayerDeath:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.sobekSpecificSound.playerDeath);
                        break;
                    case PlayerSounds.PlayerRetractChains:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.sobekSpecificSound.playerRetractChains);
                        break;
                    case PlayerSounds.PlayerCooldown:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.sobekSpecificSound.hookThrowCooldownFail);
                        break;
                    case PlayerSounds.PlayerRespawn:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.sobekSpecificSound.playerRespawn);
                        break;
                    default:
                        Debug.LogWarning("Sound " + ac.ToString() + " not yet implemented.");
                        break;
                }
            } else {
                switch (ac) {
                    case PlayerSounds.PlayerBump:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.cthuluSpecificSound.bumpSound);
                        break;
                    case PlayerSounds.PlayerPush:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.cthuluSpecificSound.pushSound);
                        break;
                    case PlayerSounds.PlayerGrab:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.cthuluSpecificSound.grabSound);
                        break;
                    case PlayerSounds.PlayerChainFirst:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.cthuluSpecificSound.playerChain_ThrowFirstSound);
                        break;
                    case PlayerSounds.PlayerChainSecond:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.cthuluSpecificSound.playerChain_ThrowSecondSound);
                        break;
                    case PlayerSounds.PlayerDeath:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.cthuluSpecificSound.playerDeath);
                        break;
                    case PlayerSounds.PlayerRetractChains:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.cthuluSpecificSound.playerRetractChains);
                        break;
                    case PlayerSounds.PlayerCooldown:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.cthuluSpecificSound.hookThrowCooldownFail);
                        break;
                    case PlayerSounds.PlayerRespawn:
                        _actionAudioSource.PlayOneShot(GameManager.audioManager.characterSpecificSound.cthuluSpecificSound.playerRespawn);
                        break;
                    default:
                        Debug.LogWarning("Sound " + ac.ToString() + " not yet implemented.");
                        break;
                }
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

   
    }
}