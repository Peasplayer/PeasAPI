using System;
using System.Collections.Generic;
using System.Reflection;
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
        
        private Color _startColorText = new Color(255, 255, 255);
        private Sprite _buttonSprite;
        private bool _canUse;
        private bool _isEffectActive;
        private BaseRole _role;
        private bool _useRole = false;
        private bool _impostorButton = false;
        
        public KillButtonManager KillButtonManager;
        public Vector2 PositionOffset;
        public Vector2 TextOffset;
        public float MaxCooldown;
        public float Cooldown;
        public float EffectDuration;
        public bool HasEffect;
        public bool Enabled = true;
        public bool Visible = true;
        public string Text;
        public bool UseText => string.IsNullOrEmpty(Text);
        
        public readonly Action OnClick;
        public readonly Action OnEffectEnd;
        public readonly bool DeadCanUse;

        public static CustomButton AddImpostorButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset, bool deadCanUse,
            float effectDuration, Action onEffectEnd, string text = "",
            Vector2 textOffset = new Vector2())
        {
            var button = new CustomButton(onClick, cooldown, image, positionOffset, deadCanUse, effectDuration,
                onEffectEnd, text, textOffset) {_impostorButton = true};
            return button;
        }
        
        public static CustomButton AddImpostorButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset, bool deadCanUse, bool useText = false, string text = "",
            Vector2 textOffset = new Vector2())
        {
            var button = new CustomButton(onClick, cooldown, image, positionOffset, deadCanUse, 
                text, textOffset) {_impostorButton = true};
            return button;
        }
        
        public static CustomButton AddRoleButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset, bool deadCanUse, BaseRole role,
            float effectDuration, Action onEffectEnd, bool useText = false, string text = "",
            Vector2 textOffset = new Vector2())
        {
            var button = new CustomButton(onClick, cooldown, image, positionOffset, deadCanUse, effectDuration,
                onEffectEnd, text, textOffset) {_useRole = true, _role = role};
            return button;
        }
        
        public static CustomButton AddRoleButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset, bool deadCanUse, BaseRole role, bool useText = false, string text = "",
            Vector2 textOffset = new Vector2())
        {
            var button = new CustomButton(onClick, cooldown, image, positionOffset, deadCanUse, 
                text, textOffset) {_useRole = true, _role = role};
            return button;
        }
        
        private CustomButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset, bool deadCanUse,
            float effectDuration, Action onEffectEnd, string text = "",
            Vector2 textOffset = new Vector2())
        {
            OnClick = onClick;

            PositionOffset = positionOffset;

            DeadCanUse = deadCanUse;

            MaxCooldown = cooldown;
            Cooldown = MaxCooldown;

            _buttonSprite = image;

            OnEffectEnd = onEffectEnd;
            EffectDuration = effectDuration;
            HasEffect = true;

            Text = text;
            TextOffset = textOffset;

            Buttons.Add(this);

            Start();
        }

        private CustomButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset, bool deadCanUse,
            string text = "", Vector2 textOffset = new Vector2())
        {
            OnClick = onClick;

            PositionOffset = positionOffset;

            DeadCanUse = deadCanUse;

            MaxCooldown = cooldown;
            Cooldown = MaxCooldown;

            _buttonSprite = image;

            Text = text;
            TextOffset = textOffset;

            Buttons.Add(this);

            Start();
        }

        private void Start()
        {
            KillButtonManager = Object.Instantiate(HudManager.Instance.KillButton, HudManager.Instance.transform);
            KillButtonManager.gameObject.SetActive(true);
            
            _startColorText = KillButtonManager.TimerText.color;
            
            KillButtonManager.renderer.enabled = true;
            KillButtonManager.renderer.sprite = _buttonSprite;
            
            KillButtonManager.killText.enabled = UseText;
            KillButtonManager.killText.text = Text;
            KillButtonManager.killText.transform.position += (Vector3) TextOffset;
            
            var button = KillButtonManager.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener((UnityEngine.Events.UnityAction) listener);

            void listener()
            {
                if (Cooldown <= 0f && _canUse && Enabled && KillButtonManager.gameObject.active &&
                    PlayerControl.LocalPlayer.moveable)
                {
                    KillButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.3f);
                    OnClick();
                    Cooldown = MaxCooldown;
                    if (HasEffect)
                    {
                        _isEffectActive = true;
                        Cooldown = EffectDuration;
                        KillButtonManager.TimerText.color = new Color(0, 255, 0);
                    }
                }
            }
        }
        
        private void Update()
        {
            var pos = KillButtonManager.transform.localPosition;
            
            if (pos.x > 0f)
                KillButtonManager.transform.localPosition = new Vector3((pos.x + 1.3f) * -1, pos.y, pos.z) + new Vector3(PositionOffset.x, PositionOffset.y);
            
            if (Cooldown < 0f && PlayerControl.LocalPlayer.moveable)
            {
                KillButtonManager.renderer.color = new Color(1f, 1f, 1f, 1f);
                
                if (_isEffectActive)
                {
                    KillButtonManager.TimerText.color = _startColorText;
                    Cooldown = MaxCooldown;
                    
                    _isEffectActive = false;
                    OnEffectEnd();
                }
            }
            else
            {
                if (_canUse)
                    Cooldown -= Time.deltaTime;
                
                KillButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.3f);
            }

            KillButtonManager.killText.enabled = UseText;
            KillButtonManager.killText.text = Text;
            
            KillButtonManager.gameObject.SetActive(_canUse);
            KillButtonManager.renderer.enabled = _canUse;
            
            if (_canUse)
            {
                KillButtonManager.renderer.material.SetFloat("_Desat", 0f);
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
            
            if (_useRole)
            {
                _canUse = PlayerControl.LocalPlayer.IsRole(_role) &&
                          (DeadCanUse || !PlayerControl.LocalPlayer.Data.IsDead);
            }
            else if (_impostorButton)
            {
                _canUse = PlayerControl.LocalPlayer.Data.IsImpostor &&
                          (DeadCanUse || !PlayerControl.LocalPlayer.Data.IsDead);
            }
            else
            {
                _canUse = DeadCanUse || !PlayerControl.LocalPlayer.Data.IsDead;
            }

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
        
        public bool IsEffectActive()
        {
            return _isEffectActive;
        }
        
        public bool IsCoolingDown()
        {
            return KillButtonManager.isCoolingDown;
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
                
                    Buttons[i].KillButtonManager.renderer.sprite = button._buttonSprite;
                
                    killButton.gameObject.SetActive(button.Visible && canUse);
                
                    killButton.killText.enabled = canUse;
                    killButton.killText.alpha = killButton.isCoolingDown ? Palette.DisabledClear.a : Palette.EnabledColor.a;

                    if (canUse && button.Visible)
                        button.Update();
                }
            }
        }
    }
}