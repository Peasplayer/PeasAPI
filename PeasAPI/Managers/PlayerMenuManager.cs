using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Text.Json;
using Reactor.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace PeasAPI.Managers
{
    public static class PlayerMenuManager
    {
        internal static bool IsMenuOpen = false;
        internal static MeetingHud Instance;
        
        public static void OpenPlayerMenu(List<byte> players, Action<PlayerControl> onPlayerWasChosen, Action onMenuClosed)
        {
            if (MeetingHud.Instance != null)
                return;
            if (AmongUsClient.Instance.IsGameOver)
                return;
            
            Coroutines.Start(CoCreatePlayerMenu(players, onPlayerWasChosen, onMenuClosed));
        }

        private static IEnumerator CoCreatePlayerMenu(List<byte> players, Action<PlayerControl> onPlayerWasChosen, Action onMenuClosed)
        {
            IsMenuOpen = true;

            if (MapBehaviour.Instance)
                MapBehaviour.Instance.Close();
            if (Minigame.Instance)
                Minigame.Instance.ForceClose();

            var instance = Instance = Object.Instantiate(HudManager.Instance.MeetingPrefab, DestroyableSingleton<HudManager>.Instance.transform, true);
            
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
                playerButton.OnClick.AddListener((UnityAction) ChooseListener);

                void ChooseListener()
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
            skipButton.OnClick.AddListener((UnityAction) CloseListener);
            void CloseListener()
            {
                try
                {
                    onMenuClosed.Invoke();
                }
                catch (Exception err)
                {
                    PeasAPI.Logger.LogError("There was an error while executing the player menu: " + err);
                }

                CloseMenu();
            }

            instance.SortButtons();

            PlayerControl.LocalPlayer.MyPhysics.ResetMoveState(false);
            PlayerControl.LocalPlayer.NetTransform.Halt();

            instance.transform.localPosition = new Vector2(0f, 0f);
            instance.MeetingIntro.gameObject.SetActive(true);

            HudManager.Instance.SetHudActive(false);
            instance.transform.FindChild("Background").gameObject.SetActive(false);
            instance.MeetingIntro.gameObject.SetActive(false);
            ControllerManager.Instance.OpenOverlayMenu(Instance.name, null,
                Instance.DefaultButtonSelected, Instance.ControllerSelectable, false);
            yield break;
        }

        private static void CloseMenu()
        {
            MeetingHud.Instance = null;
            HudManager.Instance.Chat.SetPosition(null);
            HudManager.Instance.Chat.SetVisible(PlayerControl.LocalPlayer.Data.IsDead);
            HudManager.Instance.Chat.BanButton.Hide();
            Instance.DespawnOnDestroy = false;
            ConsoleJoystick.SetMode_Task();
            Camera.main.GetComponent<FollowerCamera>().Locked = false;
          // DestroyableSingleton<HudManager>.Instance.SetHudActive(false);
            ControllerManager.Instance.ResetAll();
            Object.Destroy(Instance.gameObject);
            IsMenuOpen = false;
        }
        
        [HarmonyPatch]
        internal static class Patches
        {
            [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
            [HarmonyPrefix]
            public static void OnMeetingStartPatch(PlayerControl __instance)
            {
                if (IsMenuOpen)
                {
                    Instance.gameObject.GetComponentInChildren<PassiveButton>().OnClick.Invoke();
                }
            }
            
            [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
            [HarmonyPostfix]
            public static void MeetingHudOnStartPatch(MeetingHud __instance)
            {
                if (IsMenuOpen)
                {
                    HudManager.Instance.Chat.SetPosition(null);
                    HudManager.Instance.Chat.SetVisible(false);
                    __instance.discussionTimer = 21;
                }
            }
            
            [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
            [HarmonyPrefix]
            public static bool MeetingHudStartPatch(MeetingHud __instance)
            {
                if (IsMenuOpen)
                {
                    foreach (SpriteRenderer playerMaterialColors in __instance.PlayerColoredParts)
                    {
                        PlayerControl.LocalPlayer.SetPlayerMaterialColors(playerMaterialColors);
                    }
                    DestroyableSingleton<HudManager>.Instance.StopOxyFlash();
                    DestroyableSingleton<HudManager>.Instance.StopReactorFlash();
                    __instance.SkipVoteButton.SetTargetPlayerId(253);
                    __instance.SkipVoteButton.Parent = __instance;
                    Camera.main.GetComponent<FollowerCamera>().Locked = true;
                    if (!AmongUsClient.Instance.DisconnectHandlers.Contains(__instance.Cast<IDisconnectHandler>()))
                        AmongUsClient.Instance.DisconnectHandlers.Add(__instance.Cast<IDisconnectHandler>());
                    foreach (PlayerVoteArea playerVoteArea in __instance.playerStates)
                    {
                        __instance.ControllerSelectable.Add(playerVoteArea.PlayerButton);
                    }
                    DestroyableSingleton<AchievementManager>.Instance.OnMeetingCalled();
                    return false;
                }

                return true;
            }
            
            [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.SetForegroundForDead))]
            [HarmonyPrefix]
            public static bool DisableDeadOverlayPatch(MeetingHud __instance)
            {
                if (IsMenuOpen)
                    return false;

                return true;
            }
            
            [HarmonyPatch(typeof(DummyBehaviour), nameof(DummyBehaviour.Update))]
            [HarmonyPrefix]
            public static bool DummyDontVotePatch(DummyBehaviour __instance)
            {
                GameData.PlayerInfo data = __instance.myPlayer.Data;
                if (data == null || data.IsDead)
                {
                    return false;
                }
                if (MeetingHud.Instance && !IsMenuOpen)
                {
                    Logger<PeasAPI>.Info("IsMenuOpen: " + IsMenuOpen);
                    if (!__instance.voted)
                    {
                        __instance.voted = true;
                        __instance.StartCoroutine(__instance.DoVote());
                        return false;
                    }
                }
                else
                {
                    __instance.voted = false;
                }
                return false;
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