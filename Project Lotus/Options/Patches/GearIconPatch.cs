using TMPro;
using Lotus.GUI;
using Lotus.Roles;
using Lotus.Logging;
using UnityEngine;
using Lotus.Extensions;
using System.Reflection;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;
using System.Linq;

namespace Lotus.Options.Patches;
[LoadStatic]
public class GearIconPatch
{
    public static void AddGearToSettings(RoleOptionSetting roleOption, AbstractBaseRole role)
    {
        PassiveButton settingsButton = roleOption.FindChild<PassiveButton>("Chance %/MinusButton");
        settingsButton = Object.Instantiate(settingsButton, roleOption.transform);
        settingsButton.transform.localPosition = new Vector3(-1.3f, -0.3f, 0);
        settingsButton.transform.localScale = new Vector3(1.03f, 1f, 1f);
        settingsButton.name = "SettingsButton";

        settingsButton.FindChild<TextMeshPro>("Text_TMP").gameObject.SetActive(false);

        SpriteRenderer minusActiveSprite = settingsButton.FindChild<SpriteRenderer>("ButtonSprite");
        minusActiveSprite.sprite = LotusAssets.LoadSprite("gearicon.png");
        minusActiveSprite.color = Color.white;
        settingsButton.Modify(() =>
        {
            if (role.RoleOptions.Tab == null)
            {
                DevLogger.Log("tab is null.");
                return;
            }
            role.RoleOptions.Tab
                .GetType()
                .GetMethod("HandleClick", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?
                .Invoke(role.RoleOptions.Tab, null);

            // do some VERY COMPLEX math to figure out the correct height of the current option
            var optionList = role.RoleOptions.Tab.PreRender();
            optionList = optionList.Take(optionList.IndexOf(role.RoleOptions)).ToList();

            int roleOptionCount = optionList.Where(o => o.OptionType == VentLib.Options.Enum.OptionType.Role).Count();
            int totalCount = optionList.Count;

            float newHeight = (2.7f * roleOptionCount) + (0.45f * (totalCount - roleOptionCount)) - .1f; // math isn't perfect. it does get desynced after a bunch of options

            var scrollBar = DestroyableSingleton<RolesSettingsMenu>.Instance.scrollBar;
            Vector3 vector = new(scrollBar.Inner.transform.localPosition.x, newHeight, scrollBar.Inner.transform.localPosition.z);
            scrollBar.Inner.transform.localPosition = vector;
            scrollBar.UpdateScrollBars();
        });
    }
}