// Loads a scene into Unity. 3d Models are in assetbundles, stored in Firebase Storage
// The scene configuration data is stored in a Firestore document. 
// This script fetches the document and instantiates the scene objects.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Storage;
using Firebase.Extensions;
using UnityEngine.Networking;
using Firebase.Firestore;

[System.Serializable]
public class SceneData
{
    public string sceneName;
    public List<ObjectData> objects;
}

[System.Serializable]
public class ObjectData
{
    public string id;
    public string assetBundleName;
    public Vector3Data position;
    public Vector3Data rotation;
    public Vector3Data scale;
}

[System.Serializable]
public class Vector3Data
{
    public float x;
    public float y;
    public float z;
}

public class SceneLoader : MonoBehaviour
{
    private FirebaseStorage storage;
    private const string assetBundleBasePath = "gs://xcainventorytracker-83807.appspot.com/";

    void Start()
    {
        Debug.Log("SceneLoader started");
        storage = FirebaseStorage.DefaultInstance;
        LoadSceneData();
    }

    private void LoadSceneData()
    {
        Debug.Log("Loading scene data from Firestore");
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        DocumentReference docRef = db.Collection("scenes").Document("MySampleScene");
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to fetch scene data: " + task.Exception);
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Debug.Log("Document exists, processing data");
                Dictionary<string, object> sceneDataMap = snapshot.ToDictionary();
                Debug.Log($"Raw Firestore Data: {sceneDataMap.ToString()}");  // Debug Firestore Content

                // Initialize SceneData
                SceneData sceneData = new SceneData();

                // Process scene name
                if (sceneDataMap.ContainsKey("sceneName"))
                {
                    sceneData.sceneName = sceneDataMap["sceneName"] as string;
                    Debug.Log($"Processed Scene Name: {sceneData.sceneName}");
                }
                else
                {
                    Debug.LogError("sceneName not found in Firestore document");
                    return;
                }

                // Process objects
                if (sceneDataMap.ContainsKey("objects"))
                {
                    Dictionary<string, object> objectsMap = sceneDataMap["objects"] as Dictionary<string, object>;
                    sceneData.objects = new List<ObjectData>();

                    // Loop through each object in Firestore
                    foreach (KeyValuePair<string, object> objectEntry in objectsMap)
                    {
                        Debug.Log($"Processing Object with Key: {objectEntry.Key}");
                        Dictionary<string, object> objectDataMap = objectEntry.Value as Dictionary<string, object>;

                        // Initialize ObjectData
                        ObjectData objectData = new ObjectData();
                        objectData.id = objectEntry.Key;

                        // Assuming your Firestore has a field named 'assetBundleName'
                        if (objectDataMap.ContainsKey("assetBundleName"))
                        {
                            objectData.assetBundleName = objectDataMap["assetBundleName"] as string;
                            Debug.Log($"Processed assetBundleName: {objectData.assetBundleName}");
                        }
                        else
                        {
                            Debug.LogError($"assetBundleName not found for object {objectData.id}");
                            continue;
                        }

                        // Replace this block in your existing code
                        if (objectDataMap.ContainsKey("position") && objectDataMap.ContainsKey("rotation") && objectDataMap.ContainsKey("scale"))
                        {
                            // Process Position
                            Dictionary<string, object> positionMap = objectDataMap["position"] as Dictionary<string, object>;
                            objectData.position = new Vector3Data
                            {
                                x = Convert.ToSingle(positionMap["x"]),
                                y = Convert.ToSingle(positionMap["y"]),
                                z = Convert.ToSingle(positionMap["z"])
                            };
                            Debug.Log($"Processed Position: {objectData.position.x}, {objectData.position.y}, {objectData.position.z}");

                            // Process Rotation
                            Dictionary<string, object> rotationMap = objectDataMap["rotation"] as Dictionary<string, object>;
                            objectData.rotation = new Vector3Data
                            {
                                x = Convert.ToSingle(rotationMap["x"]),
                                y = Convert.ToSingle(rotationMap["y"]),
                                z = Convert.ToSingle(rotationMap["z"])
                            };
                            Debug.Log($"Processed Rotation: {objectData.rotation.x}, {objectData.rotation.y}, {objectData.rotation.z}");

                            // Process Scale
                            Dictionary<string, object> scaleMap = objectDataMap["scale"] as Dictionary<string, object>;
                            objectData.scale = new Vector3Data
                            {
                                x = Convert.ToSingle(scaleMap["x"]),
                                y = Convert.ToSingle(scaleMap["y"]),
                                z = Convert.ToSingle(scaleMap["z"])
                            };
                            Debug.Log($"Processed Scale: {objectData.scale.x}, {objectData.scale.y}, {objectData.scale.z}");
                        }
                        else
                        {
                            Debug.LogError("Missing position, rotation, or scale data for object");
                            continue;
                        }


                        // Add to sceneData objects
                        sceneData.objects.Add(objectData);
                    }

                    Debug.Log("Processing scene data");
                    ProcessSceneData(sceneData);
                }
                else
                {
                    Debug.LogError("objects key not found in Firestore document");
                }
            }
            else
            {
                Debug.LogError("No such document in Firestore");
            }
        });
    }




    private void ProcessSceneData(SceneData sceneData)
    {
        Debug.Log("Processing scene data");
        foreach (ObjectData objectData in sceneData.objects)
        {
            string assetBundleUrl = assetBundleBasePath + objectData.assetBundleName;
            StartCoroutine(DownloadAndInstantiateObject(objectData, assetBundleUrl));
        }
    }

    private IEnumerator DownloadAndInstantiateObject(ObjectData objectData, string assetBundleUrl)
    {
        Debug.Log("Downloading and instantiating object");
        StorageReference assetBundleRef = storage.GetReferenceFromUrl(assetBundleUrl);

        assetBundleRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to get download URL for AssetBundle: " + task.Exception);
                return;
            }

            string downloadUrl = task.Result.ToString();
            StartCoroutine(DownloadAssetBundleAndInstantiate(downloadUrl, objectData));
        });

        yield return null;
    }

    private IEnumerator DownloadAssetBundleAndInstantiate(string assetBundleUrl, ObjectData objectData)
    {
        Debug.Log("Starting DownloadAssetBundleAndInstantiate coroutine");

        // Debug: Log the assetBundleUrl
        Debug.Log($"assetBundleUrl: {assetBundleUrl}");

        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(assetBundleUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download AssetBundle: " + www.error);
                yield break;
            }

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            if (bundle == null)
            {
                Debug.LogError("AssetBundle is null");
                yield break;
            }

            // Debug: Log objectData
            Debug.Log($"objectData: {objectData}");
            if (objectData == null)
            {
                Debug.LogError("objectData is null");
                yield break;
            }

            GameObject prefab = bundle.LoadAsset<GameObject>(objectData.assetBundleName);
            if (prefab == null)
            {
                Debug.LogError("Prefab is null");
                yield break;
            }

            // Debug: Log objectData.position
            Debug.Log($"objectData.position: {objectData.position}");
            if (objectData.position == null)
            {
                Debug.LogError("objectData.position is null");
                yield break;
            }

            Vector3 position = new Vector3(objectData.position.x, objectData.position.y, objectData.position.z);
            Quaternion rotation = Quaternion.Euler(objectData.rotation.x, objectData.rotation.y, objectData.rotation.z);
            Vector3 scale = new Vector3(objectData.scale.x, objectData.scale.y, objectData.scale.z);

            GameObject go = Instantiate(prefab, position, rotation);
            go.transform.localScale = scale;

            // Debug: Log GameObject instantiation
            Debug.Log($"GameObject instantiated: {go}");

            yield return null;

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material[] materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material originalMaterial = materials[i];
                    Shader originalShader = originalMaterial.shader;
                    originalMaterial.shader = Shader.Find(originalShader.name);
                }
                renderer.materials = materials;
            }
            else
            {
                // Debug: Log if Renderer is null
                Debug.LogError("Renderer component is null");
            }
        }
    }

}
