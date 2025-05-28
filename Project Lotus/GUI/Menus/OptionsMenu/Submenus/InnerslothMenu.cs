using System;
using TMPro;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;
using Lotus.GUI.Menus.OptionsMenu.Components;
using Lotus.GUI.Menus.OptionsMenu.Patches;
using Lotus.Logging;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.GUI.Menus.OptionsMenu.Submenus;

[RegisterInIl2Cpp]
public class InnerslothMenu : MonoBehaviour, IBaseOptionMenuComponent
{
    private TextMeshPro titleHeader;
    private GameObject anchorObject;

    private MonoToggleButton privacyPolicyLink;
    private MonoToggleButton termsOfUseLink;
    private MonoToggleButton dataCollection;

    public InnerslothMenu(IntPtr intPtr) : base(intPtr)
    {
        anchorObject = gameObject.CreateChild("Innersloth");
        anchorObject.transform.localPosition += new Vector3(2f, 2f);
        anchorObject.transform.localScale = new Vector3(1f, 1f, 1);

        titleHeader = Instantiate(FindObjectOfType<TextMeshPro>(), anchorObject.transform);
        titleHeader.font = CustomOptionContainer.GetGeneralFont();
        titleHeader.transform.localPosition = new Vector3(7.06f, -1.85f, 0);
        titleHeader.name = "MainTitle";
    }

    private void Start()
    {
        titleHeader.text = "Among Us Data";
    }

    public void PassMenu(OptionsMenuBehaviour menuBehaviour)
    {
        anchorObject.SetActive(false);

        DevLogger.Log("checking if exists");
        UnityOptional<PassiveButton> dataCollectionButton = menuBehaviour.FindChildOrEmpty<PassiveButton>("DataCollectionButton", true);
        DevLogger.Log("idk at this point");
        if (!dataCollectionButton.Exists())
        {
            // This means we are in game.
            DevLogger.Log("in game");
            CustomOptionContainer container = menuBehaviour.gameObject.GetComponent<CustomOptionContainer>();
            Async.WaitUntil(() => container.innerslothButton, button => button.gameObject.SetActive(false));
            return;
        }
        DevLogger.Log("it exists");

        bool openDataCollection = false; // pl auto runs the action when it is made visible. so we keep track of the first visit to stop this.

        GameObject dataCollectionObject = new("Manage Data Collection");
        dataCollectionObject.transform.SetParent(anchorObject.transform);
        dataCollectionObject.transform.localScale = new Vector3(1f, 1f, 1f);
        dataCollection = dataCollectionObject.AddComponent<MonoToggleButton>();
        dataCollection.ConfigureAsPressButton("Manage Data Collection", () =>
        {
            if (!openDataCollection)
            {
                openDataCollection = true;
                return;
            }
            dataCollectionButton.Get().OnClick.Invoke();
        });
        dataCollectionObject.transform.localPosition = new Vector3(0.029f, 0.353f, -1f);

        bool openPrivacyLink = false; // pl auto runs the action when it is made visible. so we keep track of the first visit to stop this.

        GameObject privacyPolicyLinkObject = new("Privacy Policy Link");
        privacyPolicyLinkObject.transform.SetParent(anchorObject.transform);
        privacyPolicyLinkObject.transform.localScale = new Vector3(1f, 1f, 1f);
        privacyPolicyLink = privacyPolicyLinkObject.AddComponent<MonoToggleButton>();
        privacyPolicyLink.ConfigureAsPressButton("View Privacy Policy", () =>
        {
            if (!openPrivacyLink)
            {
                openPrivacyLink = true;
                return;
            }
            menuBehaviour.OpenPrivacyPolicy();
        });
        privacyPolicyLinkObject.transform.localPosition = new Vector3(0.029f, -1.014f, -1f);

        bool openTermsOfUseLink = false; // pl auto runs the action when it is made visible. so we keep track of the first visit to stop this.

        GameObject termsOfUseLinkObject = new("Terms Of Use Link");
        termsOfUseLinkObject.transform.SetParent(anchorObject.transform);
        termsOfUseLinkObject.transform.localScale = new Vector3(1f, 1f, 1f);
        termsOfUseLink = termsOfUseLinkObject.AddComponent<MonoToggleButton>();
        termsOfUseLink.ConfigureAsPressButton("View Terms Of Use", () =>
        {
            if (!openTermsOfUseLink)
            {
                openTermsOfUseLink = true;
                return;
            }
            menuBehaviour.OpenTermsOfUse();
        });
        termsOfUseLinkObject.transform.localPosition = new Vector3(0.029f, -2.381f, -1f);

        // bring over Innersloth stuff
        DevLogger.Log("checking twitch link");
        PassiveButton twitchButton = menuBehaviour.FindChild<PassiveButton>("TwitchLinkButton", true);
        DevLogger.Log("checking show puid");
        PassiveButton puidButton = menuBehaviour.FindChild<PassiveButton>("ShowPUID", true);

        twitchButton.transform.SetParent(anchorObject.transform);
        puidButton.transform.SetParent(anchorObject.transform);

        twitchButton.transform.localScale = Vector3.one * 0.3f;
        twitchButton.transform.localPosition = new Vector3(1.848f, -1.695f, -1f);

        puidButton.transform.localScale = Vector3.one;
        puidButton.transform.localPosition = new Vector3(-1.1f, -4.548f, -1);
    }

    public virtual void Open()
    {
        anchorObject.SetActive(true);
    }

    public virtual void Close()
    {
        anchorObject.SetActive(false);
    }

}