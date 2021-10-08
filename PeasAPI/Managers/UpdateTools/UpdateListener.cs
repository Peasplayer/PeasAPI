﻿using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using BepInEx.IL2CPP;
using PeasAPI.Enums;
using Reactor;
using UnityEngine;

namespace PeasAPI.Managers.UpdateTools
{
    public abstract class UpdateListener
    {
        public Assembly Assembly { get; set; }
        protected string JsonLink { get; set; }
        protected string AssetLink { get; set; }
        public string Name => Assembly.GetName().Name;

        public Version Version { get; set; }

        private FileType? Type
        {
            get
            {
                if (AssetLink == null) return null;
                if (AssetLink.EndsWith(".zip")) return FileType.Zip;
                if (AssetLink.EndsWith(".dll")) return FileType.Dll;

                return null;
            }
        }

        public virtual bool IsUpToDate()
        {
            return !(Assembly.GetName().Version < Version);
        }

        public virtual void Initialize()
        {
            try
            {
                var httpResponseMessage = FetchData();
                var result = httpResponseMessage.Content.ReadAsStringAsync().Result;
                FromJsonElement(JsonDocument.Parse(result).RootElement);
            }
            catch (Exception ex)
            {
                Logger<PeasApi>.Error($"An error occured while initializing {Name}: \n{ex}");
            }
        }

        public abstract void FromJsonElement(JsonElement json);

        public virtual HttpResponseMessage FetchData()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PeasAPI Updater");
            return httpClient.GetAsync(new Uri(JsonLink), 0).Result;
        }

        private string GetAssemblyPath()
        {
            var directoryName = Path.GetDirectoryName(Application.dataPath);
            var enumerable = IL2CPPChainloader.Instance.Plugins.Values.Select(x => x.Location);
            return enumerable.FirstOrDefault(x => Path.GetFileName(x).Contains(Name)) ??
                   $"{directoryName}\\BepInEx\\plugins\\{Name}.dll";
        }

        public virtual void UpdateMod()
        {
            using var webClient = new WebClient();
            
            var directoryName = Path.GetDirectoryName(Application.dataPath);
            var assemblyPath = GetAssemblyPath();
            var text = $"OutdatedMods\\{Name}.dll";
            
            Directory.CreateDirectory($"{directoryName}\\OutdatedMods");
            if (File.Exists(text)) File.Delete(text);
            File.Move(assemblyPath, text);
            
            switch (Type)
            {
                case FileType.Dll:
                    webClient.DownloadFile(AssetLink, $"{Name}.dll");
                    File.Move($"{Name}.dll", assemblyPath);
                    break;
                
                case FileType.Zip:
                    webClient.DownloadFile(AssetLink, $"{Name}.zip");
                    ZipFile.ExtractToDirectory($"{Name}.zip", "BepInEx\\plugins", true);
                    File.Delete($"{Name}.zip");
                    break;
                
                case FileType.First: throw new ArgumentOutOfRangeException();
                default: throw new ArgumentOutOfRangeException();
            }

            PeasApi.Logger.LogInfo($"Successfully updated {Name}!");
        }
    }
}