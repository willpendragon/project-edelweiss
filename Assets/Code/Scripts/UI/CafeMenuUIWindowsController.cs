using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CafeMenuUIWindowsController : MonoBehaviour
{
    public GameObject menu1;
    public GameObject menu2;
    public GameObject menu3;

    public Button button1;
    public Button button2;
    public Button button3;

    public TextMeshProUGUI buttonText1;
    public TextMeshProUGUI buttonText2;
    public TextMeshProUGUI buttonText3;

    public CanvasGroup cafeMenuGroup;

    public const string PASTRY_SHOP = "Pastry Shop";
    public const string DIALOGUES = "Dialogues";
    public const string PASTRY_CRAFTING = "Pastry Crafting";


    private void Start()
    {
        // Initialize both menus to be closed.
        menu1.SetActive(false);
        menu2.SetActive(false);
        menu3.SetActive(false);

        // Set initial button texts.
        buttonText1.text = PASTRY_SHOP;
        buttonText2.text = DIALOGUES;
        buttonText3.text = PASTRY_CRAFTING;

        // Add click listeners to buttons.
        button1.onClick.AddListener(() => ToggleMenu(menu1, buttonText1, PASTRY_SHOP));
        button2.onClick.AddListener(() => ToggleMenu(menu2, buttonText2, DIALOGUES));
        button3.onClick.AddListener(() => ToggleMenu(menu3, buttonText3, PASTRY_CRAFTING)); // Actually Opens Pastry making menu.
    }

    private void ToggleMenu(GameObject menu, TextMeshProUGUI buttonText, string menuName)
    {
        bool isMenuOpen = menu.activeSelf;

        // Close the other menu if this one is going to be opened
        if (!isMenuOpen)
        {
            if (menu == menu1)
            {
                menu2.SetActive(false);
                menu3.SetActive(false);
                buttonText2.text = DIALOGUES;
                buttonText3.text = PASTRY_CRAFTING;
                var cafeMenu = FindAnyObjectByType<CafeMenuUIController>();
                cafeMenu.GenerateFoodList();
            }
            else if (menu == menu2)
            {
                menu1.SetActive(false);
                menu3.SetActive(false);
                buttonText1.text = PASTRY_SHOP;
                buttonText3.text = PASTRY_CRAFTING;
            }
            else if (menu == menu3)
            {
                menu1.SetActive(false);
                menu2.SetActive(false);
                buttonText1.text = PASTRY_SHOP;
                buttonText2.text = DIALOGUES;
            }
        }

        // Toggle the current menu.
        menu.SetActive(!isMenuOpen);

        // Update the button text based on the menu state.
        if (menu.activeSelf)
        {
            buttonText.text = "Close " + menuName;
        }
        else
        {
            buttonText.text = "Open " + menuName;
        }
    }
}