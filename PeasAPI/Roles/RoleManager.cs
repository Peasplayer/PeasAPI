using System.Collections.Generic;
using Hazel;

namespace PeasAPI.Roles
{
    public static class RoleManager
    {
        public static List<byte> Crewmates = new List<byte>();
        
        public static List<byte> Impostors = new List<byte>();

        public static List<BaseRole> Roles = new List<BaseRole>();

        public static byte GetRoleId() => (byte) Roles.Count;

        public static void RegisterRole(BaseRole role) => Roles.Add(role);
        
        public static void ResetRoles()
        {
            Crewmates.Clear();
            Impostors.Clear();
            
            foreach (var _role in RoleManager.Roles)
            {
                _role.Members.Clear();;
            }
        }
        
        public static void RpcResetRoles()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.ResetRole, SendOption.None, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        
        public static BaseRole GetRole(byte id)
        {
            foreach (var _role in RoleManager.Roles)
            {
                if (_role.Id == id)
                    return _role;
            }

            return null;
        }
    }
}