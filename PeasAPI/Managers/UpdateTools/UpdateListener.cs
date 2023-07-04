using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using BepInEx.Unity.IL2CPP;
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

        public UpdateType UpdateType { get; set; } = UpdateType.Every;

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
            switch (UpdateType)
            {
                default:
                    return !(Assembly.GetName().Version < Version);
                case UpdateType.Every:
                    return !(Assembly.GetName().Version < Version);
                case UpdateType.OnlyMajor:
                    return !(Assembly.GetName().Version.Major < Version.Major);
                case UpdateType.OnlyMinor:
                    return !(Assembly.GetName().Version.Minor < Version.Minor);
                case UpdateType.OnlyBuild:
                    return !(Assembly.GetName().Version.Build < Version.Build);
                case UpdateType.MajorAndBuild:
                    return !(Assembly.GetName().Version.Major < Version.Major) || !(Assembly.GetName().Version.Build < Version.Build);
                case UpdateType.MajorAndMinor:
                    return !(Assembly.GetName().Version.Major < Version.Major) || !(Assembly.GetName().Version.Minor < Version.Minor);
                case UpdateType.MinorAndBuild:
                    return !(Assembly.GetName().Version.Minor < Version.Minor) || !(Assembly.GetName().Version.Build < Version.Build);
            }
        }

        public virtual void Initialize()
        {
            try
            {
                var httpResponseMessage = FetchData();
                if (httpResponseMessage == null)
                    return;
                var result = httpResponseMessage.Content.ReadAsStringAsync().Result;
                FromJsonElement(JsonDocument.Parse(result).RootElement);
            } catch (Exception e)
            {
                
            }
           // catch (Exception ex)
           // {
              //  PeasAPI.Logger.LogError($"An error occured while initializing {Name}: \n{ex.Message}");
           // }
        }

        public abstract void FromJsonElement(JsonElement json);

        public virtual HttpResponseMessage FetchData()
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "PeasAPI Updater");
                return httpClient.GetAsync(new Uri(JsonLink), 0).Result;
            }
            catch (Exception)
            {
                PeasAPI.Logger.LogError("There was an error whilst trying to fetch update data");
            }

            return null;
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

            if (PeasAPI.Logging)
                PeasAPI.Logger.LogInfo($"Successfully updated {Name}!");
        }
    }
}