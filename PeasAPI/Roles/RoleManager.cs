using System.Collections.Generic;
using Hazel;
using PeasAPI.CustomRpc;
using Reactor.Networking;

namespace PeasAPI.Roles
{
    public static class RoleManager
    {
        public static List<byte> Crewmates = new List<byte>();
        
        public static List<byte> Impostors = new List<byte>();

        public static List<BaseRole> Roles = new List<BaseRole>();

        public static int GetRoleId() => Roles.Count;

        public static void RegisterRole(BaseRole role) => Roles.Add(role);
        
        public static void ResetRoles()
        {
            Crewmates.Clear();
            Impostors.Clear();
            
            foreach (var _role in RoleManager.Roles)
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
            foreach (var _role in RoleManager.Roles)
            {
                if (_role.Id == id)
                    return _role;
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