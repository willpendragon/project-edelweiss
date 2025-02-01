using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class MapNodeNotificationsController : MonoBehaviour
{
    [Header("Floating Settings")]
    public float floatDistance = 10f; // How far the bubble will float (up and down)
    public float floatDuration = 2f;  // Duration of one float cycle (up and down)

    public RectTransform rectTransform;
    public TextMeshProUGUI conversationCounter;

    private void Start()
    {

        if (rectTransform != null)
        {
            StartFloating();
            UpdateConversationCounter();
        }
    }

    private void StartFloating()
    {
        // Save the original anchored position
        Vector3 originalPosition = rectTransform.anchoredPosition;

        // Animate the bubble to float up and down using a looping DoTween sequence
        rectTransform
            .DOAnchorPosY(originalPosition.y + floatDistance, floatDuration / 2) // Float up
            .SetEase(Ease.InOutSine) // Smooth float
            .SetLoops(-1, LoopType.Yoyo); // Loop infinitely, going up and down
    }
    private void UpdateConversationCounter()
    {
        GameSaveData conversationData = SaveStateManager.saveData;
        int readConversation = 0;

        foreach (var convo in conversationData.unlockedConversations)
        {
            if (convo.isRead != true && convo.isUnlocked != false)
            {
                readConversation++;
            }
        }
        conversationCounter.text = readConversation.ToString();
    }
}
