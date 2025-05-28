using System;
using Lotus.Extensions.Interfaces;
using Lotus.GUI;
using Lotus.Roles.GUI.Interfaces;
using Lotus.Utilities;
using TMPro;
using UnityEngine;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using Object = UnityEngine.Object;

namespace Lotus.Roles.GUI;

public class RoleButton: IRoleButtonEditor, ICloneable<RoleButton>
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(RoleUIManager));

    public bool IsOverriding { get; private set; }

    protected Sprite Sprite
    {
        get => lazySprite?.Get() ?? _sprite;
        private set => _sprite = value;
    }

    private UnityOptional<ActionButton> underlyingButton;
    private ActionButton setButton = null!;

    private Func<ActionButton> buttonSupplier;
    private Func<int>? usesSupplier;
    private Cooldown? boundCooldown;
    private Material? material;

    private Sprite originalSprite = null!;
    private Sprite _sprite = null!;
    private LazySprite? lazySprite;
    private string localizedText;
    private bool changedSprite;

    public RoleButton(Func<ActionButton> buttonSupplier)
    {
        underlyingButton = UnityOptional<ActionButton>.Of(buttonSupplier.Invoke());
        underlyingButton.IfPresent(b =>
        {
            localizedText = b.buttonLabelText.text;
            Sprite = this.originalSprite = b.graphic.sprite;

            if (b.usesRemainingSprite == null)
            {
                b.usesRemainingSprite = Object.Instantiate(HudManager.Instance.AbilityButton.usesRemainingSprite, b.gameObject.transform);
                b.usesRemainingSprite.transform.localPosition += new Vector3(0, 0, -34f);
                b.usesRemainingText = b.usesRemainingSprite.gameObject.FindChildOrEmpty<TextMeshPro>("Text_TMP", true).OrElseGet(() =>
                    Object.Instantiate(HudManager.Instance.AbilityButton.usesRemainingText,
                        b.usesRemainingSprite.gameObject.transform));
                b.usesRemainingSprite.gameObject.SetActive(false);
                b.usesRemainingText.gameObject.SetActive(false);
            }

            if (b.cooldownTimerText == null)
            {
                b.cooldownTimerText = Object.Instantiate(HudManager.Instance.KillButton.cooldownTimerText, b.gameObject.transform);
                b.cooldownTimerText.transform.localPosition += new Vector3(0, 0, -20);
                b.cooldownTimerText.gameObject.SetActive(false);
            }
        });
        this.buttonSupplier = buttonSupplier;
        IsOverriding = true;
    }

    public RoleButton BindCooldown(Cooldown? cooldown)
    {
        boundCooldown = cooldown;
        ActionButton button = GetButton();

        button.cooldownTimerText.gameObject.SetActive(cooldown != null);

        return this;
    }

    public RoleButton BindUses(Func<int>? usesSupplier)
    {
        ActionButton button = GetButton();
        button.usesRemainingSprite.gameObject.SetActive(usesSupplier != null);
        button.usesRemainingText.gameObject.SetActive(usesSupplier != null);

        this.usesSupplier = usesSupplier;
        return this;
    }

    public RoleButton Default(bool skipThisButton)
    {
        IsOverriding = !skipThisButton;
        return this;
    }

    public RoleButton SetText(string localizedText)
    {
        this.localizedText = localizedText;
        return this;
    }

    public RoleButton SetSprite(Func<Sprite> spriteLoadingFunction)
    {
        lazySprite = new(spriteLoadingFunction);
        return SetSprite(lazySprite);
    }

    public RoleButton SetSprite(LazySprite sprite)
    {
        lazySprite = sprite;
        Sprite = lazySprite.Get();
        changedSprite = true;
        return this;
    }

    public RoleButton SetMaterial(Material material)
    {
        this.material = material;
        return this;
    }

    public RoleButton RevertSprite()
    {
        if (originalSprite != null) this.SetSprite(() => originalSprite);
        return this;
    }

    public Sprite GetSprite() => Sprite;

    public string GetText() => localizedText;

    internal void CompleteLoad() => setButton = GetButton();
    public ActionButton GetButton()
    {
        if (!ReferenceEquals(setButton, null)) return setButton;
        ActionButton button = underlyingButton.OrElseSet(buttonSupplier);
        if (originalSprite == null) originalSprite = button.graphic.sprite;

        if (button.usesRemainingSprite == null)
        {
            button.usesRemainingSprite = Object.Instantiate(HudManager.Instance.AbilityButton.usesRemainingSprite, button.gameObject.transform);
            button.usesRemainingSprite.transform.localPosition += new Vector3(0, 0, -34f);
            button.usesRemainingText = button.usesRemainingSprite.gameObject.FindChildOrEmpty<TextMeshPro>("Text_TMP").OrElseGet(() =>
                Object.Instantiate(HudManager.Instance.AbilityButton.usesRemainingText,
                    button.usesRemainingSprite.gameObject.transform));
            bool flag = this.usesSupplier != null;
            button.usesRemainingSprite.gameObject.SetActive(flag);
            button.usesRemainingText.gameObject.SetActive(flag);
        }
        if (button.cooldownTimerText == null)
        {
            button.cooldownTimerText = Object.Instantiate(HudManager.Instance.KillButton.cooldownTimerText, button.gameObject.transform);
            button.cooldownTimerText.transform.localPosition += new Vector3(0, 0, -20);
            button.cooldownTimerText.gameObject.SetActive(boundCooldown != null);
        }
        return button;
    }

    public void Update()
    {
        ActionButton button = GetButton();
        if (button == null) return;

        if (usesSupplier != null)
        {
            int uses = usesSupplier();
            if (uses is <= -1 or int.MaxValue) button.SetInfiniteUses();
            else
            {
                button.SetUsesRemaining(uses);
                if (uses == 0) button.SetDisabled();
            }

            if (uses == 0) button.SetDisabled();
        }
        else
        {
            button.usesRemainingSprite.gameObject.SetActive(false);
            button.usesRemainingText.gameObject.SetActive(false);
        }

        if (changedSprite)
        {
            changedSprite = false;
            button.graphic.sprite = Sprite;
            button.graphic.SetCooldownNormalizedUvs();
        }
        button.buttonLabelText.text = localizedText;
        if (material != null) button.buttonLabelText.fontSharedMaterial = material;
        if (boundCooldown == null) return;
        button.SetCoolDown(boundCooldown.TimeRemaining(), boundCooldown.Duration);
    }

    internal void ForceUpdate()
    {
        // ActionButton button = GetButton();
        // button.buttonLabelText.text = localizedText;
        // button.graphic.sprite = GetSprite();
    }

    public RoleButton Clone() => (RoleButton)this.MemberwiseClone();
}