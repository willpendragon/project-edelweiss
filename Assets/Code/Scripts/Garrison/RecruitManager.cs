using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class RecruitManager : MonoBehaviour
{
    public static RecruitManager Instance { get; private set; }

    public const int MaxActivePartySize = 3;

    [Header("Recruit Status Lists")] public List<Unit> lockedUnits = new List<Unit>();

    // This array acts as the exact 3 slots. Null means the slot is empty.
    public Unit[] selectedActiveParty = new Unit[MaxActivePartySize];

    public List<Unit> AvailablePartyMembers =>
        GameManager.Instance != null ? GameManager.Instance.playerPartyMembers : new List<Unit>();

    [Header("UI References")] public Transform activePartyGrid;
    public Transform availableUnitsGrid;
    public GameObject unitPortraitPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Handles display of characters in the Garrison UI.
        int limit = Mathf.Min(MaxActivePartySize, AvailablePartyMembers.Count);
        for (int i = 0; i < limit; i++)
        {
            selectedActiveParty[i] = AvailablePartyMembers[i];
        }

        RefreshUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12)) DebugLoadCafe();
    }

    public void DebugLoadCafe()
    {
        Debug.Log("Loading 'cafe_node' scene to test party instance updates...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("cafe_node");
    }

    /// <summary>
    /// Writes the current roster back to GameSaveData and forces a file save.
    /// </summary>
    private void SavePartyToSaveFile()
    {
        GameSaveData currentSave = SaveStateManager.saveData;
        if (currentSave == null) return;

        currentSave.activePartyUnitIds.Clear();
        foreach (Unit active in selectedActiveParty)
        {
            if (active != null) currentSave.activePartyUnitIds.Add(active.Id);
        }

        currentSave.availablePartyUnitIds.Clear();
        foreach (Unit available in AvailablePartyMembers)
        {
            if (!System.Array.Exists(selectedActiveParty, u => u != null && u.unitTemplate == available.unitTemplate))
            {
                currentSave.availablePartyUnitIds.Add(available.Id);
            }
        }

        SaveStateManager.SaveGame(currentSave);
    }

    public void RefreshUI()
    {
        PopulateActiveGrid();
        PopulateAvailableGrid();
    }

    private void PopulateActiveGrid()
    {
        foreach (Transform child in activePartyGrid) Destroy(child.gameObject);

        for (int i = 0; i < MaxActivePartySize; i++)
        {
            GameObject slotInstance = Instantiate(unitPortraitPrefab, activePartyGrid);
            RecruitSlotUI slotUI = slotInstance.GetComponent<RecruitSlotUI>() ??
                                   slotInstance.AddComponent<RecruitSlotUI>();
            slotUI.isActiveSlot = true;
            slotUI.slotIndex = i;
            slotUI.assignedUnit = selectedActiveParty[i];

            // Image portraitImage = slotInstance.GetComponentInChildren<Image>();
            Image portraitImage = slotUI.portrait;
            if (portraitImage != null && selectedActiveParty[i] != null && selectedActiveParty[i].unitTemplate != null)
            {
                portraitImage.sprite = selectedActiveParty[i].unitTemplate.unitPortrait;
                portraitImage.color = Color.white;
            }
            else if (portraitImage != null)
            {
                portraitImage.sprite = null;
                portraitImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            }
        }
    }

    private void PopulateAvailableGrid()
    {
        foreach (Transform child in availableUnitsGrid) Destroy(child.gameObject);

        foreach (Unit unit in AvailablePartyMembers)
        {
            if (unit == null) continue;

            GameObject slotInstance = Instantiate(unitPortraitPrefab, availableUnitsGrid);
            RecruitSlotUI slotUI = slotInstance.GetComponent<RecruitSlotUI>() ??
                                   slotInstance.AddComponent<RecruitSlotUI>();
            slotUI.isActiveSlot = false;
            slotUI.assignedUnit = unit;

            // Image portraitImage = slotInstance.GetComponentInChildren<Image>();
            Image portraitImage = portraitImage = slotUI.portrait;
            if (portraitImage != null && unit.unitTemplate != null)
            {
                portraitImage.sprite = unit.unitTemplate.unitPortrait;
                bool isCurrentlyActive = System.Array.Exists(selectedActiveParty,
                    activeUnit => activeUnit != null && activeUnit.unitTemplate == unit.unitTemplate);

                portraitImage.color = isCurrentlyActive ? Color.red : Color.white;
            }
        }
    }

    public void RemoveFromActiveParty(int slotIndex)
    {
        selectedActiveParty[slotIndex] = null;
        SyncWithGameManager();
        SavePartyToSaveFile();
        RefreshUI();
    }

    public void AddToActiveParty(Unit unitToAdd)
    {
        if (System.Array.Exists(selectedActiveParty, u => u != null && u.unitTemplate == unitToAdd.unitTemplate))
            return;

        for (int i = 0; i < MaxActivePartySize; i++)
        {
            if (selectedActiveParty[i] == null)
            {
                selectedActiveParty[i] = unitToAdd;
                SyncWithGameManager();
                SavePartyToSaveFile();
                RefreshUI();
                return;
            }
        }

        Debug.Log("Active party is full!");
    }

    /// <summary>
    /// Re-orders GameManager.playerPartyMembers so the assigned units are grouped at the front.
    /// Re-instantiates the units so systems like the Cafe update immediately.
    /// </summary>
    private void SyncWithGameManager()
    {
        List<Unit> newOrder = new List<Unit>();

        foreach (Unit active in selectedActiveParty)
        {
            if (active != null) newOrder.Add(active);
        }

        foreach (Unit rosterUnit in GameManager.Instance.playerPartyMembers)
        {
            if (!newOrder.Exists(u => u.unitTemplate == rosterUnit.unitTemplate))
            {
                newOrder.Add(rosterUnit);
            }
        }

        GameManager.Instance.playerPartyMembers.Clear();
        GameManager.Instance.playerPartyMembers.AddRange(newOrder);

        // Refresh the living instances so they are available immediately out of the menu
        GameManager.Instance.InstantiateUnits();
    }
}