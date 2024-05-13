using DG.Tweening;
using TMPro;
using UnityEngine;

public class BanterController : MonoBehaviour
{
    public GameObject banterComic;
    public TextMeshProUGUI banterComicText;
    private CanvasGroup banterCanvasGroup;
    private SpriteRenderer comicSpriteRenderer;

    private float banterDuration = 3.5f;

    private void Awake()
    {
        comicSpriteRenderer = banterComic.GetComponent<SpriteRenderer>();
        banterComicText = banterComic.GetComponentInChildren<TextMeshProUGUI>();
        banterCanvasGroup = banterComic.GetComponent<CanvasGroup>();

        if (banterCanvasGroup == null)
        {
            banterCanvasGroup = banterComic.AddComponent<CanvasGroup>();
        }
    }

    private void OnEnable()
    {
        ShowBanter();
        DOVirtual.DelayedCall(banterDuration, HideBanter);
    }

    void ShowBanter()
    {
        banterCanvasGroup.alpha = 0;
        banterCanvasGroup.blocksRaycasts = true;

        // Fade in the entire Canvas Group
        banterCanvasGroup.DOFade(1, 0.5f);

        // Scale up with a bounce
        banterComic.transform.DOScale(2f, 0.5f).SetEase(Ease.OutBack);


    }

    void HideBanter()
    {
        // Fade out the Canvas Group
        banterCanvasGroup.DOFade(0, 0.5f);

        // Scale down with an elastic effect to make it more dynamic
        banterComic.transform.DOScale(Vector3.zero, 0.7f).SetEase(Ease.InElastic).OnComplete(() =>
        {
            banterCanvasGroup.blocksRaycasts = false;
        });
    }
}