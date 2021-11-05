﻿
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
    private const string batchDirPref = "ImportBatchDirectory";
    private const string batchOutDirPref = "OutputBatchDirectory";

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

    private static string getBatchReadDir() {
      if (!EditorPrefs.HasKey(batchDirPref)){
        throw new System.Exception("Please set a directory to import from");
      }
      return EditorPrefs.GetString(batchDirPref);
    }

    private static string getBatchOutDir() {
      if (!EditorPrefs.HasKey(batchOutDirPref)){
        throw new System.Exception("Please set a directory to export to");
      }
      return EditorPrefs.GetString(batchOutDirPref);
    }


    [MenuItem("Tools/Set Import Directory")]
    private static void setImporterDir()
    {
        string batchReadDir = EditorUtility.OpenFolderPanel("Set the folder you wish to import from", "", "");
        EditorPrefs.SetString(batchDirPref, batchReadDir);
        Debug.Log("setting batch directory to:" +  batchReadDir);
    }

    [MenuItem("Tools/Set Build Directory")]
    private static void setBuildDir()
    {
        string batchOutDir = EditorUtility.OpenFolderPanel("Set the folder you wish to build to", "", "");
        EditorPrefs.SetString(batchOutDirPref, batchOutDir);
        Debug.Log("setting batch directory to:" + batchOutDir);

    }


    [MenuItem("Tools/Import Newest")]
    private static void ImporterNewest()
    {
        DirectoryInfo d = new DirectoryInfo(getBatchReadDir());
        System.DateTime lastDate = System.DateTime.MinValue;
        string dirToImport = null;
        string name = null;

        foreach (var item in d.GetDirectories())
        {
            Debug.Log("Directory: " + item.FullName);

            if (item.LastWriteTime > lastDate)
            {
                dirToImport = item.FullName;
                name = item.Name;
                lastDate = item.LastWriteTime;
            }           

        }

        if (dirToImport != null)
        {
            string [] keys = name.Split('_');
            ImportAsset(dirToImport, keys[1]);
        }
    }


    [MenuItem("Tools/Import Animations")]
    private static void ImportAnimations() {
       const string relativePath = "/CC_Assets/" + "Hero";
       const string assetPath = "Assets" + relativePath;

       AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath + "/hero_LODGroup_animator.controller");
       foreach (AnimationClip clip in controller.animationClips)
       {
         Debug.Log("Storing clip: " + clip.name);
         AssetDatabase.CreateAsset(AnimationClip.Instantiate(clip), "Assets/Animations/" + clip.name + ".anim");
       }
       AssetDatabase.SaveAssets();
    }

    // Import the boxer
    [MenuItem("Tools/Batch Import and Build")]
    private async static void ImporterMenuItem()
    {
        string fromPath = getBatchReadDir();
        DirectoryInfo d = new DirectoryInfo(fromPath);
        bool first = true;
        string targetRootPath = getBatchOutDir();
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
            string [] keys = item.Name.Split('_');
            ImportAsset(item.FullName, keys[1]);
            string targetDir = Path.Combine(targetRootPath, item.Name);
            BuildWebGLPlayer(targetDir);
            FileUtil.CopyFileOrDirectory(Path.Combine(fromPath, item.Name + "/Hero.jpg"), targetDir + "/Hero.jpg");
            FileUtil.CopyFileOrDirectory(Path.Combine(fromPath, item.Name + "/attrs.txt"), targetDir + "/attrs.txt");
        }
    }

    private static void ImportAsset(string assetDirectory, string classType) {
        const string relativePath = "/CC_Assets/" + "Hero";
        const string assetPath = "Assets" + relativePath;
        const string animationPath = "Assets/Animations";

        Debug.Log("Deleting:" + AssetDatabase.DeleteAsset(assetPath));

        FileUtil.CopyFileOrDirectory(assetDirectory, Application.dataPath + relativePath);
        Debug.Log("Copy done");

        AssetDatabase.Refresh();

        //AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath + "/hero_LODGroup_animator.controller");
        AnimatorController rootController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/CC_Assets/hero_LODGroup_animator.controller");
        AnimationClip idle = null, action = null;

        AnimatorControllerLayer rootLayer = rootController.layers[0];

        if (classType.ToLower() == "mage") {
          idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(animationPath + "/MageIdle.anim");
          action = AssetDatabase.LoadAssetAtPath<AnimationClip>(animationPath + "/MageAction.anim");
        } else {
          idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(animationPath + "/FighterIdle.anim");
          action = AssetDatabase.LoadAssetAtPath<AnimationClip>(animationPath + "/FighterAction.anim");
        }

        foreach (var cs in rootLayer.stateMachine.states)
        {
            AnimatorState state = cs.state;

            if (state.name == "idle" && (idle != null))
            {
                Debug.Log("setting idle");
                rootController.SetStateEffectiveMotion(state, idle);
            }
            else if (state.name == "idle2" && (idle != null))
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
