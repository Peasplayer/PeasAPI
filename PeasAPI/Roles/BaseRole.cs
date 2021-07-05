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

        /// <summary>
        /// The name of the Role. Will displayed at the intro, ejection and task list
        /// </summary>
        public virtual string Name { get; } = "Role";

        /// <summary>
        /// The description of the Role. Will displayed at the intro
        /// </summary>
        public virtual string Description { get; } = "Do something";
        
        /// <summary>
        /// The description of the Role at the task list
        /// </summary>
        public virtual string TaskText { get; } = null;

        /// <summary>
        /// The color of the Role. Will displayed at the intro, name, task list, game end
        /// </summary>
        public virtual Color Color { get; } = Color.white;

        /// <summary>
        /// Who can see the identity of the player with the Role
        /// </summary>
        public virtual Visibility Visibility { get; } = Visibility.NoOne;

        /// <summary>
        /// Who the player with the Role is in a team
        /// </summary>
        public virtual Team Team { get; } = Team.Alone;

        /// <summary>
        /// How many player should get the Role
        /// </summary>
        public virtual int Limit { get; set; } = 0;

        public List<byte> Members = new List<byte>();

        public void _OnGameStart()
        {
            OnGameStart();
        }

        /// <summary>
        /// Gets called when the game starts
        /// </summary>
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

        /// <summary>
        /// Gets called every frame
        /// </summary>
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

        /// <summary>
        /// Gets called every frame when a meeting is active. The meeting gets passed on
        /// </summary>
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