using System.Reflection;
using System.Text.Json;

namespace PeasAPI.Managers.UpdateManager
{
    public sealed class DefaultUpdater : UpdateListener
    {
        public DefaultUpdater(Assembly assembly, string link)
        {
            Assembly = assembly;
            JsonLink = link;
            Initialize();
        }

        public override void FromJsonElement(JsonElement json)
        {
            Version = UpdateManager.SanitizeVersion(json.GetProperty("version").GetString());
            AssetLink = json.GetProperty("downloadUrl").GetString();
        }
    }
}