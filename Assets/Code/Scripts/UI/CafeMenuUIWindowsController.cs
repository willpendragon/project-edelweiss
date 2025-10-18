using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CafeMenuUIWindowsController : MonoBehaviour
{
    public CanvasGroup[] cafeMenus; // Assign in inspector
    public Button[] buttons; // Assign in inspector
    public CanvasGroup cafeMenuGroup;

    public const string PASTRY_SHOP = "Pastry Shop";
    public const string DIALOGUES = "Dialogues";
    public const string PASTRY_CRAFTING = "Pastry Crafting";

    private void Start()
    {
        // Hide all menus at the start
        for (int i = 0; i < cafeMenus.Length; i++)
        {
            SetMenuVisible(i, false);
        }

        // Setup button listeners without repetition
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Capture index for lambda
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => ToggleMenu(index));
        }
    }

    public void ToggleMenu(int menuIndex)
    {
        bool isMenuOpen = cafeMenus[menuIndex].alpha > 0.5f;

        // Close other menus if this one is going to be opened
        if (!isMenuOpen)
        {
            for (int i = 0; i < cafeMenus.Length; i++)
            {
                if (i != menuIndex)
                {
                    SetMenuVisible(i, false);
                }
            }
            // Special case for menu1 (Pastry Shop)
            if (menuIndex == 0)
            {
                var cafeMenu = FindAnyObjectByType<CafeMenuUIController>();
                cafeMenu.GenerateFoodList();
            }
        }

        // Toggle the current menu.
        SetMenuVisible(menuIndex, !isMenuOpen);
    }

    private void SetMenuVisible(int index, bool visible)
    {
        cafeMenus[index].alpha = visible ? 1f : 0f;
        cafeMenus[index].interactable = visible;
        cafeMenus[index].blocksRaycasts = visible;
    }
}