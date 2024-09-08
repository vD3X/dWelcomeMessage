using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static dWelcomeMessage.dWelcomeMessage;

namespace dWelcomeMessage
{
    public static class Config
    {
        private static readonly string configPath = Path.Combine(Instance.ModuleDirectory, "Config.json");
        public static ConfigModel config;
        private static FileSystemWatcher fileWatcher;

        public static void Initialize()
        {
            if (!File.Exists(configPath))
            {
                Instance.Logger.LogInformation("Plik konfiguracyjny nie istnieje. Tworzenie nowego pliku z domyślną konfiguracją.");
                CreateDefaultConfig();
            }

            config = LoadConfig();

            SetupFileWatcher();
        }

        private static void CreateDefaultConfig()
        {
            var defaultConfig = new ConfigModel
            {
                Groups = new Dictionary<string, Messages>
                {
                    { "VIP", new Messages
                        {
                            PermissionRequired = "@css/vip",
                            Welcome_Message = "{YELLOW}★VIP★ {GREEN}{PLAYERNAME} {DEFAULT}({LIME}{STEAMID}{DEFAULT}) wyszedł z serwer!",
                            Goodbye_Message = "{YELLOW}★VIP★ {GREEN}{PLAYERNAME} {DEFAULT}({LIME}{STEAMID}{DEFAULT}) wyszedł z serwer!"
                        }
                    },
                    { "SVIP", new Messages
                        {
                            PermissionRequired = "@css/svip",
                            Welcome_Message = "{YELLOW}★SVIP★ {GREEN}{PLAYERNAME} {DEFAULT}({LIME}{STEAMID}{DEFAULT}) wbija na serwer!",
                            Goodbye_Message = "{YELLOW}★SVIP★ {GREEN}{PLAYERNAME} {DEFAULT}({LIME}{STEAMID}{DEFAULT}) wyszedł z serwer!"
                        }
                    }
                }
            };

            SaveConfig(defaultConfig);
        }

        private static ConfigModel LoadConfig()
        {
            try
            {
                string json = File.ReadAllText(configPath);
                var loadedConfig = JsonConvert.DeserializeObject<ConfigModel>(json);

                if (loadedConfig == null || loadedConfig.Groups == null || !loadedConfig.Groups.Any())
                {
                    Instance.Logger.LogError("Plik konfiguracyjny jest pusty lub ma błędną strukturę.");
                    return null;
                }

                Instance.Logger.LogInformation("Konfiguracja została załadowana poprawnie.");
                return loadedConfig;
            }
            catch (Exception ex)
            {
                Instance.Logger.LogError($"Błąd podczas wczytywania pliku konfiguracyjnego: {ex.Message}");
                return null;
            }
        }

        public static void SaveConfig(ConfigModel config)
        {
            try
            {
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configPath, json);
                Instance.Logger.LogInformation("Plik konfiguracyjny został zapisany.");
            }
            catch (Exception ex)
            {
                Instance.Logger.LogError($"Błąd podczas zapisywania pliku konfiguracyjnego: {ex.Message}");
            }
        }

        private static void SetupFileWatcher()
        {
            fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(configPath))
            {
                Filter = Path.GetFileName(configPath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };

            fileWatcher.Changed += (sender, e) => {
                Thread.Sleep(500);
                var newConfig = LoadConfig();
                if (newConfig != null)
                {
                    config = newConfig;
                    Instance.Logger.LogInformation("Konfiguracja została zaktualizowana po zmianie pliku.");
                }
            };

            fileWatcher.EnableRaisingEvents = true;
        }

        public class ConfigModel
        {
            public Settings Settings { get; set; } = new Settings();
            public Dictionary<string, Messages> Groups { get; set; } = new Dictionary<string, Messages>();
        }

        public class Settings
        {
            public string Prefix { get; set; } = "[{DARKRED}Połączenie{DEFAULT}]";
            public bool Default_Message_Enabled { get; set; } = true;
            public string Default_Welcome_Message { get; set; } = "{GREEN}{PLAYERNAME} {DEFAULT}({LIME}{STEAMID}{DEFAULT}) wbija na serwer!";
            public string Default_Goodbye_Message { get; set; } = "{GREEN}{PLAYERNAME} {DEFAULT}({LIME}{STEAMID}{DEFAULT}) wyszedł z serwer!";
        }

        public class Messages
        {
            public string PermissionRequired { get; set; }
            public string Welcome_Message { get; set; }
            public string Goodbye_Message { get; set; }
        }
    }
}