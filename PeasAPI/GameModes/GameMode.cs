using System;
using System.Collections.Generic;
using BepInEx.IL2CPP;

namespace PeasAPI.GameModes
{
    public class GameMode
    {
        public GameMode(BasePlugin plugin)
        {
            Id = GameModeManager.GetModeId();
            GameModeManager.RegisterMode(this);
        }
        
        public byte Id { get; } = byte.MaxValue;

        public virtual string Name { get; } = "Super cool mode";

        public virtual bool Enabled => GameModeManager.IsGameModeActive(this);

        public virtual bool HasToDoTasks { get; } = false;

        public virtual Type[] RoleWhitelist { get; } = Array.Empty<Type>();

        public virtual void OnGameStart() {}

        public virtual void OnUpdate() {}

        public virtual bool OnKill(PlayerControl killer, PlayerControl victim)
        {
            return true;
        }
        
        public virtual bool OnMeetingCall(PlayerControl caller, GameData.PlayerInfo target)
        {
            return true;
        }

        public virtual Data.CustomIntroScreen? GetIntroScreen(PlayerControl player)
        {
            return new Data.CustomIntroScreen(true, "Impostor God", "UwU overpowered", Palette.Purple, new List<byte> {PlayerControl.LocalPlayer.PlayerId});
        }

        public virtual string GetObjective(PlayerControl player)
        {
            return null;
        }

        public virtual void AssignRoles() {}

        public virtual RoleTypes? AssignLocalRole(PlayerControl player) => null;
        
        public virtual bool AllowSabotage(SystemTypes? sabotage)
        {
            return false;
        }

        public virtual bool ShouldGameStop(GameOverReason reason)
        {
            return true;
        }
    }
}