using System.Collections;
using System.Collections.Generic;

using UnityEngine;


[CreateAssetMenu(fileName = "New Sound Parameters Asset", menuName = "Custom Assets/Sound Parameters Asset")]
public class SoundParams : ScriptableObject
{
    public static float DEFAULT_SOUND_VOLUME = 1.0f;


    [Header("Sound Sets")]
    public List<SoundSet> _SoundSets = new List<SoundSet>();

    [Header("Player")]
    public AudioClip _PlayerLandingSound;
    
    [Range(0, 1)]
    public float _PlayerLandingSoundVolume = SoundParams.DEFAULT_SOUND_VOLUME;



    private Dictionary<string, SoundSet> _SoundSetsByName;



    void OnEnable()
    {
        _SoundSetsByName = new Dictionary<string, SoundSet>();


        foreach (SoundSet set in _SoundSets) 
        {
            _SoundSetsByName.Add(set.name, set);
        }
    }



    public SoundSet GetSoundSet(string name)
    {
        return _SoundSetsByName[name];
    }

}
