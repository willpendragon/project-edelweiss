using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConversationUI : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform buttonContainer;

    private void Start()
    {
        GenerateConversationButtons();
    }

    private void GenerateConversationButtons()
    {
        GameSaveData conversationData = SaveStateManager.saveData;

        foreach (var convo in conversationData.unlockedConversations)
        {
            if (convo.isUnlocked)
            {
                GameObject button = Instantiate(buttonPrefab, buttonContainer);
                button.GetComponentInChildren<TextMeshProUGUI>().text = $"{convo.conversationID}";
                string convoID = convo.conversationID;
                button.GetComponent<Button>().onClick.AddListener(() => StartConversation(convoID));
            }
        }
    }

    private void StartConversation(string id)
    {
        // Assuming you have your player and NPC references ready
        //Transform playerTransform = /* Your player transform */;
        //Transform npcTransform = /* Your NPC transform */;

        ConversationManager.Instance.StartConversationByID(id);
    }
}