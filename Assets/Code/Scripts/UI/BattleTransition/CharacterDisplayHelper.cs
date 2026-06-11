using ProjectEdelweiss.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterDisplayHelper : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case GameTags.BATTLE_SCENE:
                ShowPartyCharacters();
                break;
            case GameTags.BATTLE_TRANSITION:
                HidePartyCharacters();
                break;
        }
    }

    private void ShowPartyCharacters()
    {
        if (GameManager.Instance == null || GameManager.Instance.playerPartyMembersInstances == null) return;

        foreach (var character in GameManager.Instance.playerPartyMembersInstances)
        {
            if (character == null) continue; // Null check prevents crashing on destroyed units!

            SpriteRenderer sprite = character.GetComponentInChildren<SpriteRenderer>();
            if (sprite != null)
            {
                Color characterColor = sprite.material.color;
                characterColor.a = 1;
                sprite.material.color = characterColor;
            }
        }
    }

    private void HidePartyCharacters()
    {
        if (GameManager.Instance == null || GameManager.Instance.playerPartyMembersInstances == null) return;

        foreach (var character in GameManager.Instance.playerPartyMembersInstances)
        {
            if (character == null) continue;

            SpriteRenderer sprite = character.GetComponentInChildren<SpriteRenderer>();
            if (sprite != null)
            {
                Color characterColor = sprite.material.color;
                characterColor.a = 0;
                sprite.material.color = characterColor;
            }
        }
    }
}
