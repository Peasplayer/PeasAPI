using System.Collections;
using System.Collections.Generic;
using BepInEx.IL2CPP.Utils.Collections;
using HarmonyLib;
using PeasAPI.CustomRpc;
using Reactor.Extensions;
using Reactor.Networking;
using TMPro;
using UnityEngine;

namespace PeasAPI.Managers
{
    public static class TextMessageManager
    {
        private static Transform _textOverlay;
        
        public static void ShowMessage(string message)
        {
            Reactor.Coroutines.Start(CoShowText(message));
        }
        
        public static void RpcShowMessage(string message, List<PlayerControl> targets)
        {
            targets.Do(player => Rpc<RpcShowMessage>.Instance.SendTo(player.PlayerId, new RpcShowMessage.Data(message, targets.ConvertAll(player => player.PlayerId))));
        }
        
        private static IEnumerator CoShowText(string text)
        {
            var hudManager = HudManager.Instance;
            if (hudManager == null)
                yield break;

            if (_textOverlay == null)
            {
                _textOverlay = Object.Instantiate(hudManager.TaskCompleteOverlay, hudManager.TaskCompleteOverlay.parent);
                _textOverlay.GetComponent<TextTranslatorTMP>().Destroy();
            }

            _textOverlay.GetComponent<TextMeshPro>().text = text;

            _textOverlay.gameObject.SetActive(true);

            yield return new ManagedIl2CppEnumerator(Effects.Slide2D(_textOverlay, new Vector2(0f, -8f), Vector2.zero, 0.25f));
            for (float time = 0f; time < 0.75f; time += Time.deltaTime)
            {
                yield return null;
            }

            if (!AmongUsClient.Instance.IsGameOver)
            {
                yield return new ManagedIl2CppEnumerator(Effects.Slide2D(_textOverlay, Vector2.zero, new Vector2(0f, 8f), 0.25f));
                _textOverlay.Destroy();
            }
            
            yield break;
        }
    }
}