using System.Collections;
using System.Collections.Generic;

using UnityEngine;


[CreateAssetMenu(fileName = "New Sound Set Asset", menuName = "My Assets/Sound Set Asset")]
public class SoundSet : ScriptableObject
{    
    public List<AudioClip> SoundsList;
    [Range(0, 1)] public float Volume = SoundParams.DEFAULT_SOUND_VOLUME;
}

