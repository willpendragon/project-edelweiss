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
        var partyCharacters = GameManager.Instance.playerPartyMembersInstances;
        foreach (var character in partyCharacters)
        {
            Color characterColor = character.GetComponentInChildren<SpriteRenderer>().material.color;
            characterColor.a = 1;
            character.GetComponentInChildren<SpriteRenderer>().material.color = characterColor;
        }

    }
    private void HidePartyCharacters()
    {
        var partyCharacters = GameManager.Instance.playerPartyMembersInstances;
        foreach (var character in partyCharacters)
        {
            Color characterColor = character.GetComponentInChildren<SpriteRenderer>().material.color;
            characterColor.a = 0;
            character.GetComponentInChildren<SpriteRenderer>().material.color = characterColor;
        }
    }

}
