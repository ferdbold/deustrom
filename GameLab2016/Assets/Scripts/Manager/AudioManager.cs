using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {

    public AudioMixer audioMixer { get; private set; }

    private GameObject AmbiantSounds;

    [Header("Audio Clip")]
    [SerializeField]
    private CharacterSound _characterSound;
    public CharacterSound characterSpecificSound { get { return _characterSound; } }

    [SerializeField]
    private IslandSound _islandSound;
    public IslandSound islandSpecificSound { get { return _islandSound; } }

    [SerializeField]
    private EnvironmentSound _environmentSound;
    public EnvironmentSound environmentSpecificSound { get { return _environmentSound; } }

    [SerializeField]
    private ChainSound _chainSound;
    public ChainSound chainSound { get { return _chainSound; } }

    [SerializeField]
    private MusicSound _music;

    private AudioSource _sourceNonMusic;

    private AudioSource _sourceMusic;

    private void Awake() {
        AudioSource[] audioSources = GetComponentsInChildren<AudioSource>();
        _sourceNonMusic = audioSources[0];
        _sourceMusic = audioSources[1];
        audioMixer = _sourceNonMusic.outputAudioMixerGroup.audioMixer;
        AmbiantSounds = transform.Find("Ambiant").gameObject;
    }

    public bool IsMenuPlaying() {
        return _sourceMusic.clip == _music.menuMusic;
    }

    public void PlayAudioClipMusic(AudioClip clip) {
        _sourceMusic.clip = clip;
        _sourceMusic.Play();
    }

    public void PlayAudioClipNonMusic(AudioClip clip) {
        _sourceNonMusic.clip = clip;
        _sourceNonMusic.Play();
    }

    public void ToggleAmbiantSounds(bool active) {
        AmbiantSounds.SetActive(active);
    }

    public void ToggleGameplaySounds(bool active) {
        Debug.Log(this.audioMixer);
        if (!active) {
            this.audioMixer.SetFloat("GameplayVolume", -80.0f);
        } else {
            this.audioMixer.SetFloat("GameplayVolume", 0.0f);
        }
    }

    public void ToggleLowMusicVolume(bool isLow) {
        if (isLow) this.audioMixer.SetFloat("MusicVolume", -7.5f);
        else this.audioMixer.SetFloat("MusicVolume", 0.0f);
    }
    


    public void PlayMusic(MusicSound.Choice music) {
        AudioClip choice = null;
        switch (music) {
            case MusicSound.Choice.menu:
                choice = _music.menuMusic;
                _sourceMusic.loop = true;
                break;

            case MusicSound.Choice.play:
                choice = _music.playMusic;
                _sourceMusic.loop = true;
                break;

            case MusicSound.Choice.endGame:
                choice = _music.endGameMusic;
                this.ToggleAmbiantSounds(false);
                _sourceMusic.loop = false;
                break;
        }
        _sourceMusic.clip = choice;
        _sourceMusic.Play();
    }
}

[System.Serializable]
public class SoundClass {
    protected AudioClip GetRandom(List<AudioClip> list) {
        int randIndex = Random.Range(0, list.Count);
        return list[randIndex];
    }
}

[System.Serializable]
public class CharacterSound : SoundClass {

    [SerializeField]
    private SobekSpecificSound _sobekSound;
    public SobekSpecificSound sobekSpecificSound { get { return _sobekSound; } }

    [SerializeField]
    private CthuluSpecificSound _cthuluSound;
    public CthuluSpecificSound cthuluSpecificSound { get { return _cthuluSound; } }
   
}

[System.Serializable]
public class SobekSpecificSound : SoundClass {

    public SobekSpecificSound() {
    }

    [SerializeField]
    [Tooltip("AudioClip played when player swims")]
    private List<AudioClip> _swim;
    public AudioClip swimSound { get { return GetRandom(_swim); } }

    [SerializeField]
    [Tooltip("Sound Played when 2 player bumps each other")]
    private List<AudioClip> _bump;
    public AudioClip bumpSound { get { return GetRandom(_bump); } }

    [SerializeField]
    private List<AudioClip> _push;
    public AudioClip pushSound { get { return GetRandom(_push); } }

    [SerializeField]
    private List<AudioClip> _grab;
    public AudioClip grabSound { get { return GetRandom(_grab); } }

    [SerializeField]
    private List<AudioClip> _playerChain_ThrowFirst;
    public AudioClip playerChain_ThrowFirstSound { get { return GetRandom(_playerChain_ThrowFirst); } }

    [SerializeField]
    private List<AudioClip> _playerChain_ThrowSecond;
    public AudioClip playerChain_ThrowSecondSound { get { return GetRandom(_playerChain_ThrowSecond); } }

    [SerializeField]
    private List<AudioClip> _playerDeath;
    public AudioClip playerDeath { get { return GetRandom(_playerDeath); } }

    [SerializeField]
    private List<AudioClip> _playerRetractChains;
    public AudioClip playerRetractChains { get { return GetRandom(_playerRetractChains); } }

    [SerializeField]
    private List<AudioClip> _hookThrowCooldownFail;
    public AudioClip hookThrowCooldownFail { get { return GetRandom(_hookThrowCooldownFail); } }

    [SerializeField]
    private List<AudioClip> _playerRespawn;
    public AudioClip playerRespawn { get { return GetRandom(_playerRespawn); } }

