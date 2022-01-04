using System;
using System.Collections.Generic;
using BepInEx.IL2CPP;
using JetBrains.Annotations;
using PeasAPI.Managers;
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

        public virtual Type[] GameModeWhitelist { get; } = Array.Empty<Type>();

        public virtual float KillDistance { get; set; } = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];

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
        /// If a player should be able to see a person with this role.
        /// Only works with Visibility.Custom
        /// </summary>
        public virtual bool IsRoleVisible(PlayerControl playerWithRole, PlayerControl perspective)
        {
            return false;
        }
        
        public virtual bool _IsRoleVisible(PlayerControl playerWithRole, PlayerControl perspective)
        {
            if (playerWithRole.PlayerId == perspective.PlayerId)
                return true;

            if (playerWithRole.Data.IsDead && PeasAPI.ShowRolesOfDead.Value)
                return true;
            
            switch (this.Visibility)
            {
                case Visibility.Role: return perspective.IsRole(this);
                case Visibility.Impostor: return perspective.Data.Role.IsImpostor;
                case Visibility.Crewmate: return true;
                case Visibility.NoOne: return false;
                case Visibility.Custom: return this.IsRoleVisible(playerWithRole, perspective);
                default: throw new NotImplementedException("Unknown Visibility");
            }
        }
        
        /// <summary>
        /// This method calculates the nearest player to kill for a member of this role
        /// </summary>
        public virtual PlayerControl FindClosestTarget(PlayerControl from, bool protecting)
        {
            PlayerControl result = null;
            float num = KillDistance;
            if (!ShipStatus.Instance)
            {
                return null;
            }
            Vector2 truePosition = from.GetTruePosition();
            foreach (var playerInfo in GameData.Instance.AllPlayers)
            {
                if (!playerInfo.Disconnected && playerInfo.PlayerId != from.PlayerId && !playerInfo.IsDead && (playerInfo.Role.CanBeKilled || from.GetRole().CanKill(playerInfo.Object) || protecting) && !playerInfo.Object.inVent)
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object && @object.Collider.enabled)
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }

        public virtual bool ShouldGameEnd(GameOverReason reason) => true;
        
        /// <summary>
        /// Gets called when the game starts
        /// </summary>
        public virtual void OnGameStart()
        {
        }
        
        /// <summary>
        /// Gets called when the game stops
        /// </summary>
        public virtual void OnGameStop()
        {
        }

        internal void _OnUpdate()
        {
            foreach (var player in Members)
            {
                var playerControl = player.GetPlayer();
                if (playerControl == null)
                    continue;
                if (PlayerControl.LocalPlayer == null)
                    continue;
                if (playerControl.IsRole(this) && _IsRoleVisible(playerControl, PlayerControl.LocalPlayer))
                {
                    playerControl.nameText.color = this.Color;
                    playerControl.nameText.text = $"{player.GetPlayer().name}\n{Name}";
                }
            }

            OnUpdate();
        }

        /// <summary>
        /// Gets called every frame
        /// </summary>
        public virtual void OnUpdate()
        {
        }

        internal void _OnMeetingUpdate(MeetingHud __instance)
        {
            if (PlayerMenuManager.IsMenuOpen)
                return;
            
            foreach (var player in Members)
            {
                var playerControl = player.GetPlayer();
                if (playerControl == null)
                    continue;
                if (PlayerControl.LocalPlayer == null)
                    continue;
                if (playerControl.IsRole(this) && _IsRoleVisible(playerControl, PlayerControl.LocalPlayer))
                {
                    playerControl.nameText.color = this.Color;
                    playerControl.nameText.text = $"{player.GetPlayer().name}\n{Name}";
                }
            }

            foreach (var pstate in __instance.playerStates)
            {
                var player = pstate.TargetPlayerId.GetPlayer();
                if (player == null)
                    continue;
                if (PlayerControl.LocalPlayer == null)
                    continue;
                if (player.IsRole(this) && _IsRoleVisible(player, PlayerControl.LocalPlayer))
                {
                    pstate.NameText.color = Color;
                    pstate.NameText.text = $"{player.name}\n{Name}";
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

        public virtual bool PreKill(PlayerControl killer, PlayerControl victim)
        {
            return true;
        }
        
        public virtual void OnKill(PlayerControl killer, PlayerControl victim)
        {
        }
        
        public virtual bool PreExile(PlayerControl victim)
        {
            return true;
        }
        
        public virtual void OnExiled(PlayerControl victim)
        {
        }

        public virtual void OnRevive(PlayerControl player)
        {
        }
        
        public virtual void OnTaskComplete(PlayerControl player, PlayerTask task)
        {
        }

        public BaseRole(BasePlugin plugin)
        {
            Id = RoleManager.GetRoleId();
            RoleManager.RegisterRole(this);
        }
    }
}
