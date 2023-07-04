using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using InnerNet;
using PeasAPI.Components;
using PeasAPI.GameModes;
using PeasAPI.Managers;
using PeasAPI.Options;
using Reactor;
using UnityEngine;
using Random = System.Random;

namespace PeasAPI
{
    [BepInAutoPlugin]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public partial class PeasAPI : BasePlugin
    {

        public Harmony Harmony { get; } = new Harmony(Id);

        public static readonly Random Random = new Random();

        public static ConfigFile ConfigFile { get; private set; }

        public static ManualLogSource Logger { get; private set; }

        public static bool Logging
        {
            get
            {
                if (ConfigFile == null)
                    return true;
                return ConfigFile.Bind("Settings", "Logging", true).Value;
            }
        }

        public static bool GameStarted
        {
            get
            {
                return GameData.Instance && ShipStatus.Instance && AmongUsClient.Instance &&
                       (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started ||
                        AmongUsClient.Instance.NetworkMode == global::NetworkModes.FreePlay  &&
                        AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame);
            }
        }

        /// <summary>
        /// If you set this to false please provide credit! I mean this stuff is free and open-source so a little credit would be nice :)
        /// </summary>
        public static bool ShamelessPlug = false;

        public static CustomToggleOption ShowRolesOfDead;

        public override void Load()
        {
            Logger = this.Log;
            ConfigFile = Config;

            var useCustomServer = ConfigFile.Bind("CustomServer", "UseCustomServer", false);
            if (useCustomServer.Value)
            {
                CustomServerManager.RegisterServer(ConfigFile.Bind("CustomServer", "Name", "CustomServer").Value,
                    ConfigFile.Bind("CustomServer", "Ipv4 or Hostname", "127.0.0.1").Value,
                    ConfigFile.Bind("CustomServer", "Port", (ushort)22023).Value);
            }

            UpdateManager.RegisterGitHubUpdateListener("AmongUsDev", "PeasAPI-R");

            RegisterCustomRoleAttribute.Load();
            RegisterCustomGameModeAttribute.Load();
            
            ShowRolesOfDead =
                new CustomToggleOption("ShowRolesOfDead", "Show the roles of dead player", false) {IsFromPeasAPI = true};
            GameModeManager.GameModeOption = new CustomStringOption("gamemode", "GameMode", "None") {IsFromPeasAPI = true};
            
            Harmony.PatchAll();
        }

        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        [HarmonyPrefix]
        public static void PatchToTestSomeStuff(KeyboardJoystick __instance)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("Something is coming in the future");
            }
        }
    }
}