using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Action = System.Action;
using Object = UnityEngine.Object;

namespace PeasAPI.CustomButtons
{
    public class CustomButton
    {
        public static List<CustomButton> Buttons = new List<CustomButton>();
        public static List<CustomButton> VisibleButtons => Buttons.Where(button => button.Visible && button.CouldBeUsed()).ToList();
        public static bool HudActive = true;
        
        private Color _startColorText = new Color(255, 255, 255);
        private Sprite _buttonSprite;
        
        public KillButton KillButtonManager;
        public Vector2 PositionOffset;
        public Vector2 TextOffset;
        public float MaxCooldown;
        public float Cooldown;
        public float EffectDuration;
        public bool IsEffectActive;
        public bool Enabled = true;
        public bool Usable = true;
        public bool Visible = true;
        public Predicate<PlayerControl> _CouldBeUsed;
        public Predicate<PlayerControl> _CanBeUsed;
        public string Text;
        public Action OnClick;
        public Action OnEffectEnd;
        public bool UseText => !string.IsNullOrEmpty(Text);
        public bool HasEffect => EffectDuration != 0 && OnEffectEnd != null;
        public TargetType Target;
        public Color? TargetColor;
        public Predicate<PlayerControl> ChoosePlayerTarget;
        public PlayerControl PlayerTarget;
        public Predicate<GameObject> ChooseObjectTarget;
        public Func<GameObject> ChooseObjectTargetCustom;
        public GameObject ObjectTarget;

        public static CustomButton AddButton(Action onClick, float cooldown, Sprite image,
            Predicate<PlayerControl> couldBeUsed, Predicate<PlayerControl> canBeUsed,
            Vector2 positionOffset = new Vector2(),
            float effectDuration = 0, Action onEffectEnd = null, string text = "",
            Vector2 textOffset = new Vector2(), TargetType target = TargetType.None, Color? targetColor = null,
            Predicate<PlayerControl> choosePlayerTarget = null, Predicate<GameObject> chooseObjectTarget = null, Func<GameObject> chooseObjectTargetCustom = null)
        {
            var button = new CustomButton(onClick, cooldown, image, positionOffset, couldBeUsed, canBeUsed,
                effectDuration,
                onEffectEnd, text, textOffset, target, targetColor, choosePlayerTarget, chooseObjectTarget, chooseObjectTargetCustom);
            return button;
        }

