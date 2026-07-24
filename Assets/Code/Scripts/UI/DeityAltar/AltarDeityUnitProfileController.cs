using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AltarDeityUnitProfileController : MonoBehaviour
{
    [Header("Deity Character Details")] [SerializeField]
    TextMeshProUGUI deityName;

    [SerializeField] Image deityUnitPortrait;
    [SerializeField] Image linkedUnitPortrait;

    [Header("Deity Character UI")] [SerializeField]
    TextMeshProUGUI buffType;

    [SerializeField] TextMeshProUGUI buffAmountCounter;
    [SerializeField] Slider buffAmountSlider;
    [SerializeField] Button selectDeityButton;

    private Deity selectedDeity;

    public void PopulateDeityUnitProfile(Unit deityUnit, Deity deity)
    {
        // Null safety checks
        if (deityUnit == null || deity == null)
        {
            Debug.LogWarning("PopulateDeityUnitProfile: Null deity or unit provided");
            return;
        }

        if (deityUnit.unitTemplate == null)
        {
            Debug.LogWarning("PopulateDeityUnitProfile: Deity unit missing template");
            return;
        }

        deityName.text = deityUnit.unitTemplate.unitName;

        // Handle summoning behavior generically for all deity types
        if (deity.summoningBehaviour != null)
        {
            // Try to get moveName from Anguana type specifically, otherwise use deity name as fallback
            DeityAnguanaSummoningBehavior anguanaBehavior = deity.summoningBehaviour as DeityAnguanaSummoningBehavior;
            buffType.text = anguanaBehavior?.moveName ?? deityUnit.unitTemplate.unitName ?? "Deity Ability";
            
            // Use base class description property (works for all deity types)
            buffAmountCounter.text = deity.summoningBehaviour.description ?? "Deity summoning ability";
        }
        else
        {
            // Fallback if no summoning behavior
            buffType.text = deityUnit.unitTemplate.unitName;
            buffAmountCounter.text = "Deity summoning ability";
            Debug.LogWarning($"Deity {deityUnit.unitTemplate.unitName} missing summoningBehaviour");
        }

        // Portrait with null check
        if (deityUnit.unitTemplate.unitPortrait != null)
        {
            deityUnitPortrait.sprite = deityUnit.unitTemplate.unitPortrait;
        }
        else
        {
            Debug.LogWarning($"Missing portrait for deity: {deityUnit.unitTemplate.unitName}");
        }
        
        linkedUnitPortrait.sprite = RetrieveLinkedUnitSmallPortrait(deity);
        selectedDeity = deity;
        selectDeityButton.onClick.AddListener(SelectDeityUnit);
    }

    public void SelectDeityUnit()
    {
        Debug.Log("SelectedDeityUnit");
        DeityAltarController deityAltarController = GameObject.FindGameObjectWithTag("DeityAltarController")
            .GetComponent<DeityAltarController>();
        deityAltarController.AssignDeityToUnit(selectedDeity);
        linkedUnitPortrait.sprite = RetrieveLinkedUnitSmallPortrait(selectedDeity);
    }

    public Sprite RetrieveLinkedUnitSmallPortrait(Deity deity)
    {
        foreach (var playerUnit in GameManager.Instance.playerPartyMembersInstances)
        {
            if (playerUnit.LinkedDeityId == deity.Id)
            {
                Debug.Log("Found Linked Unit");
                return playerUnit.GetComponent<Unit>().unitTemplate.unitMiniPortrait;
            }
        }

        Debug.Log("No Linked Unit Found");

        return null;
    }
}