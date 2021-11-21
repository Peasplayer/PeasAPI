using System;
using System.Reflection;
using BepInEx.IL2CPP;
using HarmonyLib;
using PeasAPI.Roles;
using Reactor;

namespace PeasAPI.Components
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterCustomRoleAttribute : Attribute
    {
        [Obsolete("You don't need to call this anymore", true)]
        public static void Register(BasePlugin plugin)
        {
            Register(Assembly.GetCallingAssembly(), plugin);
        }

        public static void Register(Assembly assembly, BasePlugin plugin)
        {
            foreach (var type in assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<RegisterCustomRoleAttribute>(); 

                if (attribute != null)
                {
                    if (!type.IsSubclassOf(typeof(BaseRole)))
                    {
                        throw new InvalidOperationException($"Type {type.FullDescription()} must extend {nameof(BaseRole)}.");
                    }
                    
                    if (PeasAPI.Logging)
                        PeasAPI.Logger.LogInfo($"Registered role {type.Name} from {type.Assembly.GetName().Name}");
                    
                    Activator.CreateInstance(type, plugin);
                }
            }
        }

        public static void Load()
        {
            ChainloaderHooks.PluginLoad += plugin => Register(plugin.GetType().Assembly, plugin);
        }
    }
}