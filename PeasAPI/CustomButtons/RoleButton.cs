using System;
using PeasAPI.Roles;
using UnityEngine;

namespace PeasAPI.CustomButtons
{
    public class RoleButton : CustomButton
    {
        public RoleButton(Action OnClick, float Cooldown, Sprite Image, Vector2 PositionOffset, bool deadCanUse, BaseRole role) : base(OnClick, Cooldown, Image, PositionOffset, deadCanUse)
        {
            Role = role;
            UseRole = true;
        }
        
        public RoleButton(Action OnClick, float Cooldown, Sprite Image, Vector2 PositionOffset, bool deadCanUse, float EffectDuration, Action OnEffectEnd, BaseRole role) : base(OnClick, Cooldown, Image, PositionOffset, deadCanUse, EffectDuration, OnEffectEnd)
        {
            Role = role;
            UseRole = true;
        }
    }
}