using System;
using PeasAPI.Roles;
using UnityEngine;

namespace PeasAPI.CustomButtons
{
    public class RoleButton : CustomButton
    {
        public RoleButton(Action OnClick, float Cooldown, Sprite Image, Vector2 PositionOffset, bool deadCanUse,
            BaseRole role,
            bool useText = false, string text = "", Vector2 textOffset = new Vector2()) : base(OnClick, Cooldown, Image,
            PositionOffset, deadCanUse, useText, text, textOffset)
        {
            Role = role;
            UseRole = true;
        }

        public RoleButton(Action OnClick, float Cooldown, Sprite Image, Vector2 PositionOffset, bool deadCanUse,
            float EffectDuration, Action OnEffectEnd, BaseRole role,
            bool useText = false, string text = "", Vector2 textOffset = new Vector2()) : base(OnClick, Cooldown, Image,
            PositionOffset, deadCanUse, EffectDuration, OnEffectEnd, useText, text, textOffset)
        {
            Role = role;
            UseRole = true;
        }
    }
}