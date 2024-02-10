﻿using System.Globalization;
using System.IO;
using System.IO.Compression;
using RimworldTogether.GameClient.Files;
using RimworldTogether.GameClient.Managers;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.Serializers;
using UnityEngine;
using Verse;

namespace RimworldTogether.GameClient.Core
{
    public class Main
    {
        public static UnityMainThreadDispatcher threadDispatcher;
        public static Master master = new Master();
        public static ModConfigs modConfigs = new ModConfigs();

        public static string mainPath;

        public static string modPath;

        public static string connectionDataPath;

        public static string loginDataPath;

        public static string clientPreferencesPath;

        public static string savesPath;

        [StaticConstructorOnStartup]
        public static class RimworldTogether
        {
            static RimworldTogether() 
            {
                PrepareCulture();
                PreparePaths();
                LoadClientPreferences();
                CreateUnityDispatcher();

                //ZipFile.ExtractToDirectory(Path.Combine(GenFilePaths.ModsFolderPath, "Required.zip"), GenFilePaths.ModsFolderPath);
                //File.Delete(Path.Combine(GenFilePaths.ModsFolderPath, "Required.zip"));
            }

            private static void PrepareCulture()
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
                CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US", false);
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US", false);
            }

            private static void PreparePaths()
            {
                mainPath = GenFilePaths.SaveDataFolderPath;
                modPath = Path.Combine(mainPath, "Rimworld Together");
                connectionDataPath = Path.Combine(modPath, "ConnectionData.json");
                loginDataPath = Path.Combine(modPath, "LoginData.json");
                clientPreferencesPath = Path.Combine(modPath, "Preferences.json");
                savesPath = GenFilePaths.SavedGamesFolderPath;

                if (!Directory.Exists(modPath)) Directory.CreateDirectory(modPath);
            }

            private static void LoadClientPreferences()
            {
                ClientPreferencesFile newPreferences;

                if (File.Exists(clientPreferencesPath))
                {
                    newPreferences = Serializer.SerializeFromFile<ClientPreferencesFile>(clientPreferencesPath);
                    ClientValues.autosaveDays = int.Parse(newPreferences.AutosaveInterval);
                    ClientValues.autosaveInternalTicks = Mathf.RoundToInt(ClientValues.autosaveDays * 60000f);
                }

                else
                {
                    ClientValues.autosaveDays = 3;
                    ClientValues.autosaveInternalTicks = Mathf.RoundToInt(ClientValues.autosaveDays * 60000f);

                    PreferenceManager.SaveClientPreferences(ClientValues.autosaveDays.ToString());
                }
            }

            private static void CreateUnityDispatcher()
            {
                if (threadDispatcher == null)
                {
                    GameObject go = new GameObject("Dispatcher");
                    threadDispatcher = go.AddComponent(typeof(UnityMainThreadDispatcher)) as UnityMainThreadDispatcher;
                    Object.Instantiate(go);

                    Log.Message($"[Rimworld Together] > Created dispatcher for version {ClientValues.versionCode}");
                }
            }
        }
    }
}