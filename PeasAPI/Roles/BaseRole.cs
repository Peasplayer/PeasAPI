using System.Collections.Generic;
using System.Linq;
using BepInEx.IL2CPP;
using UnityEngine;

namespace PeasAPI.Roles
{
    public abstract class BaseRole
    {
        public int Id { get; }

        public List<byte> Members = new List<byte>();

        /// <summary>
        /// The name of the Role. Will displayed at the intro, ejection and task list
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The description of the Role. Will displayed at the intro
        /// </summary>
        public abstract string Description { get; }
        
        /// <summary>
        /// The description of the Role at the task list
        /// </summary>
        public abstract string TaskText { get; }

        /// <summary>
        /// The color of the Role. Will displayed at the intro, name, task list, game end
        /// </summary>
        public abstract Color Color { get; }

        /// <summary>
        /// Who can see the identity of the player with the Role
        /// </summary>
        public abstract Visibility Visibility { get; }

        /// <summary>
        /// Who the player with the Role is in a team
        /// </summary>
        public abstract Team Team { get; }

        /// <summary>
        /// Whether the player should get tasks
        /// </summary>
        public abstract bool HasToDoTasks { get; }

        /// <summary>
        /// How many player should get the Role
        /// </summary>
        public virtual int Limit { get; set; } = 0;

        /// <summary>
        /// If a member of the role should be able to kill that player / in general
        /// </summary>
        public virtual bool CanKill(PlayerControl victim = null)
        {
            return false;
        }

        /// <summary>
        /// If a member of the role should be able to use vents
        /// </summary>
        public virtual bool CanVent { get; } = false;
        
        /// <summary>
        /// If a member of the role should be able to sabotage that sabotage type / in general
        /// </summary>
        public virtual bool CanSabotage(SystemTypes? sabotage)
        {
            return false;
        }
        
        /// <summary>
        /// This method calculates the nearest player to kill for a member of this role
        /// </summary>
        public virtual PlayerControl FindClosesTarget(PlayerControl from)
        {
            var distance = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            
            if (!ShipStatus.Instance)
                return null;
            
            Vector2 truePosition = from.GetTruePosition();
            
            PlayerControl result = null;
            foreach (var playerInfo in GameData.Instance.AllPlayers)
            {
                PlayerControl @object = playerInfo.Object;
                if (!playerInfo.Disconnected && playerInfo.PlayerId != from.PlayerId && !playerInfo.IsDead && CanKill(@object))
                {
                    if (@object && @object.Collider.enabled)
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= distance && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            distance = magnitude;
                        }
                    }
                }
            }
            
            return result;
        }

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
            if (PlayerControl.LocalPlayer.IsRole(this))
            {
                PlayerControl.LocalPlayer.nameText.color = Color;
                PlayerControl.LocalPlayer.nameText.text = $"{PlayerControl.LocalPlayer.name}\n{Name}";
                if (Visibility == Visibility.Role)
                    foreach (var player in Members)
                        player.GetPlayer().nameText.color = Color;
            }
            else if (PlayerControl.LocalPlayer.Data.IsImpostor)
            {
                if (Visibility == Visibility.Impostor)
                    foreach (var player in Members)
                        player.GetPlayer().nameText.color = Color;
            }
            else if (PlayerControl.LocalPlayer.GetRole() == null)
            {
                if (Visibility == Visibility.Crewmate)
                    foreach (var player in Members)
                        player.GetPlayer().nameText.color = Color;
            }

            foreach (var pstate in __instance.playerStates)
            {
                if (pstate.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId &&
                    PlayerControl.LocalPlayer.IsRole(this))
                {
                    pstate.NameText.color = Color;
                    pstate.NameText.text = $"{PlayerControl.LocalPlayer.name}\n{Name}";
                }

                if (pstate.TargetPlayerId.GetPlayer().IsRole(this))
                {
                    if (PlayerControl.LocalPlayer.IsRole(this))
                    {
                        if (Visibility == Visibility.Role)
                            pstate.NameText.color = Color;
                    }
                    else if (PlayerControl.LocalPlayer.Data.IsImpostor)
                    {
                        if (Visibility == Visibility.Impostor)
                            pstate.NameText.color = Color;
                    }
                    else if (PlayerControl.LocalPlayer.GetRole() == null)
                    {
                        if (Visibility == Visibility.Crewmate)
                            pstate.NameText.color = Color;
                    }
                }
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
        }
    }
}