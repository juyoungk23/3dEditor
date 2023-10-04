// A script that can be attached to a GameObject with a Box Collider to allow it to be dragged around the scene.
// Issues: Rotation is not intuitive, and resizing is not smooth.

using UnityEngine;

public class ObjectDragger : MonoBehaviour
{
    private GameObject selectedObject;
    private Vector3 offset;
    private Camera mainCamera;

    // For resizing
    private Vector3 initialScale;
    private Vector3 initialMousePosition;

    // For rotating
    private Quaternion initialRotation;
    private float rotationSpeed = 1000.0f; // Increased sensitivity

    void Start()
    {
        // Cache the main camera
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Handle mouse down
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            // Check if the ray hits a GameObject with a Box Collider
            if (Physics.Raycast(ray, out hit) && hit.collider is BoxCollider)
            {
                selectedObject = hit.collider.gameObject;

                // Calculate the offset between the mouse position and the GameObject position
                offset = selectedObject.transform.position - hit.point;

                // Save initial scale and mouse position for resizing
                initialScale = selectedObject.transform.localScale;
                initialMousePosition = Input.mousePosition;

                // Save initial rotation for rotating
                initialRotation = selectedObject.transform.rotation;
            }
        }

        // Handle mouse up
        if (Input.GetMouseButtonUp(0))
        {
            // Release the selected GameObject
            selectedObject = null;
        }

        // Handle mouse drag
        if (selectedObject != null)
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            // Cast a ray from the camera to the mouse position
            if (Physics.Raycast(ray, out hit))
            {
                // If Alt is held down, resize the object
                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                {
                    Vector3 mouseDelta = Input.mousePosition - initialMousePosition;
                    float resizeFactor = 1.0f + mouseDelta.y * 0.01f;
                    selectedObject.transform.localScale = initialScale * resizeFactor;
                }
                // If Ctrl is held down, rotate the object
                else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    float rotationAmountX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
                    float rotationAmountY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
                    selectedObject.transform.Rotate(Vector3.up, -rotationAmountX);
                    selectedObject.transform.Rotate(Vector3.right, -rotationAmountY); // Negative for intuitive control
                }
                // Else, move the object
                else
                {
                    selectedObject.transform.position = hit.point + offset;
                }
            }
        }
    }
}
