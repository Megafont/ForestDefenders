using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class SoundSetPlayer : MonoBehaviour
{
    public SoundSet _SoundSet;


    private int _NextSoundIndex;


    public void PlaySound()
    {
        if (_SoundSet.SoundsList == null)
        {
            Debug.LogError($"Could not play a sound, because the sound set is null!");
            return;
        }

        if (_SoundSet.SoundsList.Count == 0)
        {
            Debug.LogError($"Could not play a sound, because the sound set \"SoundSet.Name\" is empty!");
            return;
        }


        var index = Random.Range(0, _SoundSet.SoundsList.Count);
        AudioSource.PlayClipAtPoint(_SoundSet.SoundsList[index], transform.position, _SoundSet.Volume);
    }

}
