using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class SoundSetPlayer : MonoBehaviour
{
    public SoundSet SoundSet;


    private int _NextSoundIndex;


    public void PlaySound()
    {
        if (SoundSet.SoundsList == null)
        {
            Debug.LogError($"Could not play a sound, because the sound set is null!");
            return;
        }

        if (SoundSet.SoundsList.Count == 0)
        {
            Debug.LogError($"Could not play a sound, because the sound set \"SoundSet.Name\" is empty!");
            return;
        }


        var index = Random.Range(0, SoundSet.SoundsList.Count);
        AudioSource.PlayClipAtPoint(SoundSet.SoundsList[index], transform.position, SoundSet.Volume);
    }

}
