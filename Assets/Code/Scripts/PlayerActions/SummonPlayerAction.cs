using UnityEngine;
using DG.Tweening;

public class SummonPlayerAction : MonoBehaviour, IPlayerAction
{
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    public int deityLimiter = 1;

    public void Select(TileController selectedTile)
    {
        if (selectionLimiter > 0)
        {
            //selectedTile.GetComponentInChildren<SpriteRenderer>().material.color = Color.magenta;
            selectedTile.GetComponentInChildren<TileShaderController>()?.AnimateFadeHeight(3f, 0.1f, Color.cyan);
            selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
            savedSelectedTile = selectedTile;
            selectionLimiter--;
            Debug.Log(selectedTile + "chosen for summoning.");
        }
    }
    public void Execute()
    {
        Unit currentActivePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        Deity linkedDeity = currentActivePlayerUnit.linkedDeity;

        if (linkedDeity != null && currentActivePlayerUnit.unitOpportunityPoints > 0 && deityLimiter > 0)
        {
            savedSelectedTile.currentSingleTileCondition = SingleTileCondition.occupiedByDeity;
            savedSelectedTile.gameObject.GetComponentInChildren<TileShaderController>().AnimateFadeHeight(3f, 0f, Color.green);
            // Beware, hard-coded number
            int summoningCost = 10;
            currentActivePlayerUnit.SpendManaPoints(summoningCost);
            SummonDeityOnBattlefield(linkedDeity, currentActivePlayerUnit);
            Debug.Log("Summoned Deity on Battlefield");
            currentActivePlayerUnit.GetComponent<SummoningUIController>()?.SwitchButtonToPrayMode();
            //currentActivePlayerUnit.unitOpportunityPoints--;
            deityLimiter--;
            Debug.Log("Summoning Deity");
        }
        else if (currentActivePlayerUnit.summonedLinkedDeity != null)
        {
            Debug.Log("This Unit already has summoned a Deity on the Battlefield");
        }
        else
        {
            Debug.Log("Unable to Summon Deity on Battlefield");
        }
    }
    public void Deselect()
    {
        selectionLimiter++;
        if (savedSelectedTile != null)
        {
            //int summoningRange = 2;
            savedSelectedTile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            savedSelectedTile.gameObject.GetComponentInChildren<TileShaderController>().AnimateFadeHeight(0f, 0.2f, Color.white);
            //foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile, summoningRange))

            //{
            //    tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            //}
            Debug.Log("Deselected Summon Spawn Area");
            deityLimiter++;
            savedSelectedTile = null;
        }
        else if (savedSelectedTile == null)
        {
            BattleInterface.Instance.DeactivateActionInfoPanel();
            foreach (var tile in GridManager.Instance.gridTileControllers)
            {
                tile.currentPlayerAction = new SelectUnitPlayerAction();
                tile.tileShaderController.AnimateFadeHeight(0, 0.2f, Color.white);
            }
            GameObject[] playerUISpellButtons = GameObject.FindGameObjectsWithTag("PlayerUISpellButton");
            foreach (var playerUISpellButton in playerUISpellButtons)
            {
                Destroy(playerUISpellButton);
            }
            Destroy(GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().unitProfilePanel);
            GridManager.Instance.currentPlayerUnit.tag = "Player";
            GridManager.Instance.currentPlayerUnit = null;

            GameObject movesContainer = GameObject.FindGameObjectWithTag("MovesContainer");
            movesContainer.transform.localScale = new Vector3(0, 0, 0);
            Destroy(GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon"));
            GridManager.Instance.ClearPath();
            BattleInterface.Instance.DeactivateActionInfoPanel();
        }

        //foreach (var deitySpawningZoneTile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile, 2))
        //{
        //    deitySpawningZoneTile.gameObject.GetComponentInChildren<TileShaderController>().AnimateFadeHeight(0f, 0.2f, Color.white);
        //}


    }
    private void SummonDeityOnBattlefield(Deity linkedDeity, Unit currentActivePlayerUnit)
    {
        var summonPosition = savedSelectedTile.transform.position + new Vector3(0, 3, 0);
        GameObject deityInstance = Instantiate(linkedDeity.gameObject, summonPosition, Quaternion.identity);

        // Set to small scale initially
        deityInstance.transform.localScale = Vector3.zero;

        // Optional: Add a bit of Y movement (like a pop-up)
        deityInstance.transform.DOMoveY(summonPosition.y + 1f, 0.3f)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo);

        // Tween scale to final size with a bounce
        deityInstance.transform.DOScale(new Vector3(2, 2, 2), 0.5f)
            .SetEase(Ease.OutBack); // Gives it a little pop

        // UI and logic
        BattleInterface.Instance.CreateUISummonInfoPanel(deityInstance);
        currentActivePlayerUnit.summonedLinkedDeity = deityInstance.GetComponent<Deity>();
    }
}
