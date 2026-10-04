using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace CS_Jukebox
{
    internal static class Properties
    {
        private const string ConfigName = "gamestate_integration_jukebox.cfg";
        private const string PropertiesFileName = "properties.json";
        private const string MusicKitsDirectoryName = "kits";

        public static string GameDir { get; set; } = string.Empty;
        public static int MasterVolume { get; set; }

        public static MusicKit? SelectedKit
        {
            get => selectedKit;
            set => SetKit(value);
        }

        public static List<MusicKit> MusicKits { get; private set; } = new();

        private static MusicKit? selectedKit;
        private static string? selectedKitName;

        public static void Load()
        {
            LoadProperties();
            LoadKits();
        }

        public static void Save()
        {
            SaveProperties();
            SaveKits();
        }

        public static string GetConfigDirectory()
        {
            if (string.IsNullOrWhiteSpace(GameDir))
                return string.Empty;

            var gameCsgoCfg = Path.Combine(GameDir, "game", "csgo", "cfg");
            if (Directory.Exists(gameCsgoCfg))
                return gameCsgoCfg;

            var legacyCsgoCfg = Path.Combine(GameDir, "csgo", "cfg");
            if (Directory.Exists(legacyCsgoCfg))
                return legacyCsgoCfg;

            return gameCsgoCfg;
        }

        public static void CreateConfig()
        {
            if (string.IsNullOrWhiteSpace(GameDir))
                return;

            var cfgDir = GetConfigDirectory();
            Directory.CreateDirectory(cfgDir);

            var configSrc = Path.Combine(AppContext.BaseDirectory, ConfigName);
            var configDest = Path.Combine(cfgDir, ConfigName);

            if (File.Exists(configDest))
                File.Delete(configDest);

            File.Copy(configSrc, configDest);
        }

        public static void SaveProperties()
        {
            var propertiesPath = Path.Combine(AppContext.BaseDirectory, PropertiesFileName);
            var properties = new PropertiesFile
            {
                GameDir = GameDir,
                SelectedKitName = selectedKitName,
                MasterVolume = MasterVolume
            };

            File.WriteAllText(propertiesPath, JsonConvert.SerializeObject(properties));
        }

        public static void LoadProperties()
        {
            var propertiesPath = Path.Combine(AppContext.BaseDirectory, PropertiesFileName);
            if (!File.Exists(propertiesPath))
                return;

            var json = File.ReadAllText(propertiesPath);
            var properties = JsonConvert.DeserializeObject<PropertiesFile>(json);

            if (properties == null)
                return;

            GameDir = properties.GameDir ?? string.Empty;
            selectedKitName = properties.SelectedKitName;
            MasterVolume = properties.MasterVolume;
        }

        public static void SaveKits()
        {
            var kitDirectory = Path.Combine(AppContext.BaseDirectory, MusicKitsDirectoryName);
            Directory.CreateDirectory(kitDirectory);

            foreach (var musicKit in MusicKits)
            {
                var kitPath = Path.Combine(kitDirectory, $"{musicKit.Name}.json");
                File.WriteAllText(kitPath, JsonConvert.SerializeObject(musicKit));
            }
        }

        public static void LoadKits()
        {
            var kitDirectory = Path.Combine(AppContext.BaseDirectory, MusicKitsDirectoryName);
            MusicKits = new List<MusicKit>();

            if (!Directory.Exists(kitDirectory))
            {
                Directory.CreateDirectory(kitDirectory);
                return;
            }

            foreach (var filePath in Directory.GetFiles(kitDirectory, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var kit = JsonConvert.DeserializeObject<MusicKit>(json);
                    if (kit != null)
                        MusicKits.Add(kit);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception when trying to load music kit '{filePath}': {ex.Message}");
                }
            }

            if (MusicKits.Count == 0)
                return;

            var selectedMatch = MusicKits.FirstOrDefault(k => string.Equals(k.Name, selectedKitName, StringComparison.Ordinal));
            if (selectedMatch != null)
            {
                SelectedKit = selectedMatch;
            }
            else
            {
                SelectedKit = MusicKits[0];
            }
        }

        public static void DeleteKitFile(string kitName)
        {
            var kitDirectory = Path.Combine(AppContext.BaseDirectory, MusicKitsDirectoryName);
            var kitPath = Path.Combine(kitDirectory, $"{kitName}.json");
            if (File.Exists(kitPath))
                File.Delete(kitPath);
        }

        private static void SetKit(MusicKit? newKit)
        {
            selectedKit = newKit;
            selectedKitName = newKit?.Name;
        }

        private sealed class PropertiesFile
        {
            public string? GameDir { get; set; }
            public string? SelectedKitName { get; set; }
            public int MasterVolume { get; set; }
        }
    }
}
