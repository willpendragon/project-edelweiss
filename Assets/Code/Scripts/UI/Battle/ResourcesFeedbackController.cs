using DG.Tweening;
using UnityEngine;

public class ResourcesFeedbackController : MonoBehaviour
{
    [Header("Setup")]
    public Transform destination;                          // UI icon destination (e.g. HUD panel)
    public GameObject crystalUpgradeIconPrefab;            // World-space flying icon prefab
    public float duration = 1f;                            // Total fly + fade duration

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Q))
    //    {
    //        MoveCrystalIcon(Color.red);
    //    }
    //}
    private void OnEnable()
    {
        TileController.OnPrizeCollected += MoveCrystalIcon;
    }

    private void OnDisable()
    {
        TileController.OnPrizeCollected -= MoveCrystalIcon;
    }

    private void MoveCrystalIcon(Color color)
    {
        // Get the starting position from the active unit
        Transform unit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").transform;
        Vector3 startPos = unit.position;

        // Instantiate icon at world position
        GameObject icon = Instantiate(crystalUpgradeIconPrefab, startPos, Quaternion.identity);
        Transform iconTransform = icon.transform;
        SpriteRenderer sr = icon.GetComponent<SpriteRenderer>();
        TrailRenderer trail = icon.GetComponent<TrailRenderer>();

        sr.material.color = color;

        //// Retrieve destination position of the character who collected the upgrade (using ProfilesUIManager)
        //var profileHelper = BattleInterface.Instance.PlayerPartyProfilesUIManager.RetrieveProfile(unit.GetComponent<Unit>().unitTemplate.unitName);

        //destination = profileHelper.gameObject.transform;

        // Compute world position of the destination UI panel
        Vector3 screenTarget = RectTransformUtility.WorldToScreenPoint(Camera.main, destination.position);
        Vector3 worldTarget = Camera.main.ScreenToWorldPoint(new Vector3(screenTarget.x, screenTarget.y, 10f)); // 10f = distance in front of camera

        // Animate movement, scale, and fade
        AnimateIcon(iconTransform, sr, worldTarget);

        // Animate trail shrinking and fading
        AnimateTrail(trail);

        // Cleanup on completion
        Destroy(icon, duration + 0.1f);
        // Plays the collected crystal upgrade on the Active Player Unit UI profile.
        BattleInterface.Instance.PlayerPartyProfilesUIManager.CollectUpgradeFeedback();
    }

    private void AnimateIcon(Transform iconTransform, SpriteRenderer sr, Vector3 targetWorldPosition)
    {
        Sequence s = DOTween.Sequence();

        // Move and shrink during the full duration
        s.Append(iconTransform.DOMove(targetWorldPosition, duration).SetEase(Ease.InOutQuad));
        s.Join(iconTransform.DOScale(Vector3.zero, duration).SetEase(Ease.InQuad));

        // Fade starts after 70% of the animation, so it fades near the end
        float fadeStartDelay = duration * 0.3f;
        float fadeDuration = duration * 0.7f;

        s.Join(sr.DOFade(0f, fadeDuration)
            .SetEase(Ease.InQuad)
            .SetDelay(fadeStartDelay));
    }
    private void AnimateTrail(TrailRenderer trail)
    {
        if (trail == null) return;

        trail.material = new Material(Shader.Find("Sprites/Default"));
        Color initialColor = trail.material.color;
        trail.widthCurve = AnimationCurve.Constant(0, 1, trail.startWidth);

        float fadeStartDelay = duration * 0.3f;
        float fadeDuration = duration * 0.7f;

        // Fade trail alpha
        DOTween.ToAlpha(
            () => trail.material.color,
            c => trail.material.color = c,
            0f,
            fadeDuration
        )
        .SetEase(Ease.OutQuad)
        .SetDelay(fadeStartDelay);

        // Shrink trail width
        DOTween.To(() => trail.startWidth, w =>
        {
            trail.startWidth = w;
            trail.endWidth = w * 0.2f;
        }, 0f, duration).SetEase(Ease.InQuad);
    }

}
