using UnityEngine;
using TMPro;
using DG.Tweening;

public class DeityEnmityTrackerController : MonoBehaviour
{
    public void OnEnable()
    {
        SpellcastingController.OnCastedSpellTypeHatedbyDeity += UpdateDeityEnmityTracker;
    }
    public void OnDisable()
    {
        SpellcastingController.OnCastedSpellTypeHatedbyDeity -= UpdateDeityEnmityTracker;
    }

    public TextMeshProUGUI deityEnmityPointsCounter;
    public Deity deity;
    [SerializeField] RectTransform enmityIconRect;
    public void SetDeity(GameObject deityGO)
    {
        deity = deityGO.GetComponent<Deity>();
    }
    public void UpdateDeityEnmityTracker()
    {
        deityEnmityPointsCounter.text = deity.enmity.ToString();
        Debug.Log("Updated Enmity Tracker");
        PlayEnmityIconFeedback();
    }
    public void PlayEnmityIconFeedback()
    {
        // Kill any existing animations to prevent stacking
        DOTween.Kill(enmityIconRect);

        // Reset to default scale
        enmityIconRect.localScale = Vector3.one * 0.328135f;

        // Pop and bounce
        enmityIconRect.DOScale(enmityIconRect.localScale * 1.6f, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                enmityIconRect.DOScale(Vector3.one * 0.328135f, 0.2f).SetEase(Ease.InOutSine);
            });

        // Twitchy vertical jump
        enmityIconRect.DOJumpAnchorPos(
            enmityIconRect.anchoredPosition,
            12f, // jump power
            2,   // jumps
            0.35f
        ).SetEase(Ease.OutQuad);
    }
}
