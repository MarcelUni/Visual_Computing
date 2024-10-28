using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MeshCombiner
{
    [MenuItem("Assets/Combine Meshes in Prefab")]
    static void CombineMeshesInPrefab()
    {
        // Get the selected object in the Project window
        Object selectedObject = Selection.activeObject;
        if (selectedObject == null || !(selectedObject is GameObject))
        {
            EditorUtility.DisplayDialog("No Prefab Selected", "Please select a prefab in the Project view.", "OK");
            return;
        }

        // Check if the selected object is a prefab
        GameObject prefab = selectedObject as GameObject;
        string assetPath = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrEmpty(assetPath))
        {
            EditorUtility.DisplayDialog("Invalid Selection", "Please select a prefab in the Project view.", "OK");
            return;
        }

        // Instantiate the prefab
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        // Get all MeshFilters and SkinnedMeshRenderers in the prefab
        MeshFilter[] meshFilters = instance.GetComponentsInChildren<MeshFilter>();
        SkinnedMeshRenderer[] skinnedMeshRenderers = instance.GetComponentsInChildren<SkinnedMeshRenderer>();

        // Dictionary to map materials to their corresponding CombineInstances
        Dictionary<Material, List<CombineInstance>> materialToCombineInstances = new Dictionary<Material, List<CombineInstance>>();

        // Process MeshFilters
        foreach (MeshFilter meshFilter in meshFilters)
        {
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
                continue;

            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null)
                continue;

            Material[] materials = meshRenderer.sharedMaterials;

            for (int s = 0; s < mesh.subMeshCount; s++)
            {
                if (s >= materials.Length)
                {
                    Debug.LogWarning("Mesh submesh count is greater than materials count. Skipping submesh.");
                    continue;
                }

                Material material = materials[s];

                CombineInstance combineInstance = new CombineInstance();
                combineInstance.mesh = mesh;
                combineInstance.subMeshIndex = s;
                combineInstance.transform = meshRenderer.transform.localToWorldMatrix;

                if (materialToCombineInstances.ContainsKey(material))
                {
                    materialToCombineInstances[material].Add(combineInstance);
                }
                else
                {
                    materialToCombineInstances[material] = new List<CombineInstance>() { combineInstance };
                }
            }
        }

        // Process SkinnedMeshRenderers
        foreach (SkinnedMeshRenderer skinnedRenderer in skinnedMeshRenderers)
        {
            Mesh mesh = new Mesh();
            skinnedRenderer.BakeMesh(mesh);

            if (mesh == null)
                continue;

            Material[] materials = skinnedRenderer.sharedMaterials;

            for (int s = 0; s < mesh.subMeshCount; s++)
            {
                if (s >= materials.Length)
                {
                    Debug.LogWarning("Mesh submesh count is greater than materials count. Skipping submesh.");
                    continue;
                }

                Material material = materials[s];

                CombineInstance combineInstance = new CombineInstance();
                combineInstance.mesh = mesh;
                combineInstance.subMeshIndex = s;
                combineInstance.transform = skinnedRenderer.transform.localToWorldMatrix;

                if (materialToCombineInstances.ContainsKey(material))
                {
                    materialToCombineInstances[material].Add(combineInstance);
                }
                else
                {
                    materialToCombineInstances[material] = new List<CombineInstance>() { combineInstance };
                }
            }
        }

        // Prepare the final list of CombineInstances and materials
        List<CombineInstance> finalCombine = new List<CombineInstance>();
        List<Material> materialsList = new List<Material>();

        foreach (KeyValuePair<Material, List<CombineInstance>> kvp in materialToCombineInstances)
        {
            CombineInstance ci = new CombineInstance();
            Mesh mesh = new Mesh();

            // Set index format to 32-bit to support large meshes
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            mesh.CombineMeshes(kvp.Value.ToArray(), true, true);
            ci.mesh = mesh;
            ci.subMeshIndex = 0;
            ci.transform = Matrix4x4.identity;
            finalCombine.Add(ci);
            materialsList.Add(kvp.Key);
        }

        // Combine all meshes into one mesh with multiple submeshes
        Mesh finalMesh = new Mesh();

        // Set index format to 32-bit to support large meshes
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        finalMesh.CombineMeshes(finalCombine.ToArray(), false, false);

        // Prompt the user to save the combined mesh asset
        string savePath = EditorUtility.SaveFilePanelInProject("Save Combined Mesh", prefab.name + "_Combined", "asset", "Please enter a file name to save the combined mesh to");

        if (!string.IsNullOrEmpty(savePath))
        {
            // Save the combined mesh as an asset
            AssetDatabase.CreateAsset(finalMesh, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Create a new GameObject with the combined mesh
            GameObject combinedObject = new GameObject(prefab.name + "_Combined");
            MeshFilter mf = combinedObject.AddComponent<MeshFilter>();
            mf.sharedMesh = finalMesh;
            MeshRenderer mr = combinedObject.AddComponent<MeshRenderer>();
            mr.sharedMaterials = materialsList.ToArray();

            // Save the combined GameObject as a new prefab
            string prefabSavePath = savePath.Replace(".asset", ".prefab");
            PrefabUtility.SaveAsPrefabAsset(combinedObject, prefabSavePath);

            // Clean up
            GameObject.DestroyImmediate(combinedObject);
            foreach (CombineInstance ci in finalCombine)
            {
                GameObject.DestroyImmediate(ci.mesh);
            }

            GameObject.DestroyImmediate(instance);

            EditorUtility.DisplayDialog("Success", "Combined mesh saved to " + savePath, "OK");
        }
        else
        {
            // Clean up
            foreach (CombineInstance ci in finalCombine)
            {
                GameObject.DestroyImmediate(ci.mesh);
            }
            GameObject.DestroyImmediate(instance);
            EditorUtility.DisplayDialog("Cancelled", "Mesh combining cancelled.", "OK");
        }
    }
}
