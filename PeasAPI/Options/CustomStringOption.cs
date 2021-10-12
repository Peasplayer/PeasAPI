using BepInEx.Configuration;
using System.Collections.Generic;
using System.Reflection;
using PeasAPI.CustomRpc;
using Reactor;
using Reactor.Networking;

namespace PeasAPI.Options
{
    public class CustomStringOption : CustomOption
    {
        public int Value { get; private set; }

        public string StringValue => Values[Value].GetTranslation();
        
        public int OldValue { get; private set; }
        
        public List<StringNames> Values { get; private set; }

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
            
            if (AmongUsClient.Instance.AmHost)
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
        
        public CustomStringOption(string id, string title, params string[] values) : base(title)
        {
            Id = $"{Assembly.GetCallingAssembly().GetName().Name}.StringOption.{id}";
            _configEntry = PeasApi.ConfigFile.Bind("Options", Id, 0);
            
            Value = _configEntry.Value;
            Values = new List<StringNames>();
            foreach (var value in values)
                Values.Add((StringNames)CustomStringName.Register(value));
            
            OptionManager.CustomOptions.Add(this);
        }
    }
}