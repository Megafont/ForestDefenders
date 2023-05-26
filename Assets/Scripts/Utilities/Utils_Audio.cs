using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public static class Utils_Audio
{
    public static IEnumerator FadeAudioSource(AudioSource source, float startVolume, float endVolume, float fadeDuration)
    {
        if (source == null)
            throw new System.Exception("The passed in AudioSource is null!");


        source.volume = startVolume;
        endVolume = Mathf.Clamp(endVolume, 0f, 1f);


        if (!source.isPlaying)
            source.Play();

        fadeDuration = Mathf.Max(fadeDuration, 0f);
        if (fadeDuration == 0)
        {
            Debug.LogWarning("Cannot fade the AudioSource, because fadeDuration is 0!");
            yield break;
        }


        float elapsedTime = 0;
        while (elapsedTime <= fadeDuration)
        {
            // Fade in the new current track.
            source.volume = Mathf.Lerp(startVolume, endVolume, elapsedTime / fadeDuration);

            elapsedTime += Time.deltaTime;
            yield return null;

        } // end while


        // Make sure the volume has faded all the way to the specified target volume.
        source.volume = endVolume;

        if (endVolume == 0)
            source.Stop();

    }

}
