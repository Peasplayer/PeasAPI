using Reactor;
using Reactor.Extensions;
using UnityEngine;

namespace PeasAPI.Options
{
    public class CustomOptionHeader : CustomOption
    {
        public CustomOptionHeader(string title) : base(title)
        {
            OptionManager.CustomOptions.Add(this);
        }

        internal OptionBehaviour CreateOption(ToggleOption toggleOptionPrefab)
        {
            ToggleOption header =
                Object.Instantiate(toggleOptionPrefab, toggleOptionPrefab.transform.parent);
                    
            header.TitleText.text = Title;
            header.Title = CustomStringName.Register(Title);
                    
            var checkBox = header.transform.FindChild("CheckBox")?.gameObject;
            if (checkBox) checkBox.Destroy();
                
            var background = header.transform.FindChild("Background")?.gameObject;
            if (background) background.Destroy();

            Option = header;
            HudFormat = "{0}";
            
            return header;
        }
    }
}