        private CustomButton(Action onClick, float cooldown, Sprite image, Vector2 positionOffset,
            Predicate<PlayerControl> couldBeUsed, Predicate<PlayerControl> canBeUsed,
            float effectDuration, Action onEffectEnd, string text = "",
            Vector2 textOffset = new Vector2(), TargetType target = TargetType.None, Color? targetColor = null,
            Predicate<PlayerControl> choosePlayerTarget = null, Predicate<GameObject> chooseObjectTarget = null, Func<GameObject> chooseObjectTargetCustom = null)
        {
            OnClick = onClick;

            PositionOffset = positionOffset;

            _CanBeUsed = canBeUsed;
            _CouldBeUsed = couldBeUsed;

            MaxCooldown = cooldown;
            Cooldown = MaxCooldown;

            _buttonSprite = image;

            OnEffectEnd = onEffectEnd;
            EffectDuration = effectDuration;

            Text = text;
            TextOffset = textOffset;

            Target = target;
            TargetColor = targetColor;
            ChoosePlayerTarget = choosePlayerTarget;
            ChooseObjectTarget = chooseObjectTarget;
            ChooseObjectTargetCustom = chooseObjectTargetCustom;

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

            KillButtonManager = Object.Instantiate(HudManager.Instance.KillButton,
                HudManager.Instance.transform.FindChild("Buttons").FindChild("Custom"));
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
                if (CanBeUsed() && CouldBeUsed() && Enabled && KillButtonManager.gameObject.active &&
                    PlayerControl.LocalPlayer.moveable)
                {
                    KillButtonManager.buttonLabelText.material.color =
                        KillButtonManager.graphic.color = new Color(1f, 1f, 1f, 0.3f);
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
            if (Target == TargetType.Player)
            {
                var target = FindClosestPlayer();

                if (PlayerTarget != target && PlayerTarget != null)
                {
                    PlayerTarget.MyRend.material.SetFloat("_Outline", 0f);
                }

                if (target != null)
                {
                    target.MyRend.material.SetFloat("_Outline", 1f);
                    target.MyRend.material.SetColor("_OutlineColor", TargetColor ?? Palette.CrewmateBlue);
                }

                PlayerTarget = target;
            }
            else if (Target == TargetType.Object)
            {
                var target = FindClosestObject();

                if (ObjectTarget != target && ObjectTarget != null)
                {
                    var image = ObjectTarget.GetComponent<SpriteRenderer>();
                    if (!image && ObjectTarget.transform.FindChild("Sprite") != null)
                        image = ObjectTarget.transform.FindChild("Sprite").GetComponent<SpriteRenderer>();
                    if (!image)
                        image = ObjectTarget.GetComponentInChildren<SpriteRenderer>();
                    
                    if (image)
                    {
                        image.material.SetFloat("_Outline", 0);
                    }
                }

                if (target != null)
                {
                    var image = target.GetComponent<SpriteRenderer>();
                    if (!image && target.transform.FindChild("Sprite") != null)
                        image = target.transform.FindChild("Sprite").GetComponent<SpriteRenderer>();
                    if (!image)
                        image = target.GetComponentInChildren<SpriteRenderer>();
                    
                    if (image)
                    {
                        image.material.SetFloat("_Outline", 1);
                        image.material.SetColor("_OutlineColor", TargetColor ?? Palette.CrewmateBlue);
                    }
                }

                ObjectTarget = target;
            }
            else if (Target == TargetType.CustomObject)
            {
                var target = ChooseObjectTargetCustom.Invoke();

                if (ObjectTarget != target && ObjectTarget != null)
                {
                    var image = ObjectTarget.GetComponent<SpriteRenderer>();
                    if (!image && ObjectTarget.transform.FindChild("Sprite") != null)
                        image = ObjectTarget.transform.FindChild("Sprite").GetComponent<SpriteRenderer>();
                    if (!image)
                        image = ObjectTarget.GetComponentInChildren<SpriteRenderer>();
                    
                    if (image)
                    {
                        image.material.SetFloat("_Outline", 0);
                    }
                }

                if (target != null)
                {
                    var image = target.GetComponent<SpriteRenderer>();
                    if (!image && target.transform.FindChild("Sprite") != null)
                        image = target.transform.FindChild("Sprite").GetComponent<SpriteRenderer>();
                    if (!image)
                        image = target.GetComponentInChildren<SpriteRenderer>();
                    
                    if (image)
                    {
                        image.material.SetFloat("_Outline", 1);
                        image.material.SetColor("_OutlineColor", TargetColor ?? Palette.CrewmateBlue);
                    }
                }

                ObjectTarget = target;
            }

            var pos = KillButtonManager.transform.localPosition;
            var i = VisibleButtons.IndexOf(this);

            if (pos.x > 0f)
            {
                var offset = PositionOffset == Vector2.zero || PositionOffset == default
                    ? new Vector3(i / 3 * 1.3f, 1.2f * (i - i / 3 * 3))
                    : new Vector3(PositionOffset.x, PositionOffset.y);
                KillButtonManager.transform.localPosition =
                    new Vector3(-(pos.x + 1.3f) + 1.3f, pos.y - 1, pos.z) + offset;
            }

            if (Cooldown < 0f && Enabled && PlayerControl.LocalPlayer.moveable)
            {
                KillButtonManager.buttonLabelText.color = KillButtonManager.graphic.color =
                    CanBeUsed() ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.3f);

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
                if (CouldBeUsed() && Enabled)
                    Cooldown -= Time.deltaTime;
                
                KillButtonManager.buttonLabelText.color = KillButtonManager.graphic.color = new Color(1f, 1f, 1f, 0.3f);
            }

            KillButtonManager.buttonLabelText.enabled = UseText;
            KillButtonManager.buttonLabelText.text = Text;
            
            KillButtonManager.gameObject.SetActive(CouldBeUsed());
            KillButtonManager.graphic.enabled = CouldBeUsed();
            
            if (CouldBeUsed())
            {
                KillButtonManager.graphic.material.SetFloat("_Desat", 0f);
                KillButtonManager.buttonLabelText.material.SetFloat("_Desat", 0f);
                KillButtonManager.SetCoolDown(Cooldown, MaxCooldown);
            }
        }

        public bool CouldBeUsed()
        {
            if (PlayerControl.LocalPlayer == null) 
                return false;
            
            if (PlayerControl.LocalPlayer.Data == null) 
                return false;
            
            if (MeetingHud.Instance != null) 
                return false;

            return _CouldBeUsed.Invoke(PlayerControl.LocalPlayer);
        }

