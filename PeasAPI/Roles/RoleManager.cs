using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PeasAPI.CustomRpc;
using Reactor.Localization.Utilities;
using Reactor.Utilities.Extensions;
using Reactor.Networking.Rpc;
using AmongUs.GameOptions;
using UnityEngine;


namespace PeasAPI.Roles
{
    public static class RoleManager
    {
        public static List<byte> Crewmates => Utility.GetAllPlayers().Where(p => !p.Data.Role.IsImpostor).ToList().ConvertAll(p => p.PlayerId);

        public static List<byte> Impostors => Utility.GetAllPlayers().Where(p => p.Data.Role.IsImpostor).ToList().ConvertAll(p => p.PlayerId);

        public static List<BaseRole> Roles = new List<BaseRole>();

        public static int GetRoleId() => Roles.Count;
        

        public static void RegisterRole(BaseRole role) => Roles.Add(role);

        internal static RoleBehaviour ToRoleBehaviour(BaseRole baseRole)
        {
            if (GameObject.Find($"{baseRole.Name}-Role"))
            {
                return GameObject.Find($"{baseRole.Name}-Role").GetComponent<RoleBehaviour>();
            }

            var roleObject = new GameObject($"{baseRole.Name}-Role");
            roleObject.DontDestroy();
            
            
            var role = roleObject.AddComponent<RoleBehaviour>();
            role.StringName = CustomStringName.CreateAndRegister(baseRole.Name);
            role.BlurbName = CustomStringName.CreateAndRegister(baseRole.Description);
            role.BlurbNameLong = CustomStringName.CreateAndRegister(baseRole.LongDescription);
            role.BlurbNameMed = CustomStringName.CreateAndRegister(baseRole.Name);
            role.Role = (RoleTypes) (6 + baseRole.Id);
            
            var abilityButtonSettings = ScriptableObject.CreateInstance<AbilityButtonSettings>();
            abilityButtonSettings.Image = baseRole.Icon;
            abilityButtonSettings.Text = CustomStringName.CreateAndRegister(baseRole.Name);
            role.Ability = abilityButtonSettings;
            
            role.TeamType = baseRole.Team switch
            {
                Team.Alone => (RoleTeamTypes) 3,
                Team.Role => (RoleTeamTypes) 3,
                Team.Crewmate => RoleTeamTypes.Crewmate,
                Team.Impostor => RoleTeamTypes.Impostor,
                _ => RoleTeamTypes.Crewmate
            };
            role.MaxCount = baseRole.MaxCount;
            role.TasksCountTowardProgress = baseRole.HasToDoTasks;
            role.CanVent = baseRole.CanVent;
            role.CanUseKillButton = baseRole.CanKill();
            
            GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(role.Role, 0, 0);

            global::RoleManager.Instance.AllRoles.AddItem(role);
            
            return role;
        }
        
        public static void ResetRoles()
        {
            foreach (var _role in Roles)
            {
                _role.Members.Clear();
            }
        }
        
        public static void RpcResetRoles()
        {
            Rpc<RpcResetRoles>.Instance.Send();
        }
        
        public static BaseRole GetRole(int id)
        {
            foreach (var _role in Roles)
            {
                if (_role.Id == id)
                    return _role;
            }

            return null;
        }

        public static T GetRole<T>() where T : BaseRole
        {
            foreach (var _role in Roles)
            {
                if (_role.GetType() == typeof(T))
                    return (T) _role;
            }

            return null;
        }

        public static class HostMod
        {
            public static Dictionary<BaseRole, bool> IsRole { get; set; } = new Dictionary<BaseRole, bool>();
            public static bool IsImpostor;
        }
    }
}