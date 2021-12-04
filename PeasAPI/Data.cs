using System.Collections.Generic;
using UnityEngine;

namespace PeasAPI
{
    public class Data
    {
        public readonly struct CustomIntroScreen
        {
            public readonly string Team;
            public readonly string TeamDescription;
            public readonly Color TeamColor;
            public readonly List<byte> TeamMembers;
            public readonly string Role;
            public readonly string RoleDescription;
            public readonly Color RoleColor;

            public CustomIntroScreen(string team, string teamDescription, Color teamColor, List<byte> teamMembers, string role = null, string roleDescription= null, Color? roleColor = null)
            {
                Team = team;
                TeamColor = teamColor;
                TeamDescription = teamDescription;
                TeamMembers = teamMembers;
                Role = role.IsNullOrWhiteSpace() ? team : role;
                RoleDescription = roleDescription.IsNullOrWhiteSpace() ? teamDescription : roleDescription;
                RoleColor = roleColor ?? teamColor;
            }
        }
    }
}