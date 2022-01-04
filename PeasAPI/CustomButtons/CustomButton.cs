using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PeasAPI.Roles;
using UnityEngine;
using Action = System.Action;
using Object = UnityEngine.Object;

namespace PeasAPI.CustomButtons
{
    public class CustomButton
    {
        public static List<CustomButton> Buttons = new List<CustomButton>();
        public static List<CustomButton> VisibleButtons => Buttons.Where(button => button.Visible && button._canUse).ToList();
        public static bool HudActive = true;
        
        private Color _startColorText = new Color(255, 255, 255);
        private Sprite _buttonSprite;
        private bool _canUse;
        private BaseRole _role;
        private bool _useRole = false;
        private bool _impostorButton = false;
        
        public KillButton KillButtonManager;
        public Vector2 PositionOffset;
        public Vector2 TextOffset;
        public float MaxCooldown;
        public float Cooldown;
        public float EffectDuration;
        public bool HasEffect;
        public bool IsEffectActive;
        public bool Enabled = true;
        public bool Usable = true;
        public bool Visible = true;
        public string Text;
        public bool UseText => !string.IsNullOrEmpty(Text);
        
        public readonly Action OnClick;
        public readonly Action OnEffectEnd;
        public readonly ButtonUsability Usability;

        public static CustomButton AddImpostorButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset = new Vector2(), ButtonUsability usability = ButtonUsability.Alive,
            float effectDuration = 0, Action onEffectEnd = null, string text = "",
            Vector2 textOffset = new Vector2())
        {
            var button = new CustomButton(onClick, cooldown, image, positionOffset, usability, effectDuration,
                onEffectEnd, text, textOffset) {_impostorButton = true};
            return button;
        }
        
        public static CustomButton AddRoleButton(Action onClick, float cooldown, Sprite image, BaseRole role, Vector2 positionOffset = new Vector2(), ButtonUsability usability = ButtonUsability.Alive,
            float effectDuration = 0, Action onEffectEnd = null, string text = "",
            Vector2 textOffset = new Vector2())
        {
            var button = new CustomButton(onClick, cooldown, image, positionOffset, usability, effectDuration,
                onEffectEnd, text, textOffset) {_useRole = true, _role = role};
            return button;
        }
        
