using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class SoundSetPlayer : MonoBehaviour
{
    [SerializeField]
    private SoundSet _SoundSet;

    private AudioSource _AudioSource;



    private void Awake()
    {
        _AudioSource = this.AddComponent<AudioSource>();        
    }

    private void Start()
    {
        if (_SoundSet)
            _AudioSource.volume = _SoundSet.Volume;
    }

    public void PlaySound(int index)
    {
        if (_SoundSet == null)
        {
            Debug.LogError($"Could not play a sound, because the sound set is null!");
            return;
        }

        if (_SoundSet.SoundsList == null)
        {
            Debug.LogError($"Could not play a sound, because the sound set \"SoundSet.Name\" contains no sounds!");
            return;
        }

        if (_SoundSet.SoundsList.Count == 0)
        {
            Debug.LogError($"Could not play a sound, because the sound set \"SoundSet.Name\" is empty!");
            return;
        }


        _AudioSource.spatialize = _SoundSet.PlayAs3DSound;
        _AudioSource.spatialBlend = _SoundSet.SpatialBlend;
        _AudioSource.volume = _SoundSet.Volume;
        _AudioSource.PlayOneShot(_SoundSet.SoundsList[index]);
    }

    public void PlayRandomSound()
    {
        if (_SoundSet == null)
        {
            Debug.LogError($"Could not play a sound, because the sound set is null!");
            return;
        }

        if (_SoundSet.SoundsList == null)
        {
            Debug.LogError($"Could not play a sound, because the sound set \"SoundSet.Name\" contains no sounds!");
            return;
        }

        if (_SoundSet.SoundsList.Count == 0)
        {
            Debug.LogError($"Could not play a sound, because the sound set \"SoundSet.Name\" is empty!");
            return;
        }


        var index = Random.Range(0, _SoundSet.SoundsList.Count);

        //AudioSource.PlayClipAtPoint(_SoundSet.SoundsList[index], transform.position, _SoundSet.Volume);

        if (_SoundSet.PlayAs3DSound)
            AudioSource.PlayClipAtPoint(_SoundSet.SoundsList[index], transform.position, _SoundSet.Volume);
        else
            _AudioSource.PlayOneShot(_SoundSet.SoundsList[index], _SoundSet.Volume);
    }



    public SoundSet SoundSet
    {
        get { return _SoundSet; }
        set
        {
            if (!value)
                Debug.LogWarning("The passed in SoundSet is null!");
            
            _SoundSet = value;
        }
    }

}
