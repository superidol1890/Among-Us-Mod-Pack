using System;
using MiraAPI.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace LaunchpadReloaded.Components;

[RegisterInIl2Cpp]
public class GradientColorComponent(IntPtr ptr) : MonoBehaviour(ptr)
{
    public SpriteRenderer renderer = null!;
    public Material mat = null!;

    public int primaryColor;
    public int secondaryColor;

    public void SetColor(int color1, int color2)
    {
        renderer = GetComponent<SpriteRenderer>();
        if (!renderer)
        {
            return;
        }

        if (color2 > Palette.PlayerColors.Length || color2 < 0)
        {
            if (color1 >= 0 && color1 < Palette.PlayerColors.Length)
            {
                PlayerMaterial.SetColors(primaryColor, mat);
            }
            return;
        }

        primaryColor = color1;
        secondaryColor = color2;

        mat = renderer.material;

        PlayerMaterial.SetColors(primaryColor, mat);

        mat.SetColor(ShaderID.Get("_BodyColor2"), Palette.PlayerColors[secondaryColor]);
        mat.SetColor(ShaderID.Get("_BackColor2"), Palette.ShadowColors[secondaryColor]);
    }

    public void SetColor(int gradientColor)
    {
        renderer = GetComponent<SpriteRenderer>();
        if (!renderer)
        {
            return;
        }

        if (gradientColor > Palette.PlayerColors.Length)
        {
            return;
        }

        secondaryColor = gradientColor;

        mat = renderer.material;

        mat.SetColor(ShaderID.Get("_BodyColor2"), Palette.PlayerColors[secondaryColor]);
        mat.SetColor(ShaderID.Get("_BackColor2"), Palette.ShadowColors[secondaryColor]);
    }

    public void SetColor(Color baseColor, Color gradientColor)
    {
        renderer = GetComponent<SpriteRenderer>();
        if (!renderer)
        {
            return;
        }

        mat = renderer.material;

        PlayerMaterial.SetColors(baseColor, mat);

        mat.SetColor(ShaderID.Get("_BodyColor2"), gradientColor);
        mat.SetColor(ShaderID.Get("_BackColor2"), gradientColor);
    }

    public void Update()
    {
        if (!renderer || !mat)
        {
            return;
        }

        mat.SetFloat(ShaderID.Get("_Flip"), renderer.flipX ? 1 : 0);
    }
}