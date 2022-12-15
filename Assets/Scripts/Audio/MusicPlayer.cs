using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


public class MusicPlayer : MonoBehaviour
{
    public AudioClip DefaultMusic;
    
    [Range(0f, 10f)]
    public float DefaultFadeTime = 2.5f;

    [Range(0.0f, 1.0f)]
    public float DefaultVolume = 0.1f;


    public static MusicPlayer Instance;


    private AudioSource _CurrentTrack;
    private AudioSource _PreviousTrack;

    private float _FadeTime;
    private float _Volume;

    Coroutine _FadeCoroutine;

    


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }


        _CurrentTrack = gameObject.AddComponent<AudioSource>();
        _PreviousTrack = gameObject.AddComponent<AudioSource>();

        _Volume = DefaultVolume;

        _CurrentTrack.loop = true;
        _CurrentTrack.volume = _Volume;
        _PreviousTrack.loop = true;
        _PreviousTrack.volume = _Volume;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (DefaultMusic)
            FadeToTrack(DefaultMusic);
    }



    public void FadeToTrack(AudioClip newClip, bool crossFade = true, float newClipVolume = -1.0f, float fadeTime = -1.0f)
    {
        if (newClip == null)
        {
            Debug.LogWarning($"Cannot crossfade because the passed in audio clip is null!");
            return;       
        }

        // If the new clip is already playing, then simply return.
        if (newClip == _CurrentTrack.clip)        
            return;


        SwapAudioSources();
        _CurrentTrack.clip = newClip;

        _FadeTime = ValidateFadeTime(fadeTime);
        _Volume = ValidateVolume(newClipVolume);


        // Don't start a new crossfade if one is already in progress.
        if (_FadeCoroutine == null)
        {
            // Start playing the new clip muted.
            _CurrentTrack.volume = 0;
            _CurrentTrack.Play();

            // Fade in the new track while fading out the old one.
            if (crossFade)
                _FadeCoroutine = StartCoroutine(DoCrossFade(_Volume, _FadeTime));
            else
                _FadeCoroutine = StartCoroutine(DoFadeOutThenFadeIn(_Volume, _FadeTime));
        }
        else
        {
            Debug.LogWarning($"Cannot crossfade to clip \"{newClip.name}\", because another crossfade is already in progress!");
        }
    }

    public void FadeToDefaultTrack(bool crossFade = true, float defaultTrackVolume = 1.0f, float fadeTime = -1.0f)
    {
        if (DefaultMusic == null)
        {
            Debug.LogWarning($"Cannot crossfade to the original track because it is null!");
            return;
        }

        FadeToTrack(DefaultMusic, crossFade, defaultTrackVolume, fadeTime);
    }


    private void SwapAudioSources()
    {
        AudioSource temp = _CurrentTrack;
        _CurrentTrack = _PreviousTrack;
        _PreviousTrack = temp;
    }

    private float ValidateFadeTime(float fadeTime)
    {
        return fadeTime >= 0 ? fadeTime : DefaultFadeTime;
    }

    private float ValidateVolume(float volume)
    {
        return volume >= 0 ? volume : DefaultVolume;
    }


    /// <summary>
    /// Crossfades between two music tracks.
    /// </summary>
    /// <param name="volume">The folume the new track will fade in to.</param>
    /// <param name="fadeTime">The fade duration.</param>
    /// <remarks>NOTE: This coroutine uses parameters rather than the class's private members (_FadeTime and _Volume), because
    ///                that way the crossfade won't be affected if for some reason those values changed while a fade is happening.</remarks></remrks>
    private IEnumerator DoCrossFade(float volume = -1.0f, float fadeTime = -1.0f)
    {
        float prevTrackVolume = _Volume;


        fadeTime = ValidateFadeTime(fadeTime);
        volume = ValidateVolume(volume);


        // If the fade time is close to zero or negative, then simply switch tracks without crossfading.
        if (fadeTime <= 0.1f)
        {
            _PreviousTrack.Stop();
            _CurrentTrack.Play();
            yield break;
        }



        float elapsedTime = 0;
        
        while (elapsedTime <= fadeTime)
        {
            // Fade in the new current track.
            _CurrentTrack.volume = Mathf.Lerp(0, volume, elapsedTime / fadeTime);

            // Fade out the previous track.
            _PreviousTrack.volume = Mathf.Lerp(prevTrackVolume, 0, elapsedTime / fadeTime);

            elapsedTime += Time.deltaTime;
            yield return null;

        } // end while


        _CurrentTrack.volume = volume;
        _PreviousTrack.volume = 0;

        
        _PreviousTrack.Stop();
        _FadeCoroutine = null; // Clear the reference to this coroutine since it has finished.
    }


    /// <summary>
    /// Fades the current track out completely before fading in the new one..
    /// </summary>
    /// <param name="volume">The folume the new track will fade in to.</param>
    /// <param name="fadeTime">The fade duration.</param>
    /// <remarks>NOTE: This coroutine uses parameters rather than the class's private members (_FadeTime and _Volume), because
    ///                that way the crossfade won't be affected if for some reason those values changed while a fade is happening.</remarks></remrks>
    private IEnumerator DoFadeOutThenFadeIn(float volume = -1.0f, float fadeTime = -1.0f)
    {
        float prevTrackVolume = _Volume;


        fadeTime = ValidateFadeTime(fadeTime);
        volume = ValidateVolume(volume);


        // Divide by two since we have two fade operations happening one after the other.
        fadeTime /= 2;


        // If the fade time is close to zero or negative, then simply switch tracks without crossfading.
        if (fadeTime <= 0.1f)
        {
            _PreviousTrack.Stop();
            _CurrentTrack.Play();
            yield break;
        }



        float elapsedTime = 0;

        // Fade the current track out.
        while (elapsedTime <= fadeTime)
        {
            _PreviousTrack.volume = Mathf.Lerp(prevTrackVolume, 0, elapsedTime / fadeTime);

            elapsedTime += Time.deltaTime;
            yield return null;

        } // end while

        _PreviousTrack.volume = 0;


        elapsedTime = 0;

        // Fade new track in.
        while (elapsedTime <= fadeTime)
        {
            _CurrentTrack.volume = Mathf.Lerp(0, volume, elapsedTime / fadeTime);

            elapsedTime += Time.deltaTime;
            yield return null;

        } // end while

        _CurrentTrack.volume = volume;


        _PreviousTrack.Stop();
        _FadeCoroutine = null; // Clear the reference to this coroutine since it has finished.
    }

}
