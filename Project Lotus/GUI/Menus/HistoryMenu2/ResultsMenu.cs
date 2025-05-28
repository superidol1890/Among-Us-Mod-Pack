﻿using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Stats;
using Lotus.Managers.History;
using Lotus.Roles;
using Lotus.Utilities;
using TMPro;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;

namespace Lotus.GUI.Menus.HistoryMenu2;

[Localized("GUI.HistoryMenu.ResultsMenu")]
[RegisterInIl2Cpp]
public class ResultsMenu : MonoBehaviour, IHistoryMenuChild
{
    public static List<Statistic> EligibleEndgameStats = DisplayEligibleStats();
    private PoolablePlayer playerPrefab = null!;
    private GameObject anchor;

    private GameObject tabIconObject;
    private PassiveButton tabButton = null!;
    private SpriteRenderer tabButtonRenderer = null!;
    private bool initialized;

    public ResultsMenu(IntPtr intPtr) : base(intPtr)
    {
        anchor = gameObject.CreateChild("Anchor");
        tabIconObject = gameObject.CreateChild("Tab Icon");
    }

    public void PassHudManager(HudManager hudManager)
    {
        playerPrefab = Instantiate(hudManager.IntroPrefab.PlayerPrefab, anchor.transform);
        playerPrefab.transform.localScale = new Vector3(0.4f, 0.4f, 1);
        playerPrefab.transform.localPosition = new Vector3(-3.1f, 1.45f);
        playerPrefab.cosmetics.nameText.transform.localPosition -= new Vector3(0f, 1.15f);
        playerPrefab.cosmetics.nameText.transform.localScale += new Vector3(1f, 1f);
        playerPrefab.gameObject.SetActive(false);
    }

    public PassiveButton CreateTabButton(PassiveButton prefab)
    {
        tabButton = Instantiate(prefab, tabIconObject.transform);
        tabButtonRenderer = tabButton.GetComponentsInChildren<SpriteRenderer>().Last();
        tabButtonRenderer.sprite = LotusAssets.LoadSprite("HistoryMenu/ResultsIcon.png", 100);
        tabButton.transform.localPosition += new Vector3(-6.9f, 4.102f);
        return tabButton;
    }

    public void Open()
    {
        anchor.SetActive(true);
        tabButtonRenderer.color = Palette.DisabledClear;
        if (!initialized) PopulateResults();
    }

    public void Close()
    {
        anchor.SetActive(false);
        tabButtonRenderer.color = Palette.EnabledColor;
    }

    public void PopulateResults()
    {
        List<PlayerHistory>? allPlayers = Game.MatchData.GameHistory.PlayerHistory;
        if (allPlayers == null) return;
        HashSet<byte> winners = Game.MatchData.GameHistory.LastWinners.Select(p => p.MyPlayer.PlayerId).ToHashSet();

        allPlayers = allPlayers.Sorted(p => winners.Contains(p.PlayerId) ? (int)p.Status : 99 + (int)p.Status).ToList();

        playerPrefab.gameObject.SetActive(true);
        playerPrefab.UpdateFromLocalPlayer(PlayerMaterial.MaskType.SimpleUI);


        for (int index = 0; index < allPlayers.Count; index++)
        {
            PlayerHistory playerHistory = allPlayers[index];
            CustomRole role = playerHistory.MainRole;
            PoolablePlayer newPlayer = Instantiate(playerPrefab, anchor.transform);
            newPlayer.enabled = true;
            newPlayer.cosmetics.initialized = false;
            newPlayer.UpdateFromPlayerOutfit(playerHistory.Outfit, PlayerMaterial.MaskType.SimpleUI, false, false);

            int row = Mathf.FloorToInt(index / 5f);

            newPlayer.transform.localPosition += new Vector3(1.65f * (index - (row * 5)), -row * 1.35f, 0);


            string statText = playerHistory.MainRole.Statistics().FirstOrOptional().Map(t => $" <size=1.5>[{t.Name()}: {t.GetGenericValue(playerHistory.UniquePlayerId)}]</size>").OrElse("");

            string historyName = $"{playerHistory.Name}\n{role.RoleColor.Colorize(role.RoleName)}\n<size=1.4>{statText}</size>";
            newPlayer.SetName(historyName);

            TextMeshPro aboveNameTmp = Instantiate(newPlayer.cosmetics.nameText, newPlayer.transform);
            aboveNameTmp.transform.localPosition += new Vector3(0f, 2.49f, 0f);
            aboveNameTmp.transform.localScale += new Vector3(1f, 1f, 0f);

            string statusText = playerHistory.Status is PlayerStatus.Dead ? playerHistory.CauseOfDeath?.SimpleName() ?? playerHistory.Status.ToString() : playerHistory.Status.ToString();

            string aboveNameText = winners.Contains(playerHistory.PlayerId) ? new Color(1f, 0.83f, 0.24f).Colorize("Winner") : "";
            aboveNameText += "\n<size=1.25>" + StatusColor(playerHistory.Status).Colorize(statusText) + "</size>";
            aboveNameTmp.text = aboveNameText;
        }

        playerPrefab.gameObject.SetActive(false);
        initialized = true;
    }

    private static Color StatusColor(PlayerStatus status)
    {
        return status switch
        {
            PlayerStatus.Alive => Color.green,
            PlayerStatus.Exiled => new Color(0.73f, 0.54f, 0.45f),
            PlayerStatus.Disconnected => Color.grey,
            PlayerStatus.Dead => Color.red,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    private static List<Statistic> DisplayEligibleStats() => new()
    {
        VanillaStatistics.Kills, VanillaStatistics.BodiesReported, VanillaStatistics.SabotagesCalled,
        VanillaStatistics.SabotagesFixed, VanillaStatistics.PlayersExiled
    };


}