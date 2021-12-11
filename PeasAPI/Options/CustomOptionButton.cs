namespace PeasAPI.Options
{
    public class CustomOptionButton : CustomOption
    {
        public bool Value { get; private set; }
        
        public bool OldValue { get; private set; }

        public delegate void OnValueChangedHandler(CustomOptionButtonValueChangedArgs args);

        public event OnValueChangedHandler OnValueChanged;

        public class CustomOptionButtonValueChangedArgs
        {
            public CustomOptionButton Option;

            public bool OldValue;

            public bool NewValue;
            
            public CustomOptionButtonValueChangedArgs(CustomOptionButton option, bool oldValue, bool newValue)
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
                if (Option)
                    ((ToggleOption) Option).CheckMark.enabled = value;
            
                Value = value;
                OldValue = oldValue;
                
                ValueChanged(value, oldValue);
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
            var args = new CustomOptionButtonValueChangedArgs(this, oldValue, newValue);
            OnValueChanged?.Invoke(args);
        }
        
        public CustomOptionButton(string id, string title, bool defaultValue) : base(title)
        {
            OptionManager.CustomOptions.Add(this);
        }
    }
}