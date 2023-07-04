﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using PeasAPI.Enums;
using PeasAPI.Managers.UpdateTools;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PeasAPI.Managers
{
    public static class UpdateManager
    {
        public static bool DoUpdateChecks = true;
        private static readonly List<UpdateListener> UpdateListeners = new();

        public static void RegisterGitHubUpdateListener(string owner, string repoName, UpdateType updateType = UpdateType.Every, FileType type = FileType.Dll)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            UpdateListeners.Add(new GitHubUpdater(callingAssembly, owner, repoName, type, updateType));
        }

        public static void RegisterUpdateListener(string link, UpdateType updateType = UpdateType.Every)
        {
            UpdateListeners.Add(new DefaultUpdater(Assembly.GetCallingAssembly(), link, updateType));
        }

        public static void RegisterUpdateListener(UpdateListener updateListener)
        {
            UpdateListeners.Add(updateListener);
        }

        public static Version SanitizeVersion(string str)
        {
            var text = str.Replace("v", "").Split("-").First();
            return Version.TryParse(text, out var version) ? version : null;
        }

        public static void CheckForUpdates()
        {
            try
            {
                if (!DoUpdateChecks)
                {
                    if (PeasAPI.Logging)
                        PeasAPI.Logger.LogInfo("Checking for updates skipped.");
                    return;
                }
                
                if (PeasAPI.Logging)
                    PeasAPI.Logger.LogInfo("Checking for updates..");
                
                var enumerable = UpdateListeners.Where(x => !x.IsUpToDate());
                var updateListeners = enumerable as UpdateListener[] ?? enumerable.ToArray();
                
                if (!updateListeners.Any())
                {
                    if (PeasAPI.Logging)
                        PeasAPI.Logger.LogInfo("All mods are up-to-dated..");
                    return;
                }

                var stringBuilder = 
                    new StringBuilder().AppendLine("<size=140%>The following mods can be updated:</size>\n");
                
                foreach (var updateListener in updateListeners)
                {
                    stringBuilder.AppendLine(
                        $"{updateListener.Name} ({updateListener.Assembly.GetName().Version} → {updateListener.Version})");
                    
                    if (PeasAPI.Logging)
                        PeasAPI.Logger.LogInfo($"Found an update for {updateListener.Name}!");
                }

                stringBuilder.AppendLine("\nRestart your game afterwards to use the new versions!");
                
                var genericPopup = PopUp("Update", "Don't Update", delegate
                {
                    foreach (var mod in updateListeners) 
                        mod.UpdateMod();
                });
                
                genericPopup.Show(stringBuilder.ToString());
            }
            catch (Exception ex)
            {
                var text = $"An error occurred while attempting to (check for) update: \n{ex}";
                PeasAPI.Logger.LogError(text);
                DestroyableSingleton<DiscordManager>.Instance.discordPopup.Show(text);
            }
        }

        private static GenericPopup PopUp(string option1, string option2, Action action)
        {
            var popup = Object.Instantiate(DiscordManager.Instance.discordPopup, DiscordManager.Instance.discordPopup.transform.parent);
            popup.transform.localScale = Vector3.one * 2f;
            
            var transform = CreateButton(popup, "yesButton", option1, new Vector3(-0.85f, -0.75f));
            var transform2 = CreateButton(popup, "noButton", option2, new Vector3(0.85f, -0.75f));
            
            var component = transform.GetComponent<PassiveButton>();
            component.OnClick.RemoveAllListeners();
            component.OnClick.AddListener(action);
            
            return popup;
        }

        private static Transform CreateButton(GenericPopup popup, string name, string text, Vector3 offset)
        {
            var find = popup.transform.FindChild(name);
            if (find) return find;
            
            var template = popup.transform.FindChild("ExitGame");
            template.GetComponentInChildren<TextTranslatorTMP>().Destroy();
            template.gameObject.SetActive(false);
            
            var button = Object.Instantiate(template, popup.transform).DontDestroy();
            var transform = button.transform;
            
            button.gameObject.name = name;
            transform.position += offset;
            transform.localScale /= 2f;
            
            button.GetComponentInChildren<TextMeshPro>().text = text;
            button.gameObject.SetActive(true);
            return button;
        }
    }
}