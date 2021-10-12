using System.Reflection;
using System.Text.Json;
using PeasAPI.Enums;

namespace PeasAPI.Managers.UpdateTools
{
    public sealed class DefaultUpdater : UpdateListener
    {
        public DefaultUpdater(Assembly assembly, string link, UpdateType updateType = UpdateType.Every)
        {
            Assembly = assembly;
            JsonLink = link;
            UpdateType = updateType;
            Initialize();
        }

        public override void FromJsonElement(JsonElement json)
        {
            Version = UpdateManager.SanitizeVersion(json.GetProperty("version").GetString());
            AssetLink = json.GetProperty("downloadUrl").GetString();
        }
    }
}