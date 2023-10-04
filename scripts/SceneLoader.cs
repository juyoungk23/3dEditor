// Loads a scene into unity. The 3d Models are in assetbundles, stored in Firebase Storage
// The scene configuration data is also stored in a JSON file in Firebase Storage

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Storage;
using Firebase.Extensions;
using UnityEngine.Networking;

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
    private const string sceneJsonPath = "gs://xcainventorytracker-83807.appspot.com/YourScene.json";
    private const string assetBundleBasePath = "gs://xcainventorytracker-83807.appspot.com/"; // Assuming AssetBundles are also on Firestore

    void Start()
    {
        storage = FirebaseStorage.DefaultInstance;
        LoadSceneData();
    }

    private void LoadSceneData()
    {
        // Get the storage reference for scene JSON
        StorageReference sceneJsonRef = storage.GetReferenceFromUrl(sceneJsonPath);

        // Fetch scene JSON
        sceneJsonRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to get download URL: " + task.Exception);
                return;
            }

            string downloadUrl = task.Result.ToString();
            StartCoroutine(DownloadAndProcessScene(downloadUrl));
        });
    }

    private IEnumerator DownloadAndProcessScene(string downloadUrl)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(downloadUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download scene data: " + www.error);
                yield break;
            }

            // Deserialize JSON to SceneData object
            string jsonContent = www.downloadHandler.text;
            SceneData sceneData = JsonUtility.FromJson<SceneData>(jsonContent);


            // Process the scene data
            ProcessSceneData(sceneData);
        }
    }

    private void ProcessSceneData(SceneData sceneData)
    {
        foreach (ObjectData objectData in sceneData.objects)
        {
            string assetBundleUrl = assetBundleBasePath + objectData.assetBundleName; // Assuming URL based on assetBundleName
            StartCoroutine(DownloadAndInstantiateObject(objectData, assetBundleUrl));
        }
    }

    private IEnumerator DownloadAndInstantiateObject(ObjectData objectData, string assetBundleUrl)
    {
        // Get the storage reference for AssetBundle
        StorageReference assetBundleRef = storage.GetReferenceFromUrl(assetBundleUrl);

        // Fetch AssetBundle
        assetBundleRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to get download URL for AssetBundle: " + task.Exception);
                return;
            }

            string downloadUrl = task.Result.ToString();

            // Download AssetBundle and instantiate
            StartCoroutine(DownloadAssetBundleAndInstantiate(downloadUrl, objectData));
        });

        yield return null;
    }

    private IEnumerator DownloadAssetBundleAndInstantiate(string assetBundleUrl, ObjectData objectData)
    {
        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(assetBundleUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download AssetBundle: " + www.error);
                yield break;
            }

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            GameObject prefab = bundle.LoadAsset<GameObject>(objectData.assetBundleName);

            Vector3 position = new Vector3(objectData.position.x, objectData.position.y, objectData.position.z);
            Quaternion rotation = Quaternion.Euler(objectData.rotation.x, objectData.rotation.y, objectData.rotation.z);
            Vector3 scale = new Vector3(objectData.scale.x, objectData.scale.y, objectData.scale.z);

            GameObject go = Instantiate(prefab, position, rotation);
            go.transform.localScale = scale;

            // Delay for a frame to allow Unity to catch up
            yield return null;

            // Reapply materials and shaders
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
                renderer.materials = materials; // Reapply materials
            }
        }
    }


}
