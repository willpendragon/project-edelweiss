using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeityListUIController : MonoBehaviour
{
    [SerializeField] CanvasGroup _deityListCanvas;
    [SerializeField] CanvasGroup _mainCanvas;
    [SerializeField] Button _openDeityListButton;
    [SerializeField] GameObject _deityEntryObject;
    [SerializeField] Transform _deityListGrid;

    private void Start()
    {
        _deityListCanvas.alpha = 0f;
        _deityListCanvas.blocksRaycasts = false;
        _openDeityListButton.onClick.AddListener(OpenDeityList);
    }

    private void OpenDeityList()
    {
        _deityListCanvas.alpha = 1.0f;
        _mainCanvas.alpha = 0f;
        _deityListCanvas.blocksRaycasts = true;
    }

    public void AddDeityProfile(Deity linkedDeity)
    {
        // Creates a new Deity profile on the grid list.
        GameObject newDeityEntry = Instantiate(_deityEntryObject, _deityListGrid);
        var entryController = newDeityEntry.GetComponent<DeityEntryController>();
        entryController.FillEntryDetails(linkedDeity);
    }
}
