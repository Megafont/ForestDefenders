
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
    public static SceneSwitcher Instance;


    public Color32 DefaultScreenFadeColor = Color.black;
    public float DefaultScreenFadeDuration = 2.5f;


    private Image _ScreenFader;



    private void Awake()
    {
        // If there is already an instance of SceneSwitcher, then destroy this one.
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;

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

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(DoFadeToScene(sceneName, DefaultScreenFadeColor, DefaultScreenFadeDuration));
    }

    public void FadeToScene(int sceneBuildIndex)
    {
        StartCoroutine(DoFadeToScene(sceneBuildIndex, DefaultScreenFadeColor, DefaultScreenFadeDuration));
    }

    public void FadeToScene(string sceneName, Color32 fadeColor, float fadeDuration)
    {
        StartCoroutine(DoFadeToScene(sceneName, fadeColor, fadeDuration));
    }

    public void FadeToScene(int sceneBuildIndex, Color32 fadeColor, float fadeDuration)
    {
        StartCoroutine(DoFadeToScene(sceneBuildIndex, fadeColor, fadeDuration));
    }

    public void FadeIn(Color32 fadeColor, float fadeDuration)
    {
        StartCoroutine(DoScreenFade(fadeColor, Color.clear, fadeDuration));
    }

    public void FadeIn()
    {
        StartCoroutine(DoScreenFade(DefaultScreenFadeColor, Color.clear, DefaultScreenFadeDuration));
    }

    public void FadeOut(Color32 fadeColor, float fadeDuration)
    {
        StartCoroutine(DoScreenFade(Color.clear, fadeColor, fadeDuration));
    }

    public void FadeOut()
    {
        StartCoroutine(DoScreenFade(Color.clear, DefaultScreenFadeColor, DefaultScreenFadeDuration));
    }



    private IEnumerator DoFadeToScene(string sceneName, Color32 fadeColor, float fadeDuration)
    {
        // If a transition is already in progress, then simply cancel this one.
        if (IsTransitioningToScene)
        {
            Debug.LogWarning("Cannot start a scene transition when one is already underway!");
            yield break;
        }


        IsTransitioningToScene = true;


        // Fade out the current scene, and wait for the fade to complete.
        yield return StartCoroutine(DoScreenFade(Color.clear, fadeColor, fadeDuration));

        // Load the new scene.        
        SceneManager.LoadScene(sceneName);

        // Fade in the new scene, and wait for the fade to complete.
        yield return StartCoroutine(DoScreenFade(fadeColor, Color.clear, fadeDuration));


        IsTransitioningToScene = false;

    }

    private IEnumerator DoFadeToScene(int sceneBuildIndex, Color32 fadeColor, float fadeDuration)
    {
        return DoFadeToScene(SceneManager.GetSceneByBuildIndex(sceneBuildIndex).name, fadeColor, fadeDuration);
    }

    private IEnumerator DoScreenFade(Color32 startColor, Color32 endColor, float fadeDuration)
    {
        // If a fade is already in progress, then simply cancel this one.
        if (IsFading)
        {
            Debug.LogWarning("Cannot start a screen fade when one is already underway!");
            yield break;
        }


        IsFading = true;


        _ScreenFader.gameObject.SetActive(true);
        _ScreenFader.color = startColor;


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
        
        // If the alpha level of end color is 0, then deactivate the screen fader panel since we no longer need it if it is completely transparent.
        if (endColor.a == 0)
            _ScreenFader.gameObject.SetActive(false);


        IsFading = false;
    }



    public string ActiveSceneName { get { return SceneManager.GetActiveScene().name; } }

    public bool IsFading { get; private set; }
    public bool IsTransitioningToScene { get; private set; }

}
