using UnityEngine;

public class ConfigHandler : MonoBehaviour {
    private static bool VALIDATED = false;
    public KMModSettings Settings;

    void Start() {
        if (!VALIDATED) {
            if (Settings.SettingsPath.Trim() != "") {
                AllSettings set = LoadSettings();
                Settings.Settings = JsonUtility.ToJson(set, true);
                // write it
                string path = Settings.SettingsPath;
                System.IO.StreamWriter fOut = new System.IO.StreamWriter(path, false);
                fOut.Write(Settings.Settings);
                fOut.Flush();
                fOut.Close();
                VALIDATED = true;
            }
        }
    }

    public class AllSettings {
        public bool rotaryTwoDigits = false;
        public int rotarySparseness = 2;
        public bool fmnNumpad = true;
    }

    public AllSettings LoadSettings() {
        AllSettings set = new AllSettings();
        JsonUtility.FromJsonOverwrite(Settings.Settings, set);
        return set;
    }
}