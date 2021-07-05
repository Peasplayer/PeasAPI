using System;
using System.Collections.Generic;
using System.ComponentModel;
using BepInEx.IL2CPP;
using PeasAPI;
using Rewired;
using UnityEngine;

namespace PeasAPI.Roles
{
    public class BaseRole
    {
        public byte Id { get; } = byte.MaxValue;

        public virtual string Name { get; } = "Role";

        public virtual string Description { get; } = "Do something";
        
        public virtual string TaskText { get; } = null;

        public virtual Color Color { get; } = Color.white;

        public virtual Visibility Visibility { get; } = Visibility.NoOne;

        public virtual Team Team { get; } = Team.Alone;

        public virtual int Limit { get; set; } = 0;

        public List<byte> Members = new List<byte>();

        public void _OnGameStart()
        {
            OnGameStart();
        }

        public virtual void OnGameStart()
        {
        }

        public void _OnUpdate()
        {
            if (PlayerControl.LocalPlayer.IsRole(this))
            {
                PlayerControl.LocalPlayer.nameText.color = this.Color;
                PlayerControl.LocalPlayer.nameText.text = $"{PlayerControl.LocalPlayer.name}\n{Name}";
                if (this.Visibility == Visibility.Role)
                    foreach (var player in Members)
                        player.GetPlayer().nameText.color = this.Color;
            }
            else if (PlayerControl.LocalPlayer.Data.IsImpostor)
            {
                if (this.Visibility == Visibility.Impostor)
                    foreach (var player in Members)
                        player.GetPlayer().nameText.color = this.Color;
            }
            else if (PlayerControl.LocalPlayer.GetRole() == null)
            {
                if (this.Visibility == Visibility.Crewmate)
                    foreach (var player in Members)
                        player.GetPlayer().nameText.color = this.Color;
            }

            OnUpdate();
        }

        public virtual void OnUpdate()
        {
        }

        public void _OnMeetingUpdate(MeetingHud __instance)
        {
            foreach (var pstate in __instance.playerStates)
                if (pstate.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId &&
                    PlayerControl.LocalPlayer.IsRole(this))
                {
                    pstate.NameText.color = this.Color;
                    pstate.NameText.text = $"{PlayerControl.LocalPlayer.name}\n{Name}";
                }
            OnMeetingUpdate(__instance);
        }

        public virtual void OnMeetingUpdate(MeetingHud meeting)
        {
        }

        public BaseRole(BasePlugin plugin)
        {
            Id = RoleManager.GetRoleId();
            RoleManager.RegisterRole(this);

            if (TaskText == null)
                TaskText = Description;
        }
    }
}