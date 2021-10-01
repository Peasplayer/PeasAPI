namespace PeasAPI.Options
{
    public class CustomOptionHeader : CustomOption
    {
        public CustomOptionHeader(string title) : base(title)
        {
            OptionManager.CustomOptions.Add(this);
        }
    }
}