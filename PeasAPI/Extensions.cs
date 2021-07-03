using Hazel;
using PeasAPI.Roles;

namespace PeasAPI
{
    public static class Extensions
    {
        public static PlayerControl GetPlayer(this byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == id)
                {
                    return player;
                }
            }

            return null;
        }
        
        public static GameData.PlayerInfo GetPlayerInfo(this byte id)
        {
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                if (player.PlayerId == id)
                    return player;
            }
            return null;
        }

        #region Roles

        public static BaseRole GetRole(this PlayerControl player)
        {
            foreach (var _role in RoleManager.Roles)
            {
                if (_role.Members.Contains(player.PlayerId))
                    return _role;
            }

            return null;
        }
        
        public static BaseRole GetRole(this GameData.PlayerInfo player)
        {
            foreach (var _role in RoleManager.Roles)
            {
                if (_role.Members.Contains(player.PlayerId))
                    return _role;
            }

            return null;
        }

        public static bool IsRole(this PlayerControl player, BaseRole role) => player.GetRole() == role;

        #nullable enable
        public static T? GetRole<T>(this PlayerControl player) where T : BaseRole
            => player.GetRole() as T;

        public static bool IsRole<T>(this PlayerControl player) where T : BaseRole
            => player.GetRole<T>() != null;

        public static void SetRole(this PlayerControl player, BaseRole? role)
        {
            foreach (var _role in RoleManager.Roles)
            {
                if (_role != role)
                    _role.Members.Remove(player.PlayerId);
            }

            if (role != null)
            {
                role.Members.Add(player.PlayerId);
            }
        }

        public static void RpcSetRole(this PlayerControl player, BaseRole? role)
        {
            MessageWriter writer =
                AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte) CustomRpc.SetRole, SendOption.None, -1);
            writer.Write(player.PlayerId);
            writer.Write(role.Id);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            player.SetRole(role);
        }

        #endregion Roles
    }
}