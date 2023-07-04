using System;
using System.Linq;
using HarmonyLib;
using PeasAPI.Roles;
using Reactor.Localization.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using AmongUs.GameOptions;

namespace PeasAPI.Options
{
    public class CustomRoleOption : CustomOption
    {
        public static RoleOptionsData optionsData;
        public BaseRole Role;
        
        public int Count;
        
        public int Chance;
        
        public CustomOption[] AdvancedOptions;
        
        public string AdvancedOptionPrefix;public delegate void OnValueChangedHandler(CustomRoleOptionValueChangedArgs args);

        public event OnValueChangedHandler OnValueChanged;

        public class CustomRoleOptionValueChangedArgs
        {
            public CustomRoleOption Option;

            public int Count;
            
            public int OldCount;

            public int Chance;
            
            public int OldChance;
            
            public CustomRoleOptionValueChangedArgs(CustomRoleOption option, int count, int oldCount, int chance, int oldChance)
            {
                Option = option;
                Count = count;
                OldCount = oldCount;
                Chance = chance;
                OldChance = oldChance;
            }
        }

        public void SetValue(int count, int chance)
        {
            var oldCount = Count;
            var oldChance = Chance;

            Count = count;
            Chance = chance;
            
            ValueChanged(count, oldCount, chance, oldChance);
        }
        
        internal void ValueChanged(int count, int oldCount, int chance, int oldChance)
        {
            var args = new CustomRoleOptionValueChangedArgs(this, count, oldCount, chance, oldChance);
            OnValueChanged?.Invoke(args);
        }

        internal OptionBehaviour CreateOption(RoleOptionSetting roleOptionPrefab)
        {
            if (Option != null)
            {
                return Option;
            }
            var newSetting = Object.Instantiate(roleOptionPrefab, roleOptionPrefab.transform.parent);
            newSetting.name = Role.Name;
            newSetting.Role = Role.RoleBehaviour;
            newSetting.SetRole(GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions);
            
            if (!optionsData.roleRates.ContainsKey(Role.RoleBehaviour.Role))
                optionsData.roleRates[Role.RoleBehaviour.Role] =
                    new AmongUs.GameOptions.RoleRate();
            var rates = optionsData.roleRates[Role.RoleBehaviour.Role];
            Count = rates.MaxCount;
            Chance = rates.Chance;
            
            Option = newSetting;
            return newSetting;
        }
        
        internal AdvancedRoleSettingsButton CreateOptionObjects(GameObject roleTabTemplate)
        {
            if (OptionManager.NumberOptionPrefab == null || OptionManager.ToggleOptionPrefab == null ||
                OptionManager.StringOptionPrefab == null)
                return null;
            
            var tab = Object.Instantiate(roleTabTemplate, roleTabTemplate.transform.parent);
            tab.name = Role.Name + " Settings";

            if (AdvancedOptions.Length == 0 && Option != null)
            {
                Option.transform.FindChild("More Options").gameObject.SetActive(false);
            }
            
            foreach (var option in tab.GetComponentsInChildren<OptionBehaviour>())
            {
                option.gameObject.DestroyImmediate();
            }

            foreach (var advancedOption in AdvancedOptions)
            {
                OptionBehaviour optionBehaviour = null;
                switch (advancedOption)
                {
                    case CustomNumberOption option:
                        optionBehaviour = option.CreateOption(OptionManager.NumberOptionPrefab);
                        break;
                    case CustomToggleOption option:
                        optionBehaviour = option.CreateOption(OptionManager.ToggleOptionPrefab, OptionManager.StringOptionPrefab);
                        break;
                    case CustomStringOption option:
                        optionBehaviour = option.CreateOption(OptionManager.StringOptionPrefab);
                        break;
                }

                optionBehaviour.Title = CustomStringName.CreateAndRegister(advancedOption.Title);
                optionBehaviour.name = advancedOption.Title;

                var optionTransform = optionBehaviour.transform;
                optionTransform.parent = tab.transform;
                optionTransform.localScale = Vector3.one;
                optionTransform.localPosition =
                    new Vector3(-1.25f, 0.06f - AdvancedOptions.ToList().IndexOf(advancedOption) * 0.56f, 0f);
            }
            
            var roleName = tab.transform.FindChild("Role Name");
            roleName.GetComponent<TextTranslatorTMP>().Destroy();
            roleName.GetComponent<TextMeshPro>().text = Role.Name;
            
            var advancedOptions = new AdvancedRoleSettingsButton
            {
                Tab = tab,
                Type = Role.RoleBehaviour.Role
            };
            
            return advancedOptions;
        }

        public CustomRoleOption(BaseRole role, string advancedOptionPrefix, params CustomOption[] advancedOptions) : base(role.Name)
        {
            Role = role;
            AdvancedOptions = advancedOptions;
            AdvancedOptions.Do(option => option.AdvancedRoleOption = true );
            AdvancedOptionPrefix = advancedOptionPrefix;
            HudFormat = "{0}: {1} with {2}% Chance";
            
            OptionManager.CustomRoleOptions.Add(this);
        }
    }
}