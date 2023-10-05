using UnityEngine;
using TMPro;  // Add this for TextMeshPro
using UnityEngine.UI;  // For Unity's UI elements
using TinyGiantStudio.Text;
public class FontSwitcher : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Button switchFontButton;  // Fully qualify the type name
    [SerializeField]
    private string fontsPath = "MyCustomFonts";  // Relative path inside Resources folder

    private int currentIndex = 0;
    private TinyGiantStudio.Text.Font[] fonts;  // Fully qualify the type name

    void Start()
    {
        // Attach listener to button's OnClick event
        if (switchFontButton != null)
        {
            switchFontButton.onClick.AddListener(SwitchFont);
        }
        else
        {
            Debug.LogError("SwitchFontButton is not set. Please assign it in the Inspector.");
        }

        // Load all custom font files from the specified directory inside Resources folder
        fonts = Resources.LoadAll<TinyGiantStudio.Text.Font>(fontsPath);  // Fully qualify the type name

        // If no fonts are found, log a warning
        if (fonts.Length == 0)
        {
            Debug.LogWarning("No fonts found in directory: " + fontsPath);
            return;
        }

        Debug.Log("Loaded " + fonts.Length + " fonts.");
    }

    // Call this function when the button is clicked
    public void SwitchFont()
    {
        Debug.Log("SwitchFont called");

        // Check what object is currently selected in SelectedObjectTracker
        GameObject selectedObject = SelectedObjectTracker.selectedObject;

        // If no object is selected, return or print a message
        if (selectedObject == null)
        {
            Debug.LogWarning("No object selected.");
            return;
        }

        // Obtain the Modular3DText component of the selected object
        Modular3DText modular3DText = selectedObject.GetComponent<Modular3DText>();

        // If the selected object does not have a Modular3DText component, return or print a message
        if (modular3DText == null)
        {
            Debug.LogWarning("The selected object does not have a Modular3DText component.");
            return;
        }

        // Increment and loop around the index
        currentIndex++;
        if (currentIndex >= fonts.Length)
        {
            currentIndex = 0;
        }

        // Change the font of the selected object's Modular3DText component
        modular3DText.Font = fonts[currentIndex];
        Debug.Log("Switching Font to: " + fonts[currentIndex].name);
    }
}
