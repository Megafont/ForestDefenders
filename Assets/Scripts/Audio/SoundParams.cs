using System.Collections;
using System.Collections.Generic;

using UnityEngine;


[CreateAssetMenu(fileName = "New Sound Parameters Asset", menuName = "My Assets/Sound Parameters Asset")]
public class SoundParams : ScriptableObject
{
    public static float DEFAULT_SOUND_VOLUME = 1.0f;


    [Header("Sound Sets")]
    public List<SoundSet> SoundSets = new List<SoundSet>();

    [Header("Player")]
    public AudioClip PlayerLandingSound;
    [Range(0, 1)] public float PlayerLandingSoundVolume = SoundParams.DEFAULT_SOUND_VOLUME;


    private Dictionary<string, SoundSet> _SoundSetsByName;


    void OnEnable()
    {
        _SoundSetsByName = new Dictionary<string, SoundSet>();


        foreach (SoundSet set in SoundSets) 
        {
            _SoundSetsByName.Add(set.name, set);
        }
    }



    public SoundSet GetSoundSet(string name)
    {
        return _SoundSetsByName[name];
    }

}
