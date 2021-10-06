using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using PeasAPI.Enums;

namespace PeasAPI.Managers.UpdateTools
{
    public sealed class GitHubUpdater : UpdateListener
    {
        public GitHubUpdater(Assembly assembly, string owner, string repoName, FileType priority,
            string authToken = null)
        {
            Assembly = assembly;
            JsonLink = $"https://api.github.com/repos/{owner}/{repoName}/releases/latest";
            AuthToken = authToken;
            Priority = priority;
            Initialize();
        }

        private FileType Priority { get; }
        private string AuthToken { get; }

        public override void FromJsonElement(JsonElement json)
        {
            var tagVer = json.GetProperty("tag_name").GetString();
            var assetLists = json.GetProperty("assets").EnumerateArray();
            Version = UpdateManager.SanitizeVersion(tagVer);
            AssetLink = GetLinkByPriority(assetLists.ToList());
        }

        public override HttpResponseMessage FetchData()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PeasAPI Updater");
            
            if (AuthToken != null && AuthToken.Trim() != "")
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", AuthToken);
            }

            return httpClient.GetAsync(new Uri(JsonLink), 0).Result;
        }

        private string GetLinkByPriority(IReadOnlyCollection<JsonElement> array)
        {
            var priority = Priority switch
            {
                FileType.First => null,
                FileType.Dll => "application/x-msdownload",
                FileType.Zip => "application/x-zip-compressed",
                _ => null
            };

            if (!array.Any()) return null;

            var first = array.Cast<JsonElement?>().FirstOrDefault(x =>
                x?.GetProperty("content_type").GetString()
                ?.Equals(priority) ?? true) ?? array.FirstOrDefault();
            
            return first.GetProperty("browser_download_url").GetString();
        }
    }
}