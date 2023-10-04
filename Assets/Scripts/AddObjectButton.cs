using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AddObjectButton : MonoBehaviour
{
    public SceneLoader sceneLoader;  // Reference to your SceneLoader script
    public Button addCubeButton;  // Reference to your UI Button for adding a cube
    public Button addSphereButton;  // Reference to your UI Button for adding a sphere
    public Button addGearButton;  // Reference to your UI Button for adding a gear

    // Object Data for Cube
    private ObjectData cubeData = new ObjectData
    {
        id = "newCube",
        assetBundleName = "cube",
        position = new Vector3Data { x = 0, y = 1, z = 0 },
        rotation = new Vector3Data { x = 0, y = 0, z = 0 },
        scale = new Vector3Data { x = 1, y = 1, z = 1 }
    };

    // Object Data for Sphere
    private ObjectData sphereData = new ObjectData
    {
        id = "newSphere",
        assetBundleName = "sphere",
        position = new Vector3Data { x = 2, y = 1, z = 0 },
        rotation = new Vector3Data { x = 0, y = 0, z = 0 },
        scale = new Vector3Data { x = 1, y = 1, z = 1 }
    };

    // Object Data for Gear
    private ObjectData gearData = new ObjectData
    {
        id = "newGear",
        assetBundleName = "gear",
        position = new Vector3Data { x = 4, y = 1, z = 0 },
        rotation = new Vector3Data { x = 0, y = 0, z = 0 },
        scale = new Vector3Data { x = 1, y = 1, z = 1 }
    };

    void Start()
    {
        // Attach the OnClick behavior for each button
        addCubeButton.onClick.AddListener(OnAddCubeButtonClick);
        addSphereButton.onClick.AddListener(OnAddSphereButtonClick);
        addGearButton.onClick.AddListener(OnAddGearButtonClick);
    }

    public void OnAddCubeButtonClick()
    {
        Debug.Log("OnAddCubeButtonClick called");

        // Get the assetBundleUrl for the cube
        string assetBundleUrl = SceneLoader.assetBundleBasePath + cubeData.assetBundleName;

        // Call the Coroutine to download and instantiate the cube
        StartCoroutine(sceneLoader.DownloadAndInstantiateObject(cubeData, assetBundleUrl));
    }

    public void OnAddSphereButtonClick()
    {
        Debug.Log("OnAddSphereButtonClick called");

        // Get the assetBundleUrl for the sphere
        string assetBundleUrl = SceneLoader.assetBundleBasePath + sphereData.assetBundleName;

        // Call the Coroutine to download and instantiate the sphere
        StartCoroutine(sceneLoader.DownloadAndInstantiateObject(sphereData, assetBundleUrl));
    }

    public void OnAddGearButtonClick()
    {
        Debug.Log("OnAddGearButtonClick called");

        // Get the assetBundleUrl for the gear
        string assetBundleUrl = SceneLoader.assetBundleBasePath + gearData.assetBundleName;

        // Call the Coroutine to download and instantiate the gear
        StartCoroutine(sceneLoader.DownloadAndInstantiateObject(gearData, assetBundleUrl));
    }
}
