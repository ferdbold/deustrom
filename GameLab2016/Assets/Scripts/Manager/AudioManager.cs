using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

	[Header("Audio Clip")]
	[SerializeField]
	private CharacterSound _characterSound;
	public CharacterSound characterSpecificSound { get { return _characterSound; } }

	[SerializeField]
	private SobekSpecificSound _sobekSound;
	public SobekSpecificSound sobekSpecificSound { get { return _sobekSound; } }

	[SerializeField]
	private CthuluSpecificSound _cthuluSound;
	public CthuluSpecificSound cthuluSpecificSound { get { return _cthuluSound; } }

	[SerializeField]
	private IslandSound _islandSound;
	public IslandSound islandSpecificSound { get { return _islandSound; } }

	[SerializeField]
	private EnvironmentSound _environmentSound;
	public EnvironmentSound environmentSpecificSound { get { return _environmentSound; } }

	//TODO replace certain sound with list and create function to random through list
	//The get value of attribute will then return a random clip
}

[System.Serializable]
public class CharacterSound {
	[SerializeField] [Tooltip("AudioClip played when player swims")]
	private AudioClip _swim;
	public AudioClip swimSound { get { return _swim; } }

	[SerializeField] [Tooltip("Sound Played when 2 player bumps each other")]
	private AudioClip _bump;
	public AudioClip bumpSound { get { return _bump; } }

	[SerializeField]
	private AudioClip _push;
	public AudioClip pushSound { get { return _push; } }

	[SerializeField]
	private AudioClip _grab;
	public AudioClip grabSound { get { return _grab; } }

	[SerializeField]
	private AudioClip _playerChain_ThrowFirst;
	public AudioClip playerChain_ThrowFirstSound { get { return _playerChain_ThrowFirst; } }

	[SerializeField]
	private AudioClip _playerChain_ThrowSecond;
	public AudioClip playerChain_ThrowSecondSound { get { return _playerChain_ThrowSecond; } }
}

[System.Serializable]
public class SobekSpecificSound {
	
}

[System.Serializable]
public class CthuluSpecificSound {

}

[System.Serializable]
public class IslandSound {
	[SerializeField] [Tooltip("Sound played when something collides with an island")]
	private AudioClip _collision;
	public AudioClip collisionSound { get { return _collision; } }


	[SerializeField] [Tooltip("Sound played when 2 island merge togeter")]
	private AudioClip _merge;
	public AudioClip mergeSound { get { return _merge; } }

	[SerializeField] [Tooltip("Sound played when 2 island merge togeter")]
	private AudioClip _disassemble;
	public AudioClip disassembleSound { get { return _disassemble; } }
}

[System.Serializable]
public class EnvironmentSound {
	[SerializeField] [Tooltip("Sound played when island is destroyed in maelstrom")]
	private AudioClip _maelstromDestruction;
	public AudioClip maelstromDestructionSound { get { return _maelstromDestruction; } }
}
