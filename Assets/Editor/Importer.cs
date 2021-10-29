
using UnityEngine;
using UnityEditor;
using AnimatorController = UnityEditor.Animations.AnimatorController;
using AnimatorControllerLayer = UnityEditor.Animations.AnimatorControllerLayer;
using AnimatorState = UnityEditor.Animations.AnimatorState;
using System.Linq;
using System.IO;
using UnityEditor.Build.Reporting;
using System.Threading.Tasks;

public class Importer
{
    [MenuItem("Tools/Texture Copy Alpha")]
    private static void TextureMenuItem()
    {
        const string mainSubPath = "/CC_Assets/Hero/hero_LODGroup.fbm/remesh_10_combined_Remeshed_LOD1_Pbr_Diffuse.png";
        const string alphaSubPath = "/CC_Assets/Hero/textures/hero_LODGroup/remesh_10_combined_Remeshed_LOD1_Pbr_MetallicAlpha.png";
        const string mainPath = "Assets" + mainSubPath;
        const string alphaPath = "Assets" + alphaSubPath;

        TextureImporter mainImporter = TextureImporter.GetAtPath(mainPath) as TextureImporter;
        TextureImporter alphaImporter = TextureImporter.GetAtPath(alphaPath) as TextureImporter;

        //mainImporter.SetTextureSettings(TextureImporterSettings);

        mainImporter.isReadable = true;
        alphaImporter.isReadable = true;

        AssetDatabase.WriteImportSettingsIfDirty(mainImporter.assetPath);
        AssetDatabase.WriteImportSettingsIfDirty(alphaImporter.assetPath);
        AssetDatabase.Refresh();
        Texture2D main = AssetDatabase.LoadAssetAtPath<Texture2D>(mainPath);
        Texture2D alpha = AssetDatabase.LoadAssetAtPath<Texture2D>(alphaPath);

        Debug.Log("Main Transparency: " + main.alphaIsTransparency);
        Debug.Log("Main Format: " + main.format);
        Debug.Log("Main Res:" + main.dimension);
        Debug.Log("Main Res:" + main.height + "," + main.width);

        Debug.Log("Transparency: " + alpha.alphaIsTransparency);
        Debug.Log("Format: " + alpha.format);
        Debug.Log("Res:" + alpha.height + "," + alpha.width);
        Debug.Log("mips? " + main.mipmapCount);

        var outTexture = new Texture2D(main.width, main.height, TextureFormat.RGBA32, true);

        outTexture.alphaIsTransparency = true;
        Debug.Log("We has transparency? " + outTexture.alphaIsTransparency);


        var mainColorData = main.GetPixels32();
        var alphaColorData = alpha.GetPixels32();

        Debug.Log("Mip length check:" + mainColorData.Length + "," + alphaColorData.Length);

        for (int i = 0; i < mainColorData.Length; i++)
        {
            mainColorData[i].a = alphaColorData[i].r;
        }

        outTexture.SetPixels32(mainColorData);

        outTexture.Apply();


        byte[] bytes = outTexture.EncodeToPNG();

        File.WriteAllBytes(Application.dataPath + "/" + mainSubPath, bytes);
        Object.DestroyImmediate(outTexture);

        AssetDatabase.Refresh();

        //var mip0Data = m_Texture2D.GetPixelData<Color32>;


    }

    [MenuItem("Tools/Import Newest")]
    private static void ImporterNewest()
    {
        DirectoryInfo d = new DirectoryInfo("D:/CharacterGen/Batch/");
        System.DateTime lastDate = System.DateTime.MinValue;
        string dirToImport = null;

        foreach (var item in d.GetDirectories())
        {
            Debug.Log("Directory: " + item.FullName);

            if (item.LastWriteTime > lastDate)
            {
                dirToImport = item.FullName;
                lastDate = item.LastWriteTime;
            }           
 

        }

        if (dirToImport != null)
        {
            ImportAsset(dirToImport);
        }
    }

    // Import the boxer
    [MenuItem("Tools/Batch Import and Build")]
    private async static void ImporterMenuItem()
    {
        DirectoryInfo d = new DirectoryInfo("D:/CharacterGen/Batch/");
        bool first = true;
        foreach (var item in d.GetDirectories())
        {
            Debug.Log("Directory: " + item.FullName);
            if (first)
            {
                first = false;
            }
            else
            {
                await Task.Delay(3000);
            }
            ImportAsset(item.FullName);
            BuildWebGLPlayer("D:/CharacterGen/BatchOut/" + item.Name);
 
        }
    }

    private static void ImportAsset(string assetDirectory) {
        const string relativePath = "/CC_Assets/" + "Hero";
        const string assetPath = "Assets" + relativePath;

        Debug.Log("Deleting:" + AssetDatabase.DeleteAsset(assetPath));

        FileUtil.CopyFileOrDirectory(assetDirectory, Application.dataPath + relativePath);
        Debug.Log("Copy done");

        AssetDatabase.Refresh();

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath + "/hero_LODGroup_animator.controller");
        AnimatorController rootController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/CC_Assets/hero_LODGroup_animator.controller");

        AnimatorControllerLayer rootLayer = rootController.layers[0];

        AnimationClip idle = null, action = null;

        foreach (AnimationClip clip in controller.animationClips)
        {
            Debug.Log("found clip: " + clip.name);
            if (clip.name == "Idle_Battle" || clip.name == "F_LS_Warning_Idle")
            {
                Debug.Log("Found idle");
                idle = clip;
            }
            else if (clip.name == "Atk_2xCombo02" || clip.name == "M_LS_MageSpellCast_05")
            {
                Debug.Log("Found action");
                action = clip;
            }
        }

        foreach (var cs in rootLayer.stateMachine.states)
        {
            AnimatorState state = cs.state;

            if (state.name == "idle" && (idle != null))
            {
                Debug.Log("setting idle");
                rootController.SetStateEffectiveMotion(state, idle);
            }
            else if (state.name == "action" && (action != null))
            {
                Debug.Log("setting action");
                rootController.SetStateEffectiveMotion(state, action);
            }
        }

        EditorUtility.SetDirty(rootController);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        //AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ImportRecursive);
    }


    private static void BuildWebGLPlayer(string outDirName)
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.locationPathName = outDirName;
        buildPlayerOptions.target = BuildTarget.WebGL;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }
}
