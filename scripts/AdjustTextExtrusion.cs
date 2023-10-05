using UnityEngine;
using TinyGiantStudio.Text;

public class AdjustTextExtrusion : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Slider extrusionSlider; // Fully qualified to avoid ambiguity

    [SerializeField]
    private ObjectInfoDisplay objectInfoDisplay; // Drag your ObjectInfoDisplay script here

    // Start is called before the first frame update
    void Start()
    {
        if (extrusionSlider != null && objectInfoDisplay != null)
        {
            extrusionSlider.onValueChanged.AddListener(UpdateExtrusion);
        }
        else
        {
            Debug.LogError("Either extrusionSlider or objectInfoDisplay is not set. Please assign them in the Inspector.");
        }
    }

    private void UpdateExtrusion(float value)
    {
        GameObject selectedObject = objectInfoDisplay.selectedObject;
        if (selectedObject == null) return;

        Modular3DText textObject = selectedObject.GetComponent<Modular3DText>();
        if (textObject == null) return;

        Vector3 currentFontSize = textObject.FontSize; // Changed from .Size to .FontSize
        currentFontSize.z = value; // Update the Z field (extrusion depth) with the slider value
        textObject.FontSize = currentFontSize; // Update FontSize, which will invoke SetTextDirty() internally
    }

    // Optionally, you can unsubscribe the listener when the object is destroyed
    private void OnDestroy()
    {
        if (extrusionSlider != null)
        {
            extrusionSlider.onValueChanged.RemoveListener(UpdateExtrusion);
        }
    }
}
