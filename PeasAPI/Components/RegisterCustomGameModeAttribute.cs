using System;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using PeasAPI.GameModes;
using Reactor;

namespace PeasAPI.Components
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterCustomGameModeAttribute : Attribute
    {
        public static void Register(BasePlugin plugin)
        {
            Register(Assembly.GetCallingAssembly(), plugin);
        }

        public static void Register(Assembly assembly, BasePlugin plugin)
        {
            foreach (var type in assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<RegisterCustomGameModeAttribute>(); 

                if (attribute != null)
                {
                    if (!type.IsSubclassOf(typeof(GameMode)))
                    {
                        throw new InvalidOperationException($"Type {type.FullDescription()} must extend {nameof(GameMode)}.");
                    }
                    
                    if (PeasAPI.Logging)
                        PeasAPI.Logger.LogInfo($"Registered mode {type.Name} from {type.Assembly.GetName().Name}");
                    
                    Activator.CreateInstance(type, plugin);
                }
            }
        }

        public static void Load()
        {
           IL2CPPChainloader.Instance.PluginLoad += (_, assembly, plugin) => Register(assembly, plugin);
        }
    }
}