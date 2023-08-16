using System.Collections;
using System.Collections.Generic;

using UnityEngine;


[CreateAssetMenu(fileName = "New Sound Parameters Asset", menuName = "Custom Assets/Sound Parameters Asset")]
public class SoundParams : ScriptableObject
{
    public static float DEFAULT_SOUND_VOLUME = 1.0f;


    [Header("Sound Sets")]
    public List<SoundSet> SoundSets = new List<SoundSet>();


    [Header("Player")]
    public AudioClip PlayerLandingSound;
    
    [Range(0, 1)]
    public float PlayerLandingSoundVolume = DEFAULT_SOUND_VOLUME;
    [Tooltip("Whether or not the player landing sound is played at a 3D position in the world. Disabling this is helpful if the sound is too quiet at max volume.")]
    public bool PlayPlayerLandingSoundAs3DSound = true;
    [Tooltip("The spatial blend amount to use betwen 3D sound and 2D when PlayPlayerLandingSoundAs3DSound is enabled.")]
    public float PlayerLandingSoundSpatialBlend = 1.0f;

    [Space(10)]
    public AudioClip LevelUpSound;


    [Header("Birds")]
    [Range(0, 1)]
    public float BirdSoundsVolume = 1.0f;
    [Range(1, 5)]
    public float BirdSoundsFadeTime = 2.0f;



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
