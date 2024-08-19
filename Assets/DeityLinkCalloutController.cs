using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class DeityLinkCalloutController : MonoBehaviour
{
    [SerializeField] Image unitDeityLinkPanel;
    [SerializeField] RectTransform unitDeityLinkPanelCalloutBackground;
    [SerializeField] GameObject deityPortrait;
    [SerializeField] GameObject unitPortrait;
    //[SerializeField] Image fadePanel;
    [SerializeField] CanvasGroup unitDeityLinkPanelCanvasGroup;
    [SerializeField] TextMeshProUGUI linkedText;

    // Start is called before the first frame update

    public void PlayDeityLinkCalloutTransition(Sprite selectedUnitPortrait, Sprite linkedDeityPortrait)
    {
        Sequence deityLinkCalloutSequence = DOTween.Sequence();

        unitPortrait.GetComponent<Image>().sprite = selectedUnitPortrait;
        deityPortrait.GetComponent<Image>().sprite = linkedDeityPortrait;

        deityLinkCalloutSequence.Append(unitDeityLinkPanel.DOFade(0.9f, 0.3f));
        unitDeityLinkPanelCalloutBackground.anchoredPosition = new Vector2(-1920, unitDeityLinkPanelCalloutBackground.anchoredPosition.y);
        Vector3 destination = new Vector3(0, unitDeityLinkPanelCalloutBackground.position.y, 0);

        linkedText.gameObject.SetActive(true);
        deityLinkCalloutSequence.Append(unitPortrait.GetComponent<Image>().DOFade(1, 0.3f))
                .Join(deityPortrait.GetComponent<Image>().DOFade(1, 0.3f))
                .Join(unitDeityLinkPanelCalloutBackground.GetComponent<Image>().DOFade(1, 0.3f));

        deityLinkCalloutSequence.Append(unitDeityLinkPanelCalloutBackground.DOMove(destination, 0.4f).SetEase(Ease.OutQuad));
        deityLinkCalloutSequence.Append(deityPortrait.transform.DOShakePosition(0.5f, new Vector3(10, 10, 0), vibrato: 10, randomness: 0))
                .Join(unitPortrait.transform.DOShakePosition(0.5f, new Vector3(10, 10, 0), vibrato: 10, randomness: 0))
                .Join(linkedText.transform.DOShakePosition(0.5f, new Vector3(10, 10, 0), vibrato: 10, randomness: 0));

        deityLinkCalloutSequence.AppendInterval(1f);

        deityLinkCalloutSequence.Append(deityPortrait.GetComponent<Image>().DOFade(0, 0.5f))
                .Join(deityPortrait.GetComponent<Image>().DOFade(0, 0.5f))
                .Join(unitPortrait.GetComponent<Image>().DOFade(0, 0.5f))
                .Join(unitDeityLinkPanel.DOFade(0, 0.5f))
                .Join(unitDeityLinkPanelCalloutBackground.GetComponent<Image>().DOFade(0, 0.5f));

        unitDeityLinkPanelCalloutBackground.anchoredPosition = new Vector2(1920, unitDeityLinkPanelCalloutBackground.anchoredPosition.y);

        deityLinkCalloutSequence.AppendCallback(() =>
        {
            deityPortrait.GetComponent<Image>().sprite = null;
            unitPortrait.GetComponent<Image>().sprite = null;
            linkedText.gameObject.SetActive(false);
        });

        // Start the sequence
        deityLinkCalloutSequence.Play();
    }
}
