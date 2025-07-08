using UnityEngine;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    public enum CursorState
    {
        Basic,
        Select,
        Melee,
        Spell,
        Summon,
        Pray,
        Run
    }

    [SerializeField] TileController tileController;
    [SerializeField] public CursorState state;
    [SerializeField] IPlayerAction actionType;
    [SerializeField] GameObject meleeButtonPrefab;
    [SerializeField] GameObject meleeButtonPrefabInstance;
    [SerializeField] Button spellButton;
    [SerializeField] Canvas radialMenu;
    [SerializeField] GameObject buttonSpawnPoint;


    private void Start()
    {
        state = CursorState.Basic;
    }

    public void HandleDragEnter()
    {
        Debug.Log("Tile Drag Enter");
        RetrieveTileStatus();
    }
    public void HandlePointerExit()
    {
        Destroy(meleeButtonPrefabInstance);
        Debug.Log("Pointer Exiting Tile");
    }
    private void RetrieveTileStatus()
    {
        if (tileController.detectedUnit != null)
        {
            OpenRadialMenu();
        }
    }
    void OpenRadialMenu()
    {
        // Retrieve Available Moveset - contextually check based on grid logic (distance)
        // Spawn Radial Menu UI on the Battlefield
        // Populate with contextually relevant Moveset Buttons
        // Add Listeners

        if (CheckDistance())
        {
            meleeButtonPrefabInstance = Instantiate(meleeButtonPrefab, radialMenu.transform);
        }
        meleeButtonPrefabInstance?.GetComponent<Button>().onClick.AddListener(() => ChangeTileToMelee());
        //spellButton.onClick.AddListener(() => ChangeTileToSpell());
    }
    private void ChangeTileToMelee()
    {

        tileController.currentPlayerAction = new MeleePlayerAction();
        state = CursorState.Melee;
        ChangeCursorMode(state);
    }
    private void ChangeTileToSpell()
    {
        tileController.currentPlayerAction = new AOESpellPlayerAction();
        state = CursorState.Spell;
        ChangeCursorMode(state);
    }

    public void ChangeCursorMode(CursorState state)
    {
        switch (state)
        {
            case CursorState.Melee:
                tileController.currentPlayerAction.Execute(tileController);
                break;
        }
    }

    private bool CheckDistance()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        int distance = GridManager.Instance.gridMovementController.GetDistance(activePlayerUnit.ownedTile, tileController);
        if (distance <= 3)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
