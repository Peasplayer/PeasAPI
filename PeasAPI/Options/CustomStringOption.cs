using System;
using BepInEx.Configuration;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using PeasAPI.CustomRpc;
using Reactor.Localization.Utilities;
using Reactor.Networking.Rpc;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PeasAPI.Options
{
    public class CustomStringOption : CustomOption
    {
        public int Value { get; private set; }

        public string StringValue
        {
            get
            {
                if (Values.Count >= Value + 1)
                    return Values[Value].GetTranslation();
                return "Error";
            }
        }

        public int OldValue { get; private set; }
        
        public List<StringNames> Values { get; set; }

        public delegate void OnValueChangedHandler(CustomStringOptionValueChangedArgs args);

        public event OnValueChangedHandler OnValueChanged;
        
        private ConfigEntry<int> _configEntry;

        public class CustomStringOptionValueChangedArgs
        {
            public CustomStringOption Option;

            public int OldValue;

            public int NewValue;
            
            public CustomStringOptionValueChangedArgs(CustomStringOption option, int oldValue, int newValue)
            {
                Option = option;
                OldValue = oldValue;
                NewValue = newValue;
            }
        }

        public void SetValue(int value)
        {
            var oldValue = Value;
            
            if (AmongUsClient.Instance.AmHost && _configEntry != null)
                _configEntry.Value = value;
                
            Value = value;
            if (Option != null)
                ((StringOption) Option).Value = value;
            OldValue = oldValue;
                
            ValueChanged(value, oldValue);

            if (AmongUsClient.Instance.AmHost)
                Rpc<RpcUpdateSetting>.Instance.Send(new RpcUpdateSetting.Data(this, value));
        }
        
        public void ValueChanged(int newValue, int oldValue)
        {
            var args = new CustomStringOptionValueChangedArgs(this, oldValue, newValue);
            OnValueChanged?.Invoke(args);
        }

        internal OptionBehaviour CreateOption(StringOption stringOptionPrefab)
        {
            StringOption stringOption =
                Object.Instantiate(stringOptionPrefab, stringOptionPrefab.transform.parent);
                    
            stringOption.TitleText.text = Title;
            stringOption.Title = CustomStringName.CreateAndRegister(Title);
            stringOption.Value = Value;
            stringOption.ValueText.text = StringValue;
            stringOption.Values = Values.ToArray();
                    
            Option = stringOption;

            stringOption.OnValueChanged = new Action<OptionBehaviour>(behaviour =>
            {
                SetValue(stringOption.Value);
            });
            
            return stringOption;
        }
        
        public CustomStringOption(string id, string title, params string[] values) : base(title)
        {
            Id = $"{Assembly.GetCallingAssembly().GetName().Name}.StringOption.{id}";
            try
            {
                _configEntry = PeasAPI.ConfigFile.Bind("Options", Id, 0);
            } catch (Exception e) {
                PeasAPI.Logger.LogError($"Error while loading the option \"{title}\": {e.Source}");
            }
            
            Value = _configEntry?.Value ?? 0;
            Values = new List<StringNames>();
            foreach (var value in values)
                Values.Add((StringNames)CustomStringName.CreateAndRegister(value));
            HudFormat = "{0}: {1}";

            OptionManager.CustomOptions.Add(this);
        }
    }
}