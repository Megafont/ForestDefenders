using System.Collections;
using System.Collections.Generic;

using UnityEngine;


[CreateAssetMenu(fileName = "New Music Parameters Asset", menuName = "Custom Assets/Music Parameters Asset")]
public class MusicParams : ScriptableObject
{
    const float DEFAULT_MUSIC_VOLUME = 0.5f;


    public AudioClip PlayerBuildPhaseMusic;
    [Range(0, 1)] public float PlayerBuildPhaseMusicVolume = DEFAULT_MUSIC_VOLUME;

    public AudioClip MonsterAttackPhaseMusic;
    [Range(0, 1)] public float MonsterAttackPhaseMusicVolume = DEFAULT_MUSIC_VOLUME;
}
