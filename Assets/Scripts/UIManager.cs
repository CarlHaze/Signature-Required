using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public UIDocument mainMenu; // Reference to the MainMenu UI Document

    private void Start()
    {
        // Ensure the main menu is initially hidden
        mainMenu.rootVisualElement.style.display = DisplayStyle.None;
    }

    private void Update()
    {
        // Check if the Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Toggle the visibility of the main menu
            if (mainMenu.rootVisualElement.style.display == DisplayStyle.None)
            {
                mainMenu.rootVisualElement.style.display = DisplayStyle.Flex;
            }
            else
            {
                mainMenu.rootVisualElement.style.display = DisplayStyle.None;
            }
        }
    }
}
