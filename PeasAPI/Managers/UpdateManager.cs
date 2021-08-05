using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using Reactor;
using Reactor.Extensions;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PeasAPI.Managers
{
    public class UpdateManager
    {
        private static List<UpdateListener> _updateListeners = new();

        public static void RegisterUpdateListener(string dataUrl)
        {
            var listener = new UpdateListener(Assembly.GetCallingAssembly(), dataUrl);
            _updateListeners.Add(listener);
        }

        public static void CheckForUpdates(MainMenuManager mainMenuManager)
        {
            PeasApi.Logger.LogInfo("Started checking for updates ...");

            List<UpdateListener> _updatedMods = new List<UpdateListener>();
            
            foreach (var listener in _updateListeners)
            {
                PeasApi.Logger.LogInfo($"Checking for update of {listener.Mod.GetName().Name} ...");
                
                if (listener.IsUpToDate())
                {
                    PeasApi.Logger.LogInfo($"{listener.Mod.GetName().Name} is up to date!");
                }
                else
                {
                    PeasApi.Logger.LogInfo($"Found an update! {listener.Mod.GetName().Name} will be updated to version {listener.NewestVersion}");

                    if (listener.DownloadLink == null)
                    {
                        PeasApi.Logger.LogError("No download link was provided, can't update");
                        return;
                    }
                    
                    _updatedMods.Add(listener);
                }
            }
            
            PeasApi.Logger.LogInfo("Finished checking for updates!");

            var updatedMods = "This mods can be updated: \n\n";
            
            foreach (var mod in _updatedMods)
            {
                updatedMods += $"{mod.Mod.GetName().Name} ({mod.Mod.GetName().Version} -> {mod.NewestVersion})\n";
            }
            updatedMods += "\nRestart your game afterwards to use the new versions!";

            var popup = DiscordManager.Instance.discordPopup;
            popup.transform.localScale *= 2;
            
            var exitButton = popup.transform.FindChild("ExitGame");
            exitButton.GetComponentInChildren<TextTranslatorTMP>().Destroy();
            
            var updateButton = Object.Instantiate(exitButton, popup.transform);
            
            exitButton.transform.position += new Vector3(0.85f, -0.75f);
            exitButton.transform.localScale /= 2f;
            updateButton.transform.position += new Vector3(-0.85f, -0.75f);
            updateButton.transform.localScale /= 2f;
            
            updateButton.FindChild("Text_TMP").GetComponent<TextMeshPro>().text = "Update";
            exitButton.FindChild("Text_TMP").GetComponent<TextMeshPro>().text = "Don't update";

            var updateButtonButton = updateButton.GetComponent<PassiveButton>();
            updateButtonButton.OnClick.RemoveAllListeners();
            updateButtonButton.OnClick.AddListener((Action)(() =>
            {
                foreach (var mod in _updatedMods)
                {
                    mod.Update();
                }
            }));
            
            popup.Show(updatedMods);
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static class SomeCoolPatch
        {
            public static void Postfix(MainMenuManager __instance)
            {
                CheckForUpdates(__instance);
            }
        }

        private class UpdateListener
        {
            public Assembly Mod;
            public string DataUrl;

            public string NewestVersion
            {
                get
                {
                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            string json = client.DownloadString(DataUrl);
                            var data = JObject.Parse(json);
                            return (string) data["version"];
                        }
                    }
                    catch (Exception e)
                    {
                        PeasApi.Logger.LogError("Error accessing the data file.");
                    }

                    return "0.0.0";
                }
            }
            
            public string DownloadLink
            {
                get
                {
                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            string json = client.DownloadString(DataUrl);
                            var data = JObject.Parse(json);
                            return (string) data["downloadUrl"];
                        }
                    }
                    catch (Exception e)
                    {
                        PeasApi.Logger.LogError("Error accessing the data file.");
                    }

                    return null;
                }
            }

            public UpdateListener(Assembly mod, string dataUrl)
            {
                Mod = mod;
                DataUrl = dataUrl;
            }

            public bool IsUpToDate()
            {
                PeasApi.Logger.LogInfo($"{Mod.GetName().Version}, {Version.Parse(NewestVersion)}, {Mod.GetName().Version >= Version.Parse(NewestVersion)}");
                return Mod.GetName().Version >= Version.Parse(NewestVersion);
            }

            public void Update()
            {
                if (DownloadLink == null)
                {
                    PeasApi.Logger.LogError("No download link was provided, can't update");
                    return;
                }
                    
                if (!Directory.Exists("OutdatedMods"))
                    Directory.CreateDirectory("OutdatedMods");
                    
                if (File.Exists($"OutdatedMods\\{Mod.GetName().Name}.dll"))
                    File.Delete($"OutdatedMods\\{Mod.GetName().Name}.dll");
                    
                File.Move($"BepInEx\\plugins\\{Mod.GetName().Name}.dll", $"OutdatedMods\\{Mod.GetName().Name}.dll");

                if (DownloadLink.EndsWith(".zip"))
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(DownloadLink, Mod.GetName().Name + ".zip");
                    }
                    
                    ZipFile.ExtractToDirectory(Mod.GetName().Name + ".zip", "BepInEx\\plugins", true);
                        
                    File.Delete(Mod.GetName().Name + ".zip");
                    
                    PeasApi.Logger.LogInfo($"{Mod.GetName().Name} was successfully updated!");
                }
                else if (DownloadLink.EndsWith(".dll"))
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(DownloadLink, Mod.GetName().Name + ".dll");
                    }
                    
                    File.Move($"{Mod.GetName().Name}.dll", $"BepInEx\\plugins\\{Mod.GetName().Name}.dll");
                    
                    PeasApi.Logger.LogInfo($"{Mod.GetName().Name} was successfully updated!");
                }
                else
                {
                    PeasApi.Logger.LogError("Invalid download link was provided, can't update");
                }
            }
        }
    }
}