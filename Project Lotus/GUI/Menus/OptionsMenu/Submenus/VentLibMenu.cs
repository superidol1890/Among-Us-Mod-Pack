using System;
using Lotus.GUI.Menus.OptionsMenu.Components;
using Lotus.Network.PrivacyPolicy;
using TMPro;
using Lotus.Utilities;
using UnityEngine;
using UnityEngine.Events;
using VentLib.Networking;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;
using Lotus.Network.PrivacyPolicy.Patches;

namespace Lotus.GUI.Menus.OptionsMenu.Submenus;

[RegisterInIl2Cpp]
public class VentLibMenu : MonoBehaviour, IBaseOptionMenuComponent
{
    private MonoToggleButton allowLobbySending;
    private TextMeshPro allowLobbySendingText;

    private SlideBar maxPacketSizeSlider;
    private TextMeshPro maxPacketSizeLabel;
    private TextMeshPro maxPacketSizeValue;

    private TextMeshPro menuTitle;

    private GameObject anchorObject;

    public VentLibMenu(IntPtr intPtr) : base(intPtr)
    {
        anchorObject = gameObject.CreateChild("VentLib");
        anchorObject.transform.localPosition += new Vector3(2f, 2f);
        anchorObject.transform.localScale = new Vector3(1f, 1f, 1);

        menuTitle = Instantiate(FindObjectOfType<TextMeshPro>(), anchorObject.transform);
        menuTitle.font = CustomOptionContainer.GetGeneralFont();
        menuTitle.transform.localPosition = new Vector3(7.58f, -1.85f, 0);
        menuTitle.name = "MainTitle";
    }

    private void Start()
    {
        menuTitle.text = "Networking";
    }

    public void PassMenu(OptionsMenuBehaviour menuBehaviour)
    {
        maxPacketSizeSlider = Instantiate(menuBehaviour.MusicSlider, anchorObject.transform);
        maxPacketSizeSlider.transform.localPosition = new Vector3(-3.82f, -0.44f, 0);
        maxPacketSizeSlider.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

        maxPacketSizeLabel = maxPacketSizeSlider.GetComponentInChildren<TextMeshPro>();
        maxPacketSizeLabel.transform.localPosition = new Vector3(1.1f, 0.2f, 0);
        maxPacketSizeLabel.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        maxPacketSizeLabel.name = "MaxPacketSize_TMP";

        maxPacketSizeValue = Instantiate(maxPacketSizeLabel, maxPacketSizeSlider.transform);
        maxPacketSizeValue.name = "Value_TMP";

        maxPacketSizeValue.transform.localPosition += new Vector3(4f, 0.25f);
        maxPacketSizeSlider.OnValueChange = new UnityEvent();
        maxPacketSizeSlider.OnValueChange.AddListener((Action)(() =>
        {
            int packetSize = NetworkRules.MaxPacketSize = Mathf.FloorToInt(maxPacketSizeSlider.Value * (NetworkRules.AbsoluteMaxPacketSize - NetworkRules.AbsoluteMinPacketSize)) + NetworkRules.AbsoluteMinPacketSize;
            maxPacketSizeValue.text = packetSize.ToString();
        }));

        GameObject lobbyObject = anchorObject.CreateChild("Lobby Object");
        lobbyObject.transform.localPosition = new Vector3(1.04f, -0.328f, -1f);

        allowLobbySending = lobbyObject.AddComponent<MonoToggleButton>();
        allowLobbySendingText = Instantiate(FindObjectOfType<TextMeshPro>(), allowLobbySending.transform);
        allowLobbySendingText.font = CustomOptionContainer.GetGeneralFont();
        allowLobbySendingText.transform.localPosition = new Vector3(-4.65f, -0.5f, -1);
        allowLobbySendingText.transform.localScale = new Vector3(2.9f, 1.6f, 1f);

        allowLobbySending.SetOnText("ON");
        allowLobbySending.SetOffText("OFF");
        allowLobbySending.SetToggleOnAction(() =>
        {
            PrivacyPolicyPatch.EditPrivacyPolicy(PrivacyPolicyEditType.LobbyDiscovery, true);
            NetworkRules.AllowRoomDiscovery = true;
        });
        allowLobbySending.SetToggleOffAction(() =>
        {
            PrivacyPolicyPatch.EditPrivacyPolicy(PrivacyPolicyEditType.LobbyDiscovery, false);
            NetworkRules.AllowRoomDiscovery = false;
        });
        NetworkRules.AllowRoomDiscovery = PrivacyPolicyInfo.Instance.LobbyDiscovery;
        allowLobbySending.SetState(NetworkRules.AllowRoomDiscovery);

        anchorObject.SetActive(false);
    }

    public void Open()
    {
        anchorObject.SetActive(true);
        maxPacketSizeSlider.SetValue((float)NetworkRules.MaxPacketSize / NetworkRules.AbsoluteMaxPacketSize);
        Async.Schedule(() =>
        {
            maxPacketSizeLabel.text = "Max Packet Size";
            maxPacketSizeValue.text = NetworkRules.MaxPacketSize.ToString();
            allowLobbySendingText.text = "Open Rooms To Discovery";
            allowLobbySendingText.color = Color.white;
        }, 0.00001f);
    }

    public void Close()
    {
        anchorObject.SetActive(false);
    }
}