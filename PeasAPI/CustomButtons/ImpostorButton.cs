using System;
using UnityEngine;

namespace PeasAPI.CustomButtons
{
    public class ImpostorButton : CustomButton
    {
        public ImpostorButton(Action OnClick, float Cooldown, Sprite Image, Vector2 PositionOffset, bool deadCanUse,
            bool useText = false, string text = "", Vector2 textOffset = new Vector2()) : base(OnClick, Cooldown, Image,
            PositionOffset, deadCanUse, useText, text, textOffset)
        {
            ImpostorButton = true;
        }

        public ImpostorButton(Action OnClick, float Cooldown, Sprite Image, Vector2 PositionOffset, bool deadCanUse,
            float EffectDuration, Action OnEffectEnd,
            bool useText = false, string text = "", Vector2 textOffset = new Vector2()) : base(OnClick, Cooldown, Image,
            PositionOffset, deadCanUse, EffectDuration, OnEffectEnd, useText, text, textOffset)
        {
            ImpostorButton = true;
        }
    }
}