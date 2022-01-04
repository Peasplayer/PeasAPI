using System;
using System.Reflection;
using BepInEx.Configuration;
using PeasAPI.CustomRpc;
using Reactor;
using Reactor.Networking;
using Object = UnityEngine.Object;

namespace PeasAPI.Options
{
    public class CustomNumberOption : CustomOption
    {
        public float Value { get; private set; }
        
        public float OldValue { get; private set; }

        public float MinValue { get; set; }
        
        public float MaxValue { get; set; }
        
        public float Increment { get; set; }
        
        public NumberSuffixes SuffixType { get; set; }

        public delegate void OnValueChangedHandler(CustomNumberOptionValueChangedArgs args);

        public event OnValueChangedHandler OnValueChanged;
        
        private ConfigEntry<float> _configEntry;

        public class CustomNumberOptionValueChangedArgs
        {
            public CustomNumberOption Option;

            public float OldValue;

            public float NewValue;
            
            public CustomNumberOptionValueChangedArgs(CustomNumberOption option, float oldValue, float newValue)
            {
                Option = option;
                OldValue = oldValue;
                NewValue = newValue;
            }
        }

        public void SetValue(float value)
        {
            var oldValue = Value;
            
            if (AmongUsClient.Instance.AmHost && _configEntry != null)
                _configEntry.Value = value;
            
            Value = value;
            OldValue = oldValue;
                
            ValueChanged(value, oldValue);

            if (AmongUsClient.Instance.AmHost)
                Rpc<RpcUpdateSetting>.Instance.Send(new RpcUpdateSetting.Data(this, value));
        }
        
        public void ValueChanged(float newValue, float oldValue)
        {
            var args = new CustomNumberOptionValueChangedArgs(this, oldValue, newValue);
            OnValueChanged?.Invoke(args);
        }

        internal OptionBehaviour CreateOption(NumberOption numberOptionPrefab)
        {
            NumberOption numberOption =
                Object.Instantiate(numberOptionPrefab, numberOptionPrefab.transform.parent);
                    
            numberOption.TitleText.text = Title;
            numberOption.Title = CustomStringName.Register(Title);
            numberOption.Value = Value;
            numberOption.ValidRange = new FloatRange(MinValue, MaxValue);
            numberOption.Increment = Increment;
            numberOption.SuffixType = SuffixType;

            Option = numberOption;

            numberOption.OnValueChanged = new Action<OptionBehaviour>(behaviour =>
            {
                SetValue(numberOption.Value);
            });

            return numberOption;
        }
        
        public CustomNumberOption(string id, string title, float minValue, float maxValue, float increment, float defaultValue, NumberSuffixes suffixType) : base(title)
        {
            Id = $"{Assembly.GetCallingAssembly().GetName().Name}.NumberOption.{id}";
            try
            {
                _configEntry = PeasAPI.ConfigFile.Bind("Options", Id, defaultValue);
            }
            catch (Exception e)
            {
                PeasAPI.Logger.LogError($"Error while loading the option \"{title}\": {e.Source}");
            }

            Value = _configEntry?.Value ?? defaultValue;
            MinValue = minValue;
            MaxValue = maxValue;
            Increment = increment;
            SuffixType = suffixType;
            
            OptionManager.CustomOptions.Add(this);
        }
    }
}