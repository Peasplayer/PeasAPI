using System.Reflection;
using BepInEx.Configuration;
using PeasAPI.CustomRpc;
using Reactor.Networking;

namespace PeasAPI.Options
{
    public class CustomNumberOption : CustomOption
    {
        public float Value { get; private set; }
        
        public float OldValue { get; private set; }

        public float MinValue { get; private set; }
        
        public float MaxValue { get; private set; }
        
        public float Increment { get; private set; }

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
            
            if (AmongUsClient.Instance.AmHost)
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
        
        public CustomNumberOption(string id, string title, float minValue, float maxValue, float increment, float defaultValue) : base(title)
        {
            Id = $"{Assembly.GetCallingAssembly().GetName().Name}.NumberOption.{id}";
            _configEntry = PeasApi.ConfigFile.Bind("Options", Id, defaultValue);

            Value = _configEntry.Value;
            MinValue = minValue;
            MaxValue = maxValue;
            Increment = increment;
            
            OptionManager.CustomOptions.Add(this);
        }
    }
}