    [SerializeField]
    private List<AudioClip> _conversion;
    public AudioClip conversion { get { return GetRandom(_conversion); } }

    [SerializeField]
    private List<AudioClip> _conversionRiser;
    public AudioClip ConversionRiser { get { return GetRandom(_conversionRiser); } }

    [SerializeField]
    private List<AudioClip> _fastChant;
    public AudioClip fastChant { get { return GetRandom(_fastChant); } }

    [SerializeField]
    private List<AudioClip> _slowChant;
    public AudioClip slowChant { get { return GetRandom(_slowChant); } }


    [SerializeField]
    private List<AudioClip> _winInGame;
    public AudioClip winInGame { get { return GetRandom(_winInGame); } }

    [SerializeField]
    private List<AudioClip> _winScreen;
    public AudioClip winScreen { get { return GetRandom(_winScreen); } }

}

[System.Serializable]
public class CthuluSpecificSound : SoundClass {
    [SerializeField]
    [Tooltip("AudioClip played when player swims")]
    private List<AudioClip> _swim;
    public AudioClip swimSound { get { return GetRandom(_swim); } }

    [SerializeField]
    [Tooltip("Sound Played when 2 player bumps each other")]
    private List<AudioClip> _bump;
    public AudioClip bumpSound { get { return GetRandom(_bump); } }

    [SerializeField]
    private List<AudioClip> _push;
    public AudioClip pushSound { get { return GetRandom(_push); } }

    [SerializeField]
    private List<AudioClip> _grab;
    public AudioClip grabSound { get { return GetRandom(_grab); } }

    [SerializeField]
    private List<AudioClip> _playerChain_ThrowFirst;
    public AudioClip playerChain_ThrowFirstSound { get { return GetRandom(_playerChain_ThrowFirst); } }

    [SerializeField]
    private List<AudioClip> _playerChain_ThrowSecond;
    public AudioClip playerChain_ThrowSecondSound { get { return GetRandom(_playerChain_ThrowSecond); } }

    [SerializeField]
    private List<AudioClip> _playerDeath;
    public AudioClip playerDeath { get { return GetRandom(_playerDeath); } }

    [SerializeField]
    private List<AudioClip> _playerRetractChains;
    public AudioClip playerRetractChains { get { return GetRandom(_playerRetractChains); } }

    [SerializeField]
    private List<AudioClip> _hookThrowCooldownFail;
    public AudioClip hookThrowCooldownFail { get { return GetRandom(_hookThrowCooldownFail); } }

    [SerializeField]
    private List<AudioClip> _playerRespawn;
    public AudioClip playerRespawn { get { return GetRandom(_playerRespawn); } }

    [SerializeField]
    private List<AudioClip> _conversion;
    public AudioClip Conversion { get { return GetRandom(_conversion); } }

    [SerializeField]
    private List<AudioClip> _conversionRiser;
    public AudioClip ConversionRiser { get { return GetRandom(_conversionRiser); } }

    [SerializeField]
    private List<AudioClip> _fastChant;
    public AudioClip fastChant { get { return GetRandom(_fastChant); } }

    [SerializeField]
    private List<AudioClip> _slowChant;
    public AudioClip slowChant { get { return GetRandom(_slowChant); } }

    [SerializeField]
    private List<AudioClip> _winInGame;
    public AudioClip winInGame { get { return GetRandom(_winInGame); } }

    [SerializeField]
    private List<AudioClip> _winScreen;
    public AudioClip winScreen { get { return GetRandom(_winScreen); } }
}

[System.Serializable]
public class IslandSound : SoundClass {
    [SerializeField] [Tooltip("Sound played when something collides with an island")]
    private List<AudioClip> _collision;
    public AudioClip collisionSound { get { return GetRandom(_collision); } }


    [SerializeField] [Tooltip("Sound played when 2 island merge togeter")]
    private List<AudioClip> _merge;
    public AudioClip mergeSound { get { return GetRandom(_merge); } }

    [SerializeField] [Tooltip("Sound played when 2 island merge togeter")]
    private List<AudioClip> _destruction;
    public AudioClip destructionSound { get { return GetRandom(_destruction); } }

    [SerializeField] [Tooltip("Sound played when island is released from feeder")]
    private List<AudioClip> _feederRelease;
    public AudioClip feederRelease { get { return GetRandom(_feederRelease); } }
}

[System.Serializable]
public class EnvironmentSound : SoundClass {
    [SerializeField] [Tooltip("Sound played when island is destroyed in maelstrom")]
    private List<AudioClip> _maelstromDestruction;
    public AudioClip maelstromDestructionSound { get { return GetRandom(_maelstromDestruction); } }


}

[System.Serializable]
public class ChainSound : SoundClass {
    [SerializeField] [Tooltip("Sound played when a chain is destroyed")]
    private List<AudioClip> _chainDestruction;
    public AudioClip chainDestruction { get { return GetRandom(_chainDestruction); } }

    [SerializeField] [Tooltip("Sound played when a chain hits an island")]
    private List<AudioClip> _chainHit;
    public AudioClip chainHit { get { return GetRandom(_chainHit); } }
}

[System.Serializable]
public class MusicSound : SoundClass {
    public enum Choice { menu, play, endGame }

    public AudioClip menuMusic;
    public AudioClip playMusic;
    public AudioClip endGameMusic;
}
