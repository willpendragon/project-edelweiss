using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Required for scene switching
using TMPro;
using DG.Tweening;

public class CutsceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image cutsceneImage;
    [SerializeField] private TextMeshProUGUI cutsceneText;
    [SerializeField] private Button skipButton; // Reference to your UI Button

    [Header("Settings")]
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private CutsceneData currentCutscene;
    [SerializeField] private string sceneToLoad; // Manually enter scene name in Inspector

    private Coroutine cutsceneCoroutine;

    private void Start()
    {
        SetUIAlpha(0f);

        // Assign the skip button listener via code
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipCutscene);
        }

        if (playOnStart && currentCutscene != null)
        {
            PlayCutscene(currentCutscene);
        }
    }

    public void PlayCutscene(CutsceneData data)
    {
        if (data == null || data.slides.Length == 0)
        {
            Debug.LogWarning("Cutscene data is empty!");
            return;
        }

        // Store reference to the coroutine so we can stop it if skipped
        cutsceneCoroutine = StartCoroutine(ExecuteCutscene(data));
    }

    private IEnumerator ExecuteCutscene(CutsceneData data)
    {
        foreach (CutsceneSlide slide in data.slides)
        {
            cutsceneImage.sprite = slide.image;
            cutsceneText.text = slide.text;

            Sequence fadeInSequence = DOTween.Sequence();
            fadeInSequence.Join(cutsceneImage.DOFade(1f, slide.fadeDuration));
            fadeInSequence.Join(cutsceneText.DOFade(1f, slide.fadeDuration));
            
            yield return fadeInSequence.WaitForCompletion();

            yield return new WaitForSeconds(slide.duration);

            Sequence fadeOutSequence = DOTween.Sequence();
            fadeOutSequence.Join(cutsceneImage.DOFade(0f, slide.fadeDuration));
            fadeOutSequence.Join(cutsceneText.DOFade(0f, slide.fadeDuration));

            yield return fadeOutSequence.WaitForCompletion();
            
            yield return new WaitForSeconds(0.2f);
        }

        EndCutscene();
    }

    public void SkipCutscene()
    {
        Debug.Log("Cutscene Skipped!");
        
        // Stop the sequencing coroutine
        if (cutsceneCoroutine != null)
        {
            StopCoroutine(cutsceneCoroutine);
        }

        // Kill all active DOTween animations safely to prevent memory leaks or errors
        DOTween.KillAll();

        EndCutscene();
    }

    private void EndCutscene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("Scene To Load is empty! Make sure to type the exact scene name in the inspector.");
        }
    }

    private void SetUIAlpha(float alpha)
    {
        Color imgColor = cutsceneImage.color;
        imgColor.a = alpha;
        cutsceneImage.color = imgColor;

        Color txtColor = cutsceneText.color;
        txtColor.a = alpha;
        cutsceneText.color = txtColor;
    }

    private void OnDestroy()
    {
        // Clean up button listener if this object is destroyed
        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(SkipCutscene);
        }
    }
}