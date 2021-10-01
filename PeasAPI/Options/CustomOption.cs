namespace PeasAPI.Options
{
    public abstract class CustomOption
    {
        public string Title { get; set; }
        
        public string Id { get; internal set; }

        public bool HudVisible { get; set; } = true;
        
        public bool MenuVisible { get; set; } = true;
        
        public OptionBehaviour Option { get; internal set; }

        public CustomOption(string title)
        {
            Title = title;
        }
    }
}