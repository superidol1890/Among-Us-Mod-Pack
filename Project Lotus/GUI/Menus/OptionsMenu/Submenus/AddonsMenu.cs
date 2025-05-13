using System;
using System.Globalization;
using TMPro;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;
using Lotus.GUI.Menus.OptionsMenu.Components;
using Lotus.Options;

namespace Lotus.GUI.Menus.OptionsMenu.Submenus;

[RegisterInIl2Cpp]
public class AddonsMenu : MonoBehaviour, IBaseOptionMenuComponent
{
    private TextMeshPro titleHeader;
    private GameObject anchorObject;

    public AddonsMenu(IntPtr intPtr) : base(intPtr)
    {
        anchorObject = gameObject.CreateChild("Addons");
        anchorObject.transform.localPosition += new Vector3(2f, 2f);

        titleHeader = Instantiate(FindObjectOfType<TextMeshPro>(), anchorObject.transform);
        titleHeader.font = CustomOptionContainer.GetGeneralFont();
        titleHeader.transform.localPosition = new Vector3(8.05f, -1.85f, 0f);
        titleHeader.name = "MainTitle";
    }

    private void Start()
    {
        titleHeader.text = "Addons";
    }

    public void PassMenu(OptionsMenuBehaviour menuBehaviour)
    {
        anchorObject.SetActive(false);
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