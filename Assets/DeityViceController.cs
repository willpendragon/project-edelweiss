using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeityViceController : MonoBehaviour
{
    public GameObject loveEmoticon;
    private Vector3 loveEmoticonStartPos;
    private SpriteRenderer loveEmoticonRenderer;

    private void Start()
    {
        if (loveEmoticon != null)
        {
            loveEmoticonRenderer = loveEmoticon.GetComponent<SpriteRenderer>();
            loveEmoticonStartPos = loveEmoticon.transform.position;
        }
    }

    public void IncreaseLoveValue()
    {
        loveEmoticon.SetActive(true);
        if (loveEmoticon == null || loveEmoticonRenderer == null)
            return;

        loveEmoticon.SetActive(true);
        loveEmoticon.transform.position = loveEmoticonStartPos;
        loveEmoticonRenderer.color = new Color(1f, 1f, 1f, 1f); // Reset alpha

        Sequence seq = DOTween.Sequence();

        // Make sure it's visible and reset scale
        loveEmoticon.SetActive(true);
        loveEmoticon.transform.localScale = Vector3.one;
        loveEmoticonRenderer.color = new Color(1f, 1f, 1f, 1f);

        // Shake more noticeably
        seq.Append(loveEmoticon.transform.DOShakePosition(
            0.25f,
            strength: new Vector3(0.35f, 0.35f, 0),
            vibrato: 15,
            randomness: 90
        ));

        // "Punch" scale (grow and shrink quickly)
        seq.Join(loveEmoticon.transform.DOPunchScale(
            new Vector3(0.4f, 0.4f, 0),
            duration: 0.3f,
            vibrato: 10,
            elasticity: 0.9f
        ));

        // Fly upward
        seq.Append(loveEmoticon.transform.DOMoveY(
            loveEmoticonStartPos.y + 1.5f,
            0.5f
        ).SetEase(Ease.OutQuad));

        // Fade out while moving
        seq.Join(loveEmoticonRenderer.DOFade(0f, 0.5f).SetDelay(0.1f));

        // Cleanup
        seq.OnComplete(() =>
        {
            loveEmoticon.SetActive(false);
        });
    }
}
