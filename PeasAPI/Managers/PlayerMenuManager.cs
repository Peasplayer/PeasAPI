using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Reactor;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace PeasAPI.Managers
{
    public static class PlayerMenuManager
    {
        public static bool IsMenuOpen = false;
        
        public static void OpenPlayerMenu(List<byte> players, Action<PlayerControl> onPlayerWasChosen)
        {
            if (MeetingHud.Instance != null)
                return;
            if (AmongUsClient.Instance.IsGameOver)
                return;
            
            Coroutines.Start(CoCreatePlayerMenu(players, onPlayerWasChosen));
        }

        private static IEnumerator CoCreatePlayerMenu(List<byte> players, Action<PlayerControl> onPlayerWasChosen)
        {
            IsMenuOpen = true;

            if (MapBehaviour.Instance)
                MapBehaviour.Instance.Close();
            if (Minigame.Instance)
                Minigame.Instance.ForceClose();

            var instance = MeetingHud.Instance = Object.Instantiate(HudManager.Instance.MeetingPrefab, DestroyableSingleton<HudManager>.Instance.transform, true);
            
            instance.playerStates = new PlayerVoteArea[GameData.Instance.PlayerCount];
            foreach (var playerId in players)
            {
                GameData.PlayerInfo playerInfo = playerId.GetPlayerInfo();
                PlayerVoteArea playerVoteArea = instance.playerStates[playerId] = instance.CreateButton(playerInfo);
                playerVoteArea.Parent = instance;
                playerVoteArea.SetTargetPlayerId(playerInfo.PlayerId);
                playerVoteArea.SetDead(false, false);
                playerVoteArea.UpdateOverlay();

                var playerButton = playerVoteArea.gameObject.GetComponentInChildren<PassiveButton>();
                playerButton.OnClick.RemoveAllListeners();
                playerButton.OnClick.AddListener((UnityAction) listener);

                void listener()
                {
                    try
                    {
                        onPlayerWasChosen.Invoke(playerInfo.Object);
                    }
                    catch (Exception err)
                    {
                        PeasAPI.Logger.LogError("There was an error while executing the player menu: " + err);
                    }

                    CloseMenu();
                }
                
                ControllerManager.Instance.AddSelectableUiElement(playerVoteArea.PlayerButton, false);
            }
            instance.playerStates = instance.playerStates.Where(p => p != null).ToArray();

            var skipButton = instance.gameObject.GetComponentInChildren<PassiveButton>();
            skipButton.OnClick.RemoveAllListeners();
            skipButton.OnClick.AddListener((UnityAction) CloseMenu);

            instance.SortButtons();

            PlayerControl.LocalPlayer.MyPhysics.ResetMoveState(false);
            PlayerControl.LocalPlayer.NetTransform.Halt();

            instance.transform.localPosition = new Vector2(0f, 0f);
            instance.MeetingIntro.gameObject.SetActive(true);

            HudManager.Instance.SetHudActive(false);
            instance.transform.FindChild("Background").gameObject.SetActive(false);
            instance.MeetingIntro.gameObject.SetActive(false);
            ControllerManager.Instance.OpenOverlayMenu(MeetingHud.Instance.name, null,
                MeetingHud.Instance.DefaultButtonSelected, MeetingHud.Instance.ControllerSelectable, false);
            yield break;
        }

        private static void CloseMenu()
        {
            HudManager.Instance.Chat.SetPosition(null);
            HudManager.Instance.Chat.SetVisible(PlayerControl.LocalPlayer.Data.IsDead);
            HudManager.Instance.Chat.BanButton.Hide();
            MeetingHud.Instance.DespawnOnDestroy = false;
            ConsoleJoystick.SetMode_Task();
            Camera.main.GetComponent<FollowerCamera>().Locked = false;
            DestroyableSingleton<HudManager>.Instance.SetHudActive(true);
            ControllerManager.Instance.ResetAll();
            Object.Destroy(MeetingHud.Instance.gameObject);
            IsMenuOpen = false;
        }
        
        [HarmonyPatch]
        public static class Patches
        {
            [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
            [HarmonyPostfix]
            public static void MeetingHudStartPatch(MeetingHud __instance)
            {
                if (IsMenuOpen)
                {
                    HudManager.Instance.Chat.SetPosition(null);
                    HudManager.Instance.Chat.SetVisible(false);
                    __instance.discussionTimer = 20;
                }
            }
            
            [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
            [HarmonyPostfix]
            public static void MeetingHudUpdatePatch(MeetingHud __instance)
            {
                if (IsMenuOpen)
                {
                    __instance.TitleText.text = "Choose a player";
                    __instance.SkipVoteButton.GetComponentInChildren<TextMeshPro>().text = "Close";
                    __instance.discussionTimer -= Time.deltaTime;
                    __instance.UpdateButtons();

                    if (__instance.discussionTimer < 0)
                    {
                        CloseMenu();
                        return;
                    }

                    __instance.TimerText.text = $"Time left: {Mathf.CeilToInt(__instance.discussionTimer)}s";
                    //__instance.SkipVoteButton.SetDisabled();
                }
            }

            [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
            [HarmonyPrefix]
            public static void PlayerControlHandleRpcPatch(PlayerControl __instance, [HarmonyArgument(0)] byte callId)
            {
                if ((callId == 14 || callId == 11) && IsMenuOpen)
                    CloseMenu();
            }
        }
    }
}