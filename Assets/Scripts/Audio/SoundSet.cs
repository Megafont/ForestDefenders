using System.Collections;
using System.Collections.Generic;

using UnityEngine;


[CreateAssetMenu(fileName = "New Sound Set Asset", menuName = "Custom Assets/Sound Set Asset")]
public class SoundSet : ScriptableObject
{
    public List<AudioClip> SoundsList;

    [Range(0, 1)]
    public float Volume = SoundParams.DEFAULT_SOUND_VOLUME;

    [Tooltip("Whether or not the sound is played at a 3D position in the world. Disabling this is helpful if the sound is too quiet at max volume.")]
    public bool PlayAs3DSound = true;

    [Tooltip("The spatial blend amount to use betwen 3D sound and 2D when PlayAs3DSound is enabled.")]
    public float SpatialBlend = 1.0f;
}

