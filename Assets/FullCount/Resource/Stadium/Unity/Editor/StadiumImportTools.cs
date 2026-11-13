using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Copy this Editor folder into Assets together with the FBX and Textures folder.
// This tool creates materials and an aligned prefab. It does not add gameplay or colliders.
public static class StadiumImportTools
{
    [MenuItem("Tools/Baseball Pinball/Create Stadium Prefab")]
    private static void CreateStadiumPrefab()
    {
        string file = EditorUtility.OpenFilePanel("Select BaseballPinball.fbx inside Assets", Application.dataPath, "fbx");
        if (string.IsNullOrEmpty(file)) return;
        string assets = Application.dataPath.Replace('\\', '/');
        file = file.Replace('\\', '/');
        if (!file.StartsWith(assets + "/", StringComparison.OrdinalIgnoreCase))
        {
            EditorUtility.DisplayDialog("Import model first", "Copy the FBX and Textures folder into Assets, then select that FBX.", "OK");
            return;
        }
        string path = "Assets" + file.Substring(assets.Length);
        string folder = Path.GetDirectoryName(path).Replace('\\', '/');
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (model == null) throw new InvalidOperationException("FBX has not imported yet: " + path);
        string texturePath = folder + "/Textures/Field_BaseColor.png";
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture == null) throw new FileNotFoundException("Keep Textures beside the FBX", texturePath);
        var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer != null)
        {
            importer.sRGBTexture = true;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        string materialsFolder = folder + "/Materials";
        if (!AssetDatabase.IsValidFolder(materialsFolder)) AssetDatabase.CreateFolder(folder, "Materials");
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) throw new InvalidOperationException("Install URP Lit or Standard shader before creating the prefab.");

        GameObject root = new GameObject("BaseballPinball_Stadium");
        try
        {
            var mirror = new GameObject("CoordinateHandedness").transform;
            mirror.SetParent(root.transform, false);
            var alignment = new GameObject("CoordinateAlignment").transform;
            alignment.SetParent(mirror, false);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            instance.transform.SetParent(alignment, false);
            Transform home = Find(instance.transform, "HomeAnchor");
            Transform first = Find(instance.transform, "FirstBaseAnchor");
            Transform third = Find(instance.transform, "ThirdBaseAnchor");
            Transform up = Find(instance.transform, "UpAnchor");
            if (home == null || first == null || third == null || up == null)
                throw new InvalidOperationException("The FBX alignment anchors are missing.");
            Vector3 origin = home.position;
            Vector3 x = first.position - origin;
            Vector3 z = third.position - origin;
            Vector3 y = up.position - origin;
            if (x.magnitude < 0.001f || y.magnitude < 0.001f || z.magnitude < 0.001f)
                throw new InvalidOperationException("Invalid FBX anchor distances.");
            Quaternion rotation = Quaternion.Inverse(Quaternion.LookRotation(z.normalized, y.normalized));
            float scale = 20f / x.magnitude;
            alignment.localScale = Vector3.one * scale;
            alignment.localRotation = rotation;
            alignment.localPosition = -(rotation * (origin * scale));
            if (first.position.x < 0f) mirror.localScale = new Vector3(-1f, 1f, 1f);
            AssertNear(home.position, Vector3.zero, "Home");
            AssertNear(first.position, new Vector3(20, 0, 0), "First base");
            AssertNear(third.position, new Vector3(0, 0, 20), "Third base");
            AssertNear(Find(instance.transform, "PitchAnchor").position, new Vector3(15, 0, 15), "Pitch");
            AssertNear(up.position, Vector3.up, "Up direction");

            foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
            {
                string key = renderer.gameObject.name;
                Color color;
                float smoothness = 0.64f;
                string materialName;
                if (key.StartsWith("Field_PlaySurface")) { color = Color.white; materialName = "Field_PrintedPlastic"; smoothness = .66f; }
                else if (key.StartsWith("StandBoundaryWall")) { color = new Color(.035f,.30f,.60f); materialName = "StandWall_Plastic"; smoothness = .73f; }
                else if (key.StartsWith("OutfieldFenceWall")) { color = new Color(.025f,.22f,.16f); materialName = "OutfieldFence_Plastic"; smoothness = .73f; }
                else if (key.StartsWith("DefenderPocket")) { color = new Color(.012f,.016f,.014f); materialName = "DefenderPocket_Black"; }
                else if (key.StartsWith("FoulPocket")) { color = new Color(.60f,.84f,.23f); materialName = "FoulPocket_Lime"; }
                else if (key.Contains("_HIT")) { color = new Color(.035f,.31f,.89f); materialName = "HitPocket_Blue"; }
                else if (key.Contains("_OUT")) { color = new Color(.83f,.345f,.26f); materialName = "OutPocket_Coral"; }
                else { color = new Color(.08f,.27f,.15f); materialName = "Divider_Plastic"; }
                string materialPath = materialsFolder + "/" + materialName + ".mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, materialPath); }
                mat.shader = shader;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
                bool field = key.StartsWith("Field_PlaySurface");
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", field ? texture : null);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", field ? texture : null);
                EditorUtility.SetDirty(mat);
                var materials = renderer.sharedMaterials;
                for (int i=0; i<materials.Length; i++) materials[i]=mat;
                renderer.sharedMaterials = materials;
            }
            string prefabPath = AssetDatabase.GenerateUniqueAssetPath(folder + "/BaseballPinball_Stadium.prefab");
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Debug.Log("Created " + prefabPath + ". Coordinates and scale validated. Add your own colliders and judgment/pinball components.");
            EditorUtility.DisplayDialog("Stadium ready", "Created the stadium prefab and plastic materials. No gameplay or colliders were added.", "OK");
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    private static Transform Find(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    private static void AssertNear(Vector3 actual, Vector3 expected, string label)
    {
        if (Vector3.Distance(actual, expected) > .02f)
            throw new InvalidOperationException(label + " alignment failed: " + actual + " expected " + expected);
    }
}
