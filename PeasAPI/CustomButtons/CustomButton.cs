using System.Collections.Generic;
using System.Reflection;
using PeasAPI.Roles;
using UnityEngine;
using Action = System.Action;

namespace PeasAPI.CustomButtons
{
    public class CustomButton
    {
        public static List<CustomButton> Buttons = new List<CustomButton>();
        public KillButtonManager KillButtonManager;
        private Color _startColorText = new Color(255, 255, 255);
        public Vector2 PositionOffset;
        public float MaxTimer;
        public float Timer;
        public float EffectDuration;
        public bool IsEffectActive;
        public bool hasEffectDuration;
        public bool enabled = true;
        public bool Visibile = true;
        private Sprite _buttonSprite;
        public Action OnClick;
        private Action OnEffectEnd;
        private bool _canUse;
        public BaseRole Role;
        public bool UseRole = false;
        public bool ImpostorButton = false;
        public bool DeadCanUse;
        public string Text = "";
        public Vector2 TextOffset;
        public bool UseText = false;
        public PassiveButton Button;

        public CustomButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset, bool deadCanUse,
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
            hasEffectDuration = true;

            UseText = useText;
            Text = text;
            TextOffset = textOffset;

            Buttons.Add(this);

            Start();
        }

        public CustomButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset, bool deadCanUse,
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
            KillButtonManager = Object.Instantiate(HudManager.Instance.KillButton, HudManager.Instance.transform);
            KillButtonManager.gameObject.SetActive(true);
            
            _startColorText = KillButtonManager.TimerText.color;
            
            KillButtonManager.renderer.enabled = true;
            KillButtonManager.renderer.sprite = _buttonSprite;
            
            KillButtonManager.killText.enabled = UseText;
            KillButtonManager.killText.text = Text;
            KillButtonManager.killText.transform.position += (Vector3) TextOffset;
            
            Button = KillButtonManager.GetComponent<PassiveButton>();
            Button.OnClick.RemoveAllListeners();
            Button.OnClick.AddListener((UnityEngine.Events.UnityAction) listener);

            void listener()
            {
                if (Timer <= 0f && _canUse && enabled && KillButtonManager.gameObject.active &&
                    PlayerControl.LocalPlayer.moveable)
                {
                    KillButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.3f);
                    OnClick();
                    Timer = MaxTimer;
                    if (hasEffectDuration)
                    {
                        IsEffectActive = true;
                        Timer = EffectDuration;
                        KillButtonManager.TimerText.color = new Color(0, 255, 0);
                    }
                }
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
            
            if (UseRole)
            {
                _canUse = PlayerControl.LocalPlayer.IsRole(Role) &&
                          (DeadCanUse || !PlayerControl.LocalPlayer.Data.IsDead);
            }
            else if (ImpostorButton)
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

        public static void HudUpdate()
        {
            Buttons.RemoveAll(item => item.KillButtonManager == null);
            for (int i = 0; i < Buttons.Count; i++)
            {
                var button = Buttons[i];
                var killButton = button.KillButtonManager;
                var canUse = button.CanUse();
                
                Buttons[i].KillButtonManager.renderer.sprite = button._buttonSprite;
                
                killButton.gameObject.SetActive(button.Visibile && canUse);
                
                killButton.killText.enabled = canUse;
                killButton.killText.alpha = killButton.isCoolingDown ? Palette.DisabledClear.a : Palette.EnabledColor.a;

                if (canUse && button.Visibile)
                    button.Update();
            }
        }

        private void Update()
        {
            var pos = KillButtonManager.transform.localPosition;
            
            if (pos.x > 0f)
                KillButtonManager.transform.localPosition = new Vector3((pos.x + 1.3f) * -1, pos.y, pos.z) + new Vector3(PositionOffset.x, PositionOffset.y);
            
            if (Timer < 0f && PlayerControl.LocalPlayer.moveable)
            {
                KillButtonManager.renderer.color = new Color(1f, 1f, 1f, 1f);
                
                if (IsEffectActive)
                {
                    KillButtonManager.TimerText.color = _startColorText;
                    Timer = MaxTimer;
                    
                    IsEffectActive = false;
                    OnEffectEnd();
                }
            }
            else
            {
                if (_canUse)
                    Timer -= Time.deltaTime;
                
                KillButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.3f);
            }

            KillButtonManager.killText.enabled = UseText;
            KillButtonManager.killText.text = Text;
            
            KillButtonManager.gameObject.SetActive(_canUse);
            KillButtonManager.renderer.enabled = _canUse;
            
            if (_canUse)
            {
                KillButtonManager.renderer.material.SetFloat("_Desat", 0f);
                KillButtonManager.SetCoolDown(Timer, MaxTimer);
            }
        }

        public void SetTexture(string image, Assembly assembly)
        {
            _buttonSprite = Utility.CreateSprite(image);
        }
    }
}