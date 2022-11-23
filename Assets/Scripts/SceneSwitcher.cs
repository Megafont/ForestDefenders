
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



/// <summary>
/// This class is handles switching between scenes, and screen fades.
/// </summary>
/// <remarks>NOTE: The SceneManager is its own GameObject in the scene, because DontDestroyOnLoad() only works
///                on root objects according to the documentation. I still made it accessible through GameManager, though.</remarks>
public class SceneSwitcher : MonoBehaviour
{
    public Color32 DefaultScreenFadeColor = Color.black;
    public float DefaultScreenFadeDuration = 2.5f;


    private Image _ScreenFader;



    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        string screenFaderName = "Screen Fader";
        Transform screenFaderObject = transform.Find($"Screen Switcher Canvas/{screenFaderName}");
        if (screenFaderObject == null)
            throw new Exception($"Failed to find the {screenFaderName} GameObject!");

        _ScreenFader = screenFaderObject.GetComponent<Image>();
        if (_ScreenFader == null)
            throw new Exception($"The {screenFaderName} GameObject does not have an image component!");


        // Start the screen faded out.
        _ScreenFader.color = DefaultScreenFadeColor;
        _ScreenFader.gameObject.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        // Fade the screen in.
        // NOTE: This is commented out, because it is handled by GameManager after assets are loaded.
        //StartCoroutine(FadeScreen(DefaultScreenFadeColor, Color.clear, DefaultScreenFadeDuration));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeToScene(string sceneName)
    {
        StartCoroutine(FadeToScene(sceneName, DefaultScreenFadeColor, DefaultScreenFadeDuration));
    }

    public void ChangeToScene(int sceneBuildIndex)
    {
        StartCoroutine(FadeToScene(sceneBuildIndex, DefaultScreenFadeColor, DefaultScreenFadeDuration));
    }

    public void ChangeToScene(string sceneName, Color32 fadeColor, float fadeDuration)
    {
        StartCoroutine(FadeToScene(sceneName, fadeColor, fadeDuration));
    }

    public void ChangeToScene(int sceneBuildIndex, Color32 fadeColor, float fadeDuration)
    {
        StartCoroutine(FadeToScene(sceneBuildIndex, fadeColor, fadeDuration));
    }

    public void FadeIn(Color32 fadeColor, float fadeDuration)
    {
        StartCoroutine(FadeScreen(fadeColor, Color.clear, fadeDuration));
    }

    public void FadeIn()
    {
        StartCoroutine(FadeScreen(DefaultScreenFadeColor, Color.clear, DefaultScreenFadeDuration));
    }

    public void FadeOut(Color32 fadeColor, float fadeDuration)
    {
        StartCoroutine(FadeScreen(Color.clear, fadeColor, fadeDuration));
    }

    public void FadeOut()
    {
        StartCoroutine(FadeScreen(Color.clear, DefaultScreenFadeColor, DefaultScreenFadeDuration));
    }



    private IEnumerator FadeToScene(string sceneName, Color32 fadeColor, float fadeDuration)
    {
        // Fade out the current scene, and wait for the fade to complete.
        yield return StartCoroutine(FadeScreen(fadeColor, Color.clear, fadeDuration));

        // Load the new scene.
        SceneManager.LoadScene(sceneName);

        // Fade in the new scene, and wait for the fade to complete.
        yield return StartCoroutine(FadeScreen(Color.clear, fadeColor, fadeDuration));

    }

    private IEnumerator FadeToScene(int sceneBuildIndex, Color32 fadeColor, float fadeDuration)
    {
        return FadeToScene(SceneManager.GetSceneByBuildIndex(sceneBuildIndex).name, fadeColor, fadeDuration);
    }

    private IEnumerator FadeScreen(Color32 startColor, Color32 endColor, float fadeDuration)
    {
        _ScreenFader.color = startColor;
        _ScreenFader.gameObject.SetActive(true);


        float elapsedTime = 0;
        float percent = 0;
        while (elapsedTime <= fadeDuration)
        {
            yield return null;

            //Debug.Log("Fade Percent: " + percent);
            _ScreenFader.color = Color.Lerp(startColor, endColor, percent);

            elapsedTime += Time.deltaTime;
            percent = elapsedTime / fadeDuration;
        }


        _ScreenFader.color = endColor;
        _ScreenFader.gameObject.SetActive(false);
    }

}
