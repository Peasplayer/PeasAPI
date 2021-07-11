using System.Collections.Generic;
using System.Reflection;
using PeasAPI.Roles;
using RewiredConsts;
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
        public float MaxTimer = 0f;
        public float Timer = 0f;
        public float EffectDuration = 0f;
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
        public bool DeadCanUse = false;
        public PassiveButton Button;

        public CustomButton(Action OnClick, float Cooldown, Sprite Image, Vector2 PositionOffset, bool deadCanUse, float EffectDuration, Action OnEffectEnd)
        {
            this.OnClick = OnClick;
            this.OnEffectEnd = OnEffectEnd;
            this.PositionOffset = PositionOffset;
            this.EffectDuration = EffectDuration;
            DeadCanUse = deadCanUse;
            MaxTimer = Cooldown;
            Timer = MaxTimer;
            _buttonSprite = Image;
            hasEffectDuration = true;
            IsEffectActive = false;
            Buttons.Add(this);
            Start();
        }
        
        public CustomButton(Action OnClick, float Cooldown, Sprite Image, Vector2 PositionOffset, bool deadCanUse)
        {
            this.OnClick = OnClick;
            this.PositionOffset = PositionOffset;
            DeadCanUse = deadCanUse;
            MaxTimer = Cooldown;
            Timer = MaxTimer;
            _buttonSprite = Image;
            hasEffectDuration = false;
            Buttons.Add(this);
            Start();
        }

        private void Start()
        {
            KillButtonManager = UnityEngine.Object.Instantiate(HudManager.Instance.KillButton, HudManager.Instance.transform);
            _startColorText = KillButtonManager.TimerText.color;
            KillButtonManager.gameObject.SetActive(true);
            KillButtonManager.renderer.enabled = true;
            KillButtonManager.renderer.sprite = _buttonSprite;
            KillButtonManager.killText.enabled = false;
            Button = KillButtonManager.GetComponent<PassiveButton>();
            Button.OnClick.RemoveAllListeners();
            Button.OnClick.AddListener((UnityEngine.Events.UnityAction)listener);
            void listener()
            {
                if (Timer <= 0f && _canUse && enabled && KillButtonManager.gameObject.active && PlayerControl.LocalPlayer.moveable)
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
            if (PlayerControl.LocalPlayer == null) return false;
            if (PlayerControl.LocalPlayer.Data == null) return false;
            if (MeetingHud.Instance != null) return false;
            if (UseRole)
            {
                _canUse = PlayerControl.LocalPlayer.IsRole(Role) && (DeadCanUse || !PlayerControl.LocalPlayer.Data.IsDead);
            }
            else if (ImpostorButton)
            {
                _canUse = PlayerControl.LocalPlayer.Data.IsImpostor && (DeadCanUse || !PlayerControl.LocalPlayer.Data.IsDead);
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
                
                Buttons[i].KillButtonManager.renderer.sprite = Buttons[i]._buttonSprite;
                Buttons[i].KillButtonManager.gameObject.SetActive(Buttons[i].Visibile);
                Buttons[i].KillButtonManager.renderer.enabled = Buttons[i].Visibile;
                Buttons[i].KillButtonManager.enabled = Buttons[i].Visibile;
                Buttons[i].KillButtonManager.gameObject.active = Buttons[i].Visibile;

                Buttons[i].KillButtonManager.renderer.enabled = Buttons[i].CanUse();
                Buttons[i].KillButtonManager.enabled = Buttons[i].CanUse();
                Buttons[i].KillButtonManager.gameObject.active = Buttons[i].CanUse();
                
                if (Buttons[i].CanUse() && Buttons[i].Visibile)
                    Buttons[i].Update();
            }
        }
        private void Update()
        {
            if (KillButtonManager.transform.localPosition.x > 0f)
                KillButtonManager.transform.localPosition = new Vector3((KillButtonManager.transform.localPosition.x + 1.3f) * -1, KillButtonManager.transform.localPosition.y, KillButtonManager.transform.localPosition.z) + new Vector3(PositionOffset.x, PositionOffset.y);
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
            _buttonSprite = Utility.CreateSprite(image, assembly);
        }
    }
}