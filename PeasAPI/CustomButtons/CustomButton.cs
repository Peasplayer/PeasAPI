using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PeasAPI.Roles;
using UnityEngine;
using Action = System.Action;

namespace PeasAPI.CustomButtons
{
    public class CustomButton
    {
        public static List<CustomButton> Buttons = new List<CustomButton>();
        
        private KillButtonManager _killButtonManager;
        private Color _startColorText = new Color(255, 255, 255);
        private Sprite _buttonSprite;
        private bool _canUse;
        private BaseRole _role;
        private bool _useRole = false;
        private bool _impostorButton = false;
        
        public Vector2 PositionOffset;
        public Vector2 TextOffset;
        public float MaxTimer;
        public float Timer;
        public float EffectDuration;
        public bool IsEffectActive;
        public bool HasEffectDuration;
        public bool Enabled = true;
        public bool Visibile = true;
        public string Text;
        public bool UseText;
        
        public readonly Action OnClick;
        public readonly Action OnEffectEnd;
        public readonly bool DeadCanUse;

        public static CustomButton AddImpostorButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset, bool deadCanUse,
            float effectDuration, Action onEffectEnd, bool useText = false, string text = "",
            Vector2 textOffset = new Vector2())
        {
            var button = new CustomButton(onClick, cooldown, image, positionOffset, deadCanUse, effectDuration,
                onEffectEnd, useText, text, textOffset) {_impostorButton = true};
            return button;
        }
        
        public static CustomButton AddImpostorButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset, bool deadCanUse, bool useText = false, string text = "",
            Vector2 textOffset = new Vector2())
        {
            var button = new CustomButton(onClick, cooldown, image, positionOffset, deadCanUse, 
                useText, text, textOffset) {_impostorButton = true};
            return button;
        }
        
        public static CustomButton AddRoleButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset, bool deadCanUse, BaseRole role,
            float effectDuration, Action onEffectEnd, bool useText = false, string text = "",
            Vector2 textOffset = new Vector2())
        {
            var button = new CustomButton(onClick, cooldown, image, positionOffset, deadCanUse, effectDuration,
                onEffectEnd, useText, text, textOffset) {_useRole = true, _role = role};
            return button;
        }
        
        public static CustomButton AddRoleButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset, bool deadCanUse, BaseRole role, bool useText = false, string text = "",
            Vector2 textOffset = new Vector2())
        {
            var button = new CustomButton(onClick, cooldown, image, positionOffset, deadCanUse, 
                useText, text, textOffset) {_useRole = true, _role = role};
            return button;
        }
        
        private CustomButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset, bool deadCanUse,
            float effectDuration, Action onEffectEnd, bool useText = false, string text = "",
            Vector2 textOffset = new Vector2())
        {
            OnClick = onClick;

            PositionOffset = positionOffset;

            DeadCanUse = deadCanUse;

            MaxTimer = cooldown;
            Timer = MaxTimer;

            _buttonSprite = image;

            OnEffectEnd = onEffectEnd;
            EffectDuration = effectDuration;
            HasEffectDuration = true;

            UseText = useText;
            Text = text;
            TextOffset = textOffset;

            Buttons.Add(this);

            Start();
        }

        private CustomButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset, bool deadCanUse,
            bool useText = false, string text = "", Vector2 textOffset = new Vector2())
        {
            OnClick = onClick;

            PositionOffset = positionOffset;

            DeadCanUse = deadCanUse;

            MaxTimer = cooldown;
            Timer = MaxTimer;

            _buttonSprite = image;

            UseText = useText;
            Text = text;
            TextOffset = textOffset;

            Buttons.Add(this);

            Start();
        }

        private void Start()
        {
            _killButtonManager = Object.Instantiate(HudManager.Instance.KillButton, HudManager.Instance.transform);
            _killButtonManager.gameObject.SetActive(true);
            
            _startColorText = _killButtonManager.TimerText.color;
            
            _killButtonManager.renderer.enabled = true;
            _killButtonManager.renderer.sprite = _buttonSprite;
            
            _killButtonManager.killText.enabled = UseText;
            _killButtonManager.killText.text = Text;
            _killButtonManager.killText.transform.position += (Vector3) TextOffset;
            
            var button = _killButtonManager.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener((UnityEngine.Events.UnityAction) listener);

            void listener()
            {
                if (Timer <= 0f && _canUse && Enabled && _killButtonManager.gameObject.active &&
                    PlayerControl.LocalPlayer.moveable)
                {
                    _killButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.3f);
                    OnClick();
                    Timer = MaxTimer;
                    if (HasEffectDuration)
                    {
                        IsEffectActive = true;
                        Timer = EffectDuration;
                        _killButtonManager.TimerText.color = new Color(0, 255, 0);
                    }
                }
            }
        }
        
        private void Update()
        {
            var pos = _killButtonManager.transform.localPosition;
            
            if (pos.x > 0f)
                _killButtonManager.transform.localPosition = new Vector3((pos.x + 1.3f) * -1, pos.y, pos.z) + new Vector3(PositionOffset.x, PositionOffset.y);
            
            if (Timer < 0f && PlayerControl.LocalPlayer.moveable)
            {
                _killButtonManager.renderer.color = new Color(1f, 1f, 1f, 1f);
                
                if (IsEffectActive)
                {
                    _killButtonManager.TimerText.color = _startColorText;
                    Timer = MaxTimer;
                    
                    IsEffectActive = false;
                    OnEffectEnd();
                }
            }
            else
            {
                if (_canUse)
                    Timer -= Time.deltaTime;
                
                _killButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.3f);
            }

            _killButtonManager.killText.enabled = UseText;
            _killButtonManager.killText.text = Text;
            
            _killButtonManager.gameObject.SetActive(_canUse);
            _killButtonManager.renderer.enabled = _canUse;
            
            if (_canUse)
            {
                _killButtonManager.renderer.material.SetFloat("_Desat", 0f);
                _killButtonManager.SetCoolDown(Timer, MaxTimer);
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
        
        public void SetTexture(string image, Assembly assembly)
        {
            _buttonSprite = Utility.CreateSprite(image);
        }

        public void SetCoolDown(float cooldown, float? maxCooldown = null)
        {
            Timer = cooldown;
            if (maxCooldown != null)
                MaxTimer = maxCooldown.Value;
            _killButtonManager.SetCoolDown(Timer, MaxTimer);
        }

        public bool IsImpostorButton()
        {
            return _impostorButton;
        }
        
        public bool IsRoleButton()
        {
            return _useRole;
        }
        
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class HudManagerUpdatePatch
        {
            public static void Prefix(HudManager __instance)
            {
                Buttons.RemoveAll(item => item._killButtonManager == null);
                for (int i = 0; i < Buttons.Count; i++)
                {
                    var button = Buttons[i];
                    var killButton = button._killButtonManager;
                    var canUse = button.CanUse();
                
                    Buttons[i]._killButtonManager.renderer.sprite = button._buttonSprite;
                
                    killButton.gameObject.SetActive(button.Visibile && canUse);
                
                    killButton.killText.enabled = canUse;
                    killButton.killText.alpha = killButton.isCoolingDown ? Palette.DisabledClear.a : Palette.EnabledColor.a;

                    if (canUse && button.Visibile)
                        button.Update();
                }
            }
        }
    }
}