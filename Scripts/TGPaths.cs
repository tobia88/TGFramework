using UnityEngine;

public static class TGPaths {

    public static string Root {
        get {
            #if UNITY_EDITOR
                    return Application.dataPath + "/TGFramework/";
            #else
                    return Application.dataPath.Replace(Application.productName + "_Data", string.Empty);
            #endif
        }
    }

    public static string Configs {
        get { return Root + "Configs/"; }
    }

    public static string SceneData {
        get { return Configs + TGConfigs.SCENE_SETTING_FILENAME; }
    }

    public static string MainGameConfig {
        get { return Root + TGConfigs.MAIN_CONFIG_FILENAME; }
    }

    public static string EvalSetting {
        get { return Configs + TGConfigs.EVAL_SETTING_FILENAME; }
    }

    public static string KeyInputSetting {
        get { return Configs + TGConfigs.KEY_INPUT_CONFIG_FILENAME; }
    }

    public static string SettingData {
        get { return ProjectResources + "SettingData.asset"; }
    }

    public static string ProjectResources {
        get { return "Assets/_Project/Resources/"; }
    }

}