        public bool CanBeUsed()
        {
            var flag = false;
            if (Target != TargetType.None)
                flag = PlayerTarget == null && ObjectTarget == null;
            return _CanBeUsed.Invoke(PlayerControl.LocalPlayer) && Usable && Cooldown < 0f && HudActive && !flag;
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
        
        public bool IsCoolingDown()
        {
            return KillButtonManager.isCoolingDown;
        }

        public PlayerControl FindClosestPlayer()
        {
            var from = PlayerControl.LocalPlayer;
            PlayerControl result = null;
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!ShipStatus.Instance)
            {
                return null;
            }

            Vector2 truePosition = from.GetTruePosition();
            foreach (var playerInfo in GameData.Instance.AllPlayers)
            {
                if (!playerInfo.Disconnected && playerInfo.PlayerId != from.PlayerId && !playerInfo.IsDead &&
                    (ChoosePlayerTarget == null || ChoosePlayerTarget.Invoke(playerInfo.Object)) &&
                    !playerInfo.Object.inVent)
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object && @object.Collider.enabled)
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized,
                            magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }

            return result;
        }

        public GameObject FindClosestObject()
        {
            var from = PlayerControl.LocalPlayer;
            GameObject result1 = null;
            GameObject result2 = null;
            float num1 = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            float num2 = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!ShipStatus.Instance)
            {
                return null;
            }

            Vector2 truePosition = from.GetTruePosition();
            foreach (var collider in Physics2D.OverlapCircleAll(truePosition, num1))
            {
                var obj = collider.gameObject;
                if (obj.layer != LayerMask.NameToLayer("Ship") && obj.layer != LayerMask.NameToLayer("UI") &&
                    (obj.layer != LayerMask.NameToLayer("Players") || obj.GetComponent<DeadBody>() != null) &&
                    obj.layer != LayerMask.NameToLayer("Default") &&
                    (obj.GetComponent<SpriteRenderer>() != null || obj.GetComponentInChildren<SpriteRenderer>()) &&
                    (ChooseObjectTarget == null || ChooseObjectTarget.Invoke(obj)))
                {
                    Vector2 vector = new Vector2(obj.transform.position.x, obj.transform.position.y) - truePosition;
                    float magnitude = vector.magnitude;
                    if (magnitude <= num1)
                    {
                        result1 = obj;
                        num1 = magnitude;
                    }
                }
            }

            foreach (var rend in Object.FindObjectsOfType<SpriteRenderer>())
            {
                var obj = rend.gameObject;
                if (obj.layer != LayerMask.NameToLayer("Ship") && obj.layer != LayerMask.NameToLayer("UI") &&
                    (obj.layer != LayerMask.NameToLayer("Players") || obj.GetComponent<DeadBody>() != null) &&
                    obj.layer != LayerMask.NameToLayer("Default") &&
                    (obj.GetComponent<SpriteRenderer>() != null || obj.GetComponentInChildren<SpriteRenderer>()) &&
                    (ChooseObjectTarget == null || ChooseObjectTarget.Invoke(obj)))
                {
                    Vector2 vector = new Vector2(obj.transform.position.x, obj.transform.position.y) - truePosition;
                    float magnitude = vector.magnitude;
                    if (magnitude <= num2)
                    {
                        result2 = obj;
                        num2 = magnitude;
                    }
                }
            }

            return num1 < num2 ? result1 : result2;
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        internal static class HudManagerUpdatePatch
        {
            public static void Prefix(HudManager __instance)
            {
                Buttons.RemoveAll(item => item.KillButtonManager == null);
                for (int i = 0; i < Buttons.Count; i++)
                {
                    var button = Buttons[i];
                    var killButton = button.KillButtonManager;
                    var canUse = button.CouldBeUsed();
                
                    Buttons[i].KillButtonManager.graphic.sprite = button._buttonSprite;
                
                    killButton.gameObject.SetActive(button.Visible && canUse);
                
                    killButton.buttonLabelText.enabled = canUse;
                    killButton.buttonLabelText.alpha =
                        killButton.isCoolingDown ? Palette.DisabledClear.a : Palette.EnabledColor.a;

                    if (canUse && button.Visible)
                        button.Update();
                }
            }
        }
        
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
        internal static class HudManagerSetHudActivePatch
        {
            public static void Prefix(HudManager __instance, [HarmonyArgument(0)] bool isActive)
            {
                HudActive = isActive;
            }
        }

        public enum TargetType
        {
            None,
            Player,
            Object,
            CustomObject
        }
    }
}