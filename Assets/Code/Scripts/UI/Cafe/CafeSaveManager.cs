using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CafeSaveManager : MonoBehaviour
{
    public void SaveRestoredCharacterStats()
    {
        //Saves the stats after feeding.
        GameSaveData characterSaveData = SaveStateManager.saveData;

        foreach (var playerUnit in GameManager.Instance.playerPartyMembersInstances)
        {
            CharacterData existingCharacterData = characterSaveData.characterData.Find(character => character.unitId == playerUnit.Id);
            if (existingCharacterData != null)
            {
                // Update existing character data
                existingCharacterData.unitHealthPoints = playerUnit.unitHealthPoints;
                existingCharacterData.unitSavedManaPoints = playerUnit.unitManaPoints;
                existingCharacterData.unitShieldPoints = playerUnit.unitShieldPoints;

                existingCharacterData.unitLifeCondition = playerUnit.currentUnitLifeCondition;

                existingCharacterData.unitAttackPower = playerUnit.unitAttackPower;
                existingCharacterData.unitMagicPower = playerUnit.unitMagicPower;

                // Update other stats as necessary

                Debug.Log("Character Stats Saved");
            }
        }
        SaveStateManager.SaveGame(characterSaveData);
    }
}
