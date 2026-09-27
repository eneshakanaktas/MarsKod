using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Kurulum ve paket uretimi. Menu: MarsKod > ...
// Komut satiri: Unity.exe -batchmode -quit -projectPath . -executeMethod OyunBuild.BuildWindows
public static class OyunBuild
{
    const string ScenePath = "Assets/Scenes/Oyun.unity";

    [MenuItem("MarsKod/Kurulumu yenile")]
    public static void Setup()
    {
        EnsureMaterials();
        EnsureScene();
        TuneRenderAssets();
        PlayerSettings.companyName = "MarsKod";
        PlayerSettings.productName = "MarsKod";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.marskod.oyun");
        PlayerSettings.defaultScreenWidth = 540;
        PlayerSettings.defaultScreenHeight = 1170;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
        AssetDatabase.SaveAssets();
    }

    static void EnsureMaterials()
    {
        Directory.CreateDirectory("Assets/Resources/Materials");
        Make("Lit", "Universal Render Pipeline/Lit", null);
        Make("LitEmissive", "Universal Render Pipeline/Lit", m =>
        {
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            m.SetColor("_EmissionColor", Color.white);
        });
        Make("Unlit", "Universal Render Pipeline/Unlit", null);
        Make("Backdrop", "MarsKod/Backdrop", null);
        Make("Outline", "MarsKod/Outline", null);
        Make("Ring", "MarsKod/Ring", null);
        Make("Ground", "MarsKod/Ground", null);
    }

    static void Make(string name, string shader, System.Action<Material> init)
    {
        string path = $"Assets/Resources/Materials/{name}.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;
        var s = Shader.Find(shader);
        if (s == null) throw new System.Exception("Shader bulunamadi: " + shader);
        var m = new Material(s);
        init?.Invoke(m);
        AssetDatabase.CreateAsset(m, path);
    }

    static void EnsureScene()
    {
        if (!File.Exists(ScenePath))
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
    }

    // Yumusak golge, kenar yumusatma (MSAA 4x), tek golge katmani.
    static void TuneRenderAssets()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset"))
        {
            var asset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(guid));
            var so = new SerializedObject(asset);
            Set(so, "m_MSAA", 4);
            Set(so, "m_ShadowDistance", 45f);
            Set(so, "m_ShadowCascadeCount", 1);
            Set(so, "m_MainLightShadowmapResolution", 2048);
            Set(so, "m_SoftShadowsSupported", true);
            Set(so, "m_MainLightShadowsSupported", true);
            Set(so, "m_RenderScale", 1f);
            // GPU Resident Drawer (cok nesneli sahneler icin hizlandirici) kapali: sahnemiz kucuk, bir yarari yok;
            // PC ayarinda acikken telefon paketine de giriyor ve sanal telefonda isikli nesneler hic cizilmiyordu.
            Set(so, "m_GPUResidentDrawerMode", 0);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        // Telefon (Mobile) ayari "Forward" cizimdeydi: kodla uretilen Lit malzemeler telefonda simsiyah cikiyordu.
        // Iki ayar da Forward+ (2) kullanir.
        foreach (var guid in AssetDatabase.FindAssets("t:UniversalRendererData"))
        {
            var so = new SerializedObject(AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)));
            Set(so, "m_RenderingMode", 2);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    static void Set(SerializedObject so, string prop, object value)
    {
        var p = so.FindProperty(prop);
        if (p == null) { Debug.LogWarning("Ayar yok: " + prop); return; }
        switch (value)
        {
            case int i: p.intValue = i; break;
            case float f: p.floatValue = f; break;
            case bool b: p.boolValue = b; break;
        }
    }

    [MenuItem("MarsKod/Windows paketi")]
    public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Build/Win/MarsKod.exe");

    [MenuItem("MarsKod/Android paketi (APK)")]
    public static void BuildAndroid()
    {
        EditorUserBuildSettings.buildAppBundle = false;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        Build(BuildTarget.Android, "Build/Android/marskod.apk");
    }

    // Unity paket uretirken bu dosyadan o platformun kullanmadigi cizim parcalarini siliyor ve silinmis hali kaydediyor
    // (telefon paketi bilgisayarin SSAO parcalarini siliyordu; sonraki bilgisayar paketi onlarsiz simsiyah cikiyordu).
    // Bu yuzden her uretimden sonra dosya eski haline getirilir.
    const string GlobalSettingsPath = "Assets/Settings/UniversalRenderPipelineGlobalSettings.asset";

    static void Build(BuildTarget target, string output)
    {
        // Once hedef platforma gec: telefon paketinden hemen sonraki ilk bilgisayar paketi, cizim ayarlari hala telefona gore
        // hesaplandigi icin simsiyah cikiyordu (ikinci uretim duzgundu).
        if (EditorUserBuildSettings.activeBuildTarget != target)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(target), target);
        Setup();
        string globalSettings = File.Exists(GlobalSettingsPath) ? File.ReadAllText(GlobalSettingsPath) : null;
        BuildReport report;
        try
        {
            report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = target,
                options = BuildOptions.None,
            });
        }
        finally
        {
            if (globalSettings != null && File.ReadAllText(GlobalSettingsPath) != globalSettings)
            {
                File.WriteAllText(GlobalSettingsPath, globalSettings);
                AssetDatabase.ImportAsset(GlobalSettingsPath, ImportAssetOptions.ForceUpdate);
                Debug.Log("[OyunBuild] Cizim ayar dosyasi uretimden onceki haline getirildi.");
            }
        }
        Debug.Log($"[OyunBuild] {target}: {report.summary.result}, {report.summary.totalSize / 1024 / 1024} MB");
        if (Application.isBatchMode && report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
    }
}
