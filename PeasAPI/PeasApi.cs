using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using InnerNet;
using PeasAPI.Components;
using PeasAPI.Managers;
using Reactor;
using UnityEngine;
using Random = System.Random;

namespace PeasAPI
{
    [HarmonyPatch]
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class PeasApi : BasePlugin
    {
        public const string Id = "tk.peasplayer.amongus.api";
        public const string Version = "1.4.0";

        public Harmony Harmony { get; } = new Harmony(Id);

        public static readonly Random Random = new Random();

        public static ManualLogSource Logger { get; private set; }

        public static ConfigFile ConfigFile { get; private set; }

        public static bool GameStarted
        {
            get
            {
                return GameData.Instance && ShipStatus.Instance && AmongUsClient.Instance &&
                       (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started ||
                        AmongUsClient.Instance.GameMode == GameModes.FreePlay);
            }
        }

        /// <summary>
        /// Whether the roles should actually be used
        /// </summary>
        public static bool EnableRoles = true;

        /// <summary>
        /// If you set this to false please provide credit! I mean this stuff is free and open-source so a little credit would be nice :)
        /// </summary>
        public static bool ShamelessPlug = true;

        /// <summary>
        /// How much the account tab should be lowered
        /// </summary>
        public static Vector3 AccountTabOffset { get; set; } = new(0f, 0f, 0f);

        /// <summary>
        /// Whether the function of the account tab should be replaced with just the ability to change your name or not
        /// </summary>
        public static bool AccountTabOnlyChangesName { get; set; } = true;

        public override void Load()
        {
            Logger = this.Log;
            ConfigFile = Config;

            var useCustomServer = ConfigFile.Bind("CustomServer", "UseCustomServer", false);
            if (useCustomServer.Value)
            {
                CustomServerManager.RegisterServer(ConfigFile.Bind("CustomServer", "Name", "CustomServer").Value,
                    ConfigFile.Bind("CustomServer", "Ipv4 or Hostname", "au.peasplayer.tk").Value,
                    ConfigFile.Bind("CustomServer", "Port", (ushort) 22023).Value);
            }

            UpdateManager.RegisterUpdateListener(
                "https://raw.githubusercontent.com/Peasplayer/PeasAPI/main/PeasAPI/Data.json");

            RegisterCustomRoleAttribute.Register(this);

            Harmony.PatchAll();
        }

        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        [HarmonyPrefix]
        public static void PatchToTestSomeStuff(KeyboardJoystick __instance)
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
            }
        }
    }
}