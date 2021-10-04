using System;
using System.Reflection;
using BepInEx.Configuration;
using PeasAPI.CustomRpc;
using Reactor.Networking;

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
        
        public void ValueChanged(bool newValue, bool oldValue)
        {
            var args = new CustomToggleOptionValueChangedArgs(this, oldValue, newValue);
            OnValueChanged?.Invoke(args);
        }
        
        public CustomToggleOption(string id, string title, bool defaultValue) : base(title)
        {
            Id = $"{Assembly.GetCallingAssembly().GetName().Name}.ToggleOption.{id}";
            _configEntry = PeasApi.ConfigFile.Bind("Options", Id, defaultValue);
            
            Value = _configEntry.Value;
            
            OptionManager.CustomOptions.Add(this);
        }
    }
}