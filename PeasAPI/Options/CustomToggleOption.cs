using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using PeasAPI.CustomRpc;
using Reactor;
using Reactor.Networking;
using UnityEngine;
using Object = System.Object;

namespace PeasAPI.Options
{
    public class CustomToggleOption : CustomOption
    {
        public bool Value { get; private set; }
        
        public bool OldValue { get; private set; }

        public delegate void OnValueChangedHandler(CustomToggleOptionValueChangedArgs args);

        public event OnValueChangedHandler OnValueChanged;
        
        private ConfigEntry<bool> _configEntry;

        public class CustomToggleOptionValueChangedArgs
        {
            public CustomToggleOption Option;

            public bool OldValue;

            public bool NewValue;
            
            public CustomToggleOptionValueChangedArgs(CustomToggleOption option, bool oldValue, bool newValue)
            {
                Option = option;
                OldValue = oldValue;
                NewValue = newValue;
            }
        }

        public void SetValue(bool value)
        {
            var oldValue = !value;
            
            if (AmongUsClient.Instance.AmHost)
            {
                if (_configEntry != null)
                 _configEntry.Value = value;
                
                if (Option)
                    ((ToggleOption) Option).CheckMark.enabled = value;
            
                Value = value;
                OldValue = oldValue;
                
                ValueChanged(value, oldValue);
                
                Rpc<RpcUpdateSetting>.Instance.Send(new RpcUpdateSetting.Data(this, value));
            }
            else
            {
                if (Option)
                    ((StringOption) Option).Value = value ? 0 : 1;
            
                Value = value;
                OldValue = oldValue;
                
                ValueChanged(value, oldValue);
            }
        }
        
        internal void ValueChanged(bool newValue, bool oldValue)
        {
            var args = new CustomToggleOptionValueChangedArgs(this, oldValue, newValue);
            OnValueChanged?.Invoke(args);
        }

        internal OptionBehaviour CreateOption(ToggleOption toggleOptionPrefab, StringOption stringOptionPrefab)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                ToggleOption toggleOption =
                    UnityEngine.Object.Instantiate(toggleOptionPrefab, toggleOptionPrefab.transform.parent);

                Option = toggleOption;

                toggleOption.TitleText.text = Title;
                toggleOption.Title = CustomStringName.Register(Title);
                toggleOption.CheckMark.enabled = Value;

                toggleOption.OnValueChanged = new Action<OptionBehaviour>(behaviour =>
                {
                    SetValue(!toggleOption.oldValue);
                });

                return toggleOption;
            }
            else
            {
                StringOption toggleOption =
                    UnityEngine.Object.Instantiate(stringOptionPrefab, stringOptionPrefab.transform.parent);

                Option = toggleOption;

                toggleOption.TitleText.text = Title;
                toggleOption.Title = CustomStringName.Register(Title);
                toggleOption.Value = Value ? 0 : 1;

                var values = new List<StringNames>();
                values.Add(CustomStringName.Register("On"));
                values.Add(CustomStringName.Register("Off"));
                toggleOption.Values = values.ToArray();

                toggleOption.OnValueChanged = new Action<OptionBehaviour>(behaviour =>
                {
                    SetValue(toggleOption.Value == 0);
                });

                return toggleOption;
            }
        }
        
        public CustomToggleOption(string id, string title, bool defaultValue) : base(title)
        {
            Id = $"{Assembly.GetCallingAssembly().GetName().Name}.ToggleOption.{id}";
            try
            {
                _configEntry = PeasAPI.ConfigFile.Bind("Options", Id, defaultValue);
            }
            catch (Exception e)
            {
                PeasAPI.Logger.LogError($"Error while loading the option \"{title}\": {e.Source}");
            }
            
            Value = _configEntry?.Value ?? defaultValue;
            HudFormat = "{0}: {1}";
            
            OptionManager.CustomOptions.Add(this);
        }
    }
}