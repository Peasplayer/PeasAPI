﻿using System;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP;
using AmongUs.GameOptions;

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

        public virtual bool AllowVanillaRoles { get; } = false;

        public virtual void OnGameStart() {}

        public virtual void OnUpdate() {}

        public virtual void OnKill(PlayerControl killer, PlayerControl victim) {}
        
        public virtual bool CanKill(PlayerControl killer, PlayerControl victim)
        {
            return true;
        }
        
        public virtual bool OnMeetingCall(PlayerControl caller, GameData.PlayerInfo target)
        {
            return true;
        }

        public virtual Data.CustomIntroScreen? GetIntroScreen(PlayerControl player) => new Data.CustomIntroScreen();

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