        private CustomButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset, ButtonUsability usability,
            float effectDuration, Action onEffectEnd, string text = "",
            Vector2 textOffset = new Vector2())
        {
            OnClick = onClick;

            PositionOffset = positionOffset;

            Usability = usability;

            MaxCooldown = cooldown;
            Cooldown = MaxCooldown;

            _buttonSprite = image;

            OnEffectEnd = onEffectEnd;
            EffectDuration = effectDuration;
            HasEffect = effectDuration != 0 && onEffectEnd != null;

            Text = text;
            TextOffset = textOffset;

            Buttons.Add(this);

            Start();
        }

        private void Start()
        {
            if (HudManager.Instance.transform.FindChild("Buttons").FindChild("Custom") == null)
            {
                var custom = new GameObject("Custom");
                custom.transform.SetParent(HudManager.Instance.transform.FindChild("Buttons"));
                custom.transform.localPosition = HudManager.Instance.transform.localPosition;
                custom.transform.position = HudManager.Instance.transform.position;
            }
            
            KillButtonManager = Object.Instantiate(HudManager.Instance.KillButton, HudManager.Instance.transform.FindChild("Buttons").FindChild("Custom"));
            KillButtonManager.gameObject.SetActive(true);
            KillButtonManager.gameObject.name = "CustomButton";
            KillButtonManager.transform.localScale = new Vector3(1, 1, 1);
            
            _startColorText = KillButtonManager.cooldownTimerText.color;
            
            KillButtonManager.graphic.enabled = true;
            KillButtonManager.graphic.sprite = _buttonSprite;
            
            KillButtonManager.buttonLabelText.enabled = UseText;
            KillButtonManager.buttonLabelText.text = Text;
            KillButtonManager.buttonLabelText.transform.position += (Vector3) TextOffset + new Vector3(0f, 0.1f);
            
            var button = KillButtonManager.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener((UnityEngine.Events.UnityAction) listener);

            void listener()
            {
                if (IsUsable() && _canUse && Enabled && KillButtonManager.gameObject.active &&
                    PlayerControl.LocalPlayer.moveable)
                {
                    KillButtonManager.buttonLabelText.material.color = KillButtonManager.graphic.color = new Color(1f, 1f, 1f, 0.3f);
                    OnClick();
                    Cooldown = MaxCooldown;
                    if (HasEffect)
                    {
                        IsEffectActive = true;
                        Cooldown = EffectDuration;
                        KillButtonManager.cooldownTimerText.color = new Color(0, 255, 0);
                    }
                }
            }
        }
        
        private void Update()
        {
            var pos = KillButtonManager.transform.localPosition;
            var i = VisibleButtons.IndexOf(this);

            if (pos.x > 0f)
            {
                var offset = PositionOffset == Vector2.zero || PositionOffset == default
                    ? new Vector3(i / 3 * 1.3f, 1.2f * (i - i / 3 * 3))
                    : new Vector3(PositionOffset.x, PositionOffset.y);
                KillButtonManager.transform.localPosition = new Vector3(-(pos.x + 1.3f) + 1.3f, pos.y - 1, pos.z) + offset;   
            }

            if (Cooldown < 0f && Enabled && PlayerControl.LocalPlayer.moveable)
            {
                KillButtonManager.buttonLabelText.color = KillButtonManager.graphic.color = IsUsable() ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.3f);
                
                if (IsEffectActive)
                {
                    KillButtonManager.cooldownTimerText.color = _startColorText;
                    Cooldown = MaxCooldown;
                    
                    IsEffectActive = false;
                    OnEffectEnd();
                }
            }
            else
            {
                if (_canUse && Enabled)
                    Cooldown -= Time.deltaTime;
                
                KillButtonManager.buttonLabelText.color = KillButtonManager.graphic.color = new Color(1f, 1f, 1f, 0.3f);
            }

            KillButtonManager.buttonLabelText.enabled = UseText;
            KillButtonManager.buttonLabelText.text = Text;
            
            KillButtonManager.gameObject.SetActive(_canUse);
            KillButtonManager.graphic.enabled = _canUse;
            
            if (_canUse)
            {
                KillButtonManager.graphic.material.SetFloat("_Desat", 0f);
                KillButtonManager.buttonLabelText.material.SetFloat("_Desat", 0f);
                KillButtonManager.SetCoolDown(Cooldown, MaxCooldown);
            }
        }

        public bool CanUse()
        {
            if (PlayerControl.LocalPlayer == null) 
                return false;
            
            if (PlayerControl.LocalPlayer.Data == null) 
                return false;
            
            if (MeetingHud.Instance != null) 
                return false;
            
            var flag = Usability switch
            {
                ButtonUsability.Alive => !PlayerControl.LocalPlayer.Data.IsDead,
                ButtonUsability.Dead => PlayerControl.LocalPlayer.Data.IsDead,
                ButtonUsability.AliveAndDead => true,
                _ => true
            };

            _canUse = _useRole ? PlayerControl.LocalPlayer.IsRole(_role) && flag :
                _impostorButton ? PlayerControl.LocalPlayer.Data.Role.IsImpostor && flag : flag;

            return true;
        }
        
        public void SetImage(Sprite image)
        {
            _buttonSprite = image;
        }

        public void SetCoolDown(float cooldown, float? maxCooldown = null)
        {
            Cooldown = cooldown;
            if (maxCooldown != null)
                MaxCooldown = maxCooldown.Value;
            KillButtonManager.SetCoolDown(Cooldown, MaxCooldown);
        }

        public bool IsImpostorButton()
        {
            return _impostorButton;
        }
        
        public bool IsRoleButton()
        {
            return _useRole;
        }
        
        public bool IsCoolingDown()
        {
            return KillButtonManager.isCoolingDown;
        }

        public bool IsUsable()
        {
            return Usable && Cooldown < 0f && HudActive;
        }
        
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class HudManagerUpdatePatch
        {
            public static void Prefix(HudManager __instance)
            {
                Buttons.RemoveAll(item => item.KillButtonManager == null);
                for (int i = 0; i < Buttons.Count; i++)
                {
                    var button = Buttons[i];
                    var killButton = button.KillButtonManager;
                    var canUse = button.CanUse();
                
                    Buttons[i].KillButtonManager.graphic.sprite = button._buttonSprite;
                
                    killButton.gameObject.SetActive(button.Visible && canUse);
                
                    killButton.buttonLabelText.enabled = canUse;
                    killButton.buttonLabelText.alpha = killButton.isCoolingDown ? Palette.DisabledClear.a : Palette.EnabledColor.a;

                    if (canUse && button.Visible)
                        button.Update();
                }
            }
        }
        
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
        public static class HudManagerSetHudActivePatch
        {
            public static void Prefix(HudManager __instance, [HarmonyArgument(0)] bool isActive)
            {
                HudActive = isActive;
            }
        }
    }

    public enum ButtonUsability
    {
        Alive,
        Dead,
        AliveAndDead
    }
}