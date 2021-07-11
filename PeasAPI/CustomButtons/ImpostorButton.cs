using System;
using UnityEngine;

namespace PeasAPI.CustomButtons
{
    public class ImpostorButton : CustomButton
    {
        public ImpostorButton(Action OnClick, float Cooldown, Sprite Image, Vector2 PositionOffset, bool deadCanUse) : base(OnClick, Cooldown, Image, PositionOffset, deadCanUse)
        {
            ImpostorButton = true;
        }
        
        public ImpostorButton(Action OnClick, float Cooldown, Sprite Image, Vector2 PositionOffset, bool deadCanUse, float EffectDuration, Action OnEffectEnd) : base(OnClick, Cooldown, Image, PositionOffset, deadCanUse, EffectDuration, OnEffectEnd)
        {
            ImpostorButton = true;
        }
    }
}