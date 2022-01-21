namespace PeasAPI.Options
{
    public abstract class CustomOption
    {
        public string Title { get; set; }
        
        public string Id { get; internal set; }

        public bool HudVisible { get; set; } = true;
        
        public bool MenuVisible { get; set; } = true;
        
        public bool AdvancedRoleOption { get; set; }

        public string HudFormat { get; set; } = "{0}";

        internal bool IsFromPeasAPI { get; set; } = false;
        
        public OptionBehaviour Option { get; internal set; }

        public CustomOption(string title)
        {
            Title = title;
        }
    }
}