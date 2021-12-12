using System;
using System.Linq;
using HarmonyLib;
using System.Collections.Generic;
using InnerNet;
using PeasAPI.CustomRpc;
using Reactor;
using Reactor.Extensions;
using Reactor.Networking;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PeasAPI.Options
{
    [HarmonyPatch]
    public static class Patches
    {
        private static float AllOptionSize = 6.73f;
        private static float LowestOption = -7.85f;
        private static float OptionSize = 0.5f;
        private static float HudTextSize = 1.4f;

        private static Scroller OptionsScroller;

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        [HarmonyPostfix]
        private static void GameOptionsMenuStartPatch(GameOptionsMenu __instance)
        {
            var numberOptionPrefab = Object.FindObjectsOfType<NumberOption>().FirstOrDefault();
            
            var toggleOptionPrefab =
                Object.FindObjectOfType<ToggleOption>(); 
            
            var stringOptionPrefab = Object.FindObjectsOfType<StringOption>().FirstOrDefault();

            LowestOption = 1.15f - __instance.Children.Length * 0.5f;

            foreach (var customOption in OptionManager.CustomOptions)
            {
                OptionBehaviour option = null;

                if (customOption.GetType() == typeof(CustomToggleOption))
                {
                    var _option = (CustomToggleOption) customOption;

                    if (AmongUsClient.Instance.AmHost)
                    {
                        ToggleOption toggleOption =
                            Object.Instantiate(toggleOptionPrefab, toggleOptionPrefab.transform.parent);

                        _option.Option = toggleOption;

                        toggleOption.TitleText.text = _option.Title;
                        toggleOption.Title = CustomStringName.Register(_option.Title);
                        toggleOption.CheckMark.enabled = _option.Value;

                        option = toggleOption;

                        option.OnValueChanged = new Action<OptionBehaviour>(behaviour =>
                        {
                            _option.SetValue(!toggleOption.oldValue);
                        });
                    }
                    else
                    {
                        StringOption toggleOption =
                            Object.Instantiate(stringOptionPrefab, stringOptionPrefab.transform.parent);

                        _option.Option = toggleOption;

                        toggleOption.TitleText.text = _option.Title;
                        toggleOption.Title = CustomStringName.Register(_option.Title);
                        toggleOption.Value = _option.Value ? 0 : 1;

                        var values = new List<StringNames>();
                        values.Add(CustomStringName.Register("On"));
                        values.Add(CustomStringName.Register("Off"));
                        toggleOption.Values = values.ToArray();

                        option = toggleOption;

                        option.OnValueChanged = new Action<OptionBehaviour>(behaviour =>
                        {
                            _option.SetValue(toggleOption.Value == 0);
                        });
                    }
                }
                else if (customOption.GetType() == typeof(CustomNumberOption))
                {
                    var _option = (CustomNumberOption) customOption;
                    NumberOption numberOption =
                        Object.Instantiate(numberOptionPrefab, numberOptionPrefab.transform.parent);
                    
                    numberOption.TitleText.text = _option.Title;
                    numberOption.Title = CustomStringName.Register(_option.Title);
                    numberOption.Value = _option.Value;
                    numberOption.ValidRange = new FloatRange(_option.MinValue, _option.MaxValue);
                    numberOption.Increment = _option.Increment;
                    numberOption.SuffixType = _option.SuffixType;

                    option = numberOption;
                    _option.Option = numberOption;

                    option.OnValueChanged = new Action<OptionBehaviour>(behaviour =>
                    {
                        _option.SetValue(numberOption.Value);
                    });
                }
                else if (customOption.GetType() == typeof(CustomStringOption))
                {
                    var _option = (CustomStringOption) customOption;
                    StringOption stringOption =
                        Object.Instantiate(stringOptionPrefab, stringOptionPrefab.transform.parent);
                    
                    stringOption.TitleText.text = _option.Title;
                    stringOption.Title = CustomStringName.Register(_option.Title);
                    stringOption.Value = _option.Value;
                    stringOption.ValueText.text = _option.StringValue;
                    stringOption.Values = _option.Values.ToArray();
                    
                    option = stringOption;
                    _option.Option = stringOption;

                    option.OnValueChanged = new Action<OptionBehaviour>(behaviour =>
                    {
                        _option.SetValue(stringOption.Value);
                    });
                }
                else if (customOption.GetType() == typeof(CustomOptionHeader))
                {
                    var _option = (CustomOptionHeader) customOption;
                    ToggleOption header =
                        Object.Instantiate(toggleOptionPrefab, toggleOptionPrefab.transform.parent);
                    
                    header.TitleText.text = _option.Title;
                    header.Title = CustomStringName.Register(_option.Title);
                    
                    var checkBox = header.transform.FindChild("CheckBox")?.gameObject;
                    if (checkBox) checkBox.Destroy();
                
                    var background = header.transform.FindChild("Background")?.gameObject;
                    if (background) background.Destroy();

                    option = header;

                    _option.Option = header;
                }
                else if (customOption.GetType() == typeof(CustomOptionButton))
                {
                    var _option = (CustomOptionButton) customOption;

                    if (AmongUsClient.Instance.AmHost)
                    {
                        ToggleOption toggleOption =
                            Object.Instantiate(toggleOptionPrefab, toggleOptionPrefab.transform.parent);

                        _option.Option = toggleOption;

                        toggleOption.TitleText.text = _option.Title;
                        toggleOption.Title = CustomStringName.Register(_option.Title);
                        toggleOption.CheckMark.enabled = false;
                        toggleOption.transform.FindChild("CheckBox").gameObject.SetActive(false);

                        option = toggleOption;
                        
                        option.OnValueChanged = new Action<OptionBehaviour>(behaviour =>
                        {
                            _option.SetValue(!toggleOption.oldValue);
                        });
                    }
                    else
                    {
                        StringOption toggleOption =
                            Object.Instantiate(stringOptionPrefab, stringOptionPrefab.transform.parent);

                        _option.Option = toggleOption;

                        toggleOption.TitleText.text = _option.Title;
                        toggleOption.Title = CustomStringName.Register(_option.Title);
                        toggleOption.Value = 0;
                        toggleOption.transform.FindChild("Value_TMP").gameObject.SetActive(false);

                        option = toggleOption;

                        option.OnValueChanged = new Action<OptionBehaviour>(behaviour =>
                        {
                            _option.SetValue(_option.Value);
                        });
                        option.OnValueChanged.Invoke(option);
                    }
                }

                option.transform.localPosition = new Vector3(option.transform.localPosition.x,
                    LowestOption - (OptionManager.CustomOptions.IndexOf(customOption) + 1) * OptionSize, -1);

                var options = __instance.Children.ToList();
                options.Add(option);
                __instance.Children = options.ToArray();
            }

            __instance.GetComponentInParent<Scroller>().YBounds.max =
                AllOptionSize + OptionManager.MenuVisibleOptions.Count * 0.5f;
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
        [HarmonyPostfix]
        private static void GameOptionsMenuUpdatePatch(GameOptionsMenu __instance)
        {
            __instance.GetComponentInParent<Scroller>().YBounds.max =
                AllOptionSize + OptionManager.MenuVisibleOptions.Count * 0.5f;

            foreach (var option in __instance.Children.ToList().FindAll(option => option.IsCustom()))
            {
                var customOption = option.GetCustom();
                if (customOption == null)
                    continue;
                
                option.gameObject.SetActive(customOption.MenuVisible);
                
                if (customOption.MenuVisible)
                    option.transform.localPosition = new Vector3(option.transform.localPosition.x,
                        LowestOption - (OptionManager.MenuVisibleOptions.IndexOf(customOption) + 1) * OptionSize, -1);

                if (option.gameObject.GetComponent<ToggleOption>() != null)
                    option.gameObject.GetComponent<ToggleOption>().TitleText.text = customOption.Title;
                else if (option.gameObject.GetComponent<NumberOption>() != null)
                    option.gameObject.GetComponent<NumberOption>().TitleText.text = customOption.Title;
                else if (option.gameObject.GetComponent<StringOption>() != null)
                    option.gameObject.GetComponent<StringOption>().TitleText.text = customOption.Title;
            }
        }
        
        [HarmonyPatch(typeof(OptionBehaviour), nameof(OptionBehaviour.SetAsPlayer))]
        public static class OptionBehaviourSetAsPlayerPatch
        {
            public static bool Prefix(OptionBehaviour __instance)
            {
                foreach (var button in __instance.GetComponentsInChildren<PassiveButton>())
                {
                    button.Destroy();
                    button.gameObject.SetActive(button.gameObject.name == __instance.gameObject.name &&
                                                __instance.Title != StringNames.GameRecommendedSettings);
                }
                
                return false;
            }
        }
        
        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.FixedUpdate))]
        [HarmonyPostfix]
        private static void NumberOptionFixedUpdatePatch(NumberOption __instance)
        {
            var customOption = (CustomNumberOption) __instance.GetCustom();
            if (customOption != null)
            {
                if (__instance.SuffixType == NumberSuffixes.None)
                {
                    __instance.ValueText.text = customOption.Value.ToString();
                    return;
                }

                if (__instance.SuffixType == NumberSuffixes.Multiplier)
                {
                    __instance.ValueText.text = customOption.Value + "x";
                    return;
                }

                if (__instance.SuffixType == NumberSuffixes.Seconds)
                {
                    __instance.ValueText.text = customOption.Value + "s";
                    return;
                }

                __instance.ValueText.text = customOption.Value.ToString();
            }
        }
        
        [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.ToHudString))]
        [HarmonyPostfix]
        private static void GameOptionsDataToHudStringPatch(GameOptionsData __instance, ref string __result)
        {
            foreach (var option in OptionManager.HudVisibleOptions)
            {
                if (option.GetType() == typeof(CustomToggleOption))
                {
                    __instance.settings.AppendLine($"{option.Title}: " + (((CustomToggleOption) option).Value ? "On" : "Off"));
                }
                else if (option.GetType() == typeof(CustomNumberOption))
                {
                    if (((CustomNumberOption)option).SuffixType == NumberSuffixes.None)
                    {
                        __instance.settings.AppendLine($"{option.Title}: " + ((CustomNumberOption) option).Value);
                        continue;
                    }

                    if (((CustomNumberOption)option).SuffixType == NumberSuffixes.Multiplier)
                    {
                        __instance.settings.AppendLine($"{option.Title}: " + ((CustomNumberOption) option).Value + "x");
                        continue;
                    }

                    if (((CustomNumberOption)option).SuffixType == NumberSuffixes.Seconds)
                    {
                        __instance.settings.AppendLine($"{option.Title}: " + ((CustomNumberOption) option).Value + "s");
                        continue;
                    }

                    __instance.settings.AppendLine($"{option.Title}: " + ((CustomNumberOption) option).Value);
                }
                else if (option.GetType() == typeof(CustomStringOption))
                {
                    __instance.settings.AppendLine($"{option.Title}: " + ((CustomStringOption) option).StringValue);
                }
                else if (option.GetType() == typeof(CustomOptionHeader))
                {
                    __instance.settings.AppendLine($"{option.Title}");
                }
            }

            __result = __instance.settings.ToString();
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        [HarmonyPostfix]
        private static void HudManagerUpdatePatch(HudManager __instance)
        {
            if (__instance.GameSettings == null)
                return;
            
            __instance.GameSettings.fontSizeMin =
                __instance.GameSettings.fontSizeMax = 
                    __instance.GameSettings.fontSize = HudTextSize;
            
            CreateScroller(__instance);

            var bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)) - Camera.main.transform.localPosition;
            
            OptionsScroller.YBounds = new FloatRange(-bottomLeft.y, Mathf.Max(-bottomLeft.y, __instance.GameSettings.renderedHeight - -bottomLeft.y + 0.02F));
        }

        //THIS BIT IS SKIDDED FROM ESSENTIALS: https://github.com/DorCoMaNdO/Reactor-Essentials
        private static void CreateScroller(HudManager hudManager)
        {
            if (OptionsScroller != null) return;

            OptionsScroller = new GameObject("OptionsScroller").AddComponent<Scroller>();
            OptionsScroller.transform.SetParent(hudManager.GameSettings.transform.parent);
            OptionsScroller.gameObject.layer = 5;

            OptionsScroller.transform.localScale = Vector3.one;
            OptionsScroller.allowX = false;
            OptionsScroller.allowY = true;
            OptionsScroller.active = true;
            OptionsScroller.velocity = new Vector2(0, 0);
            OptionsScroller.ScrollerYRange = new FloatRange(0, 0);
            OptionsScroller.enabled = true;

            OptionsScroller.Inner = hudManager.GameSettings.transform;
            hudManager.GameSettings.transform.SetParent(OptionsScroller.transform);
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        [HarmonyPostfix]
        private static void AmongUsClientOnPlayerJoinedPatch(AmongUsClient __instance,
            [HarmonyArgument(0)] ClientData client)
        {
            if (__instance.AmHost)
            {
                foreach (var option in OptionManager.CustomOptions)
                {
                    if (option.GetType() == typeof(CustomToggleOption))
                    {
                        Rpc<RpcUpdateSetting>.Instance.SendTo(client.Id, new RpcUpdateSetting.Data(option, ((CustomToggleOption) option).Value));
                    }
                    else if (option.GetType() == typeof(CustomNumberOption))
                    {
                        Rpc<RpcUpdateSetting>.Instance.SendTo(client.Id, new RpcUpdateSetting.Data(option, ((CustomNumberOption) option).Value));
                    }
                    else if (option.GetType() == typeof(CustomStringOption))
                    {
                        Rpc<RpcUpdateSetting>.Instance.SendTo(client.Id, new RpcUpdateSetting.Data(option, ((CustomStringOption) option).Value));
                    }
                }
            }
        }
    }
}