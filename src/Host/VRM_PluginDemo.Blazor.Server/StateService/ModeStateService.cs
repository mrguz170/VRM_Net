namespace VRM_Plugin.Blazor.Server.StateService
{
    public class ModeStateService
    {

        public string Mode { get; set; } = "light";

        public event Action? ModeChanged;

        public void SetMode(string mode)
        {
            Mode = mode;
            ModeChanged?.Invoke();
        }
    }
} 