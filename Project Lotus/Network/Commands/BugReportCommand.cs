using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lotus.API.Player;
using Lotus.Chat;
using Lotus.Logging;
using Lotus.Network.PrivacyPolicy;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using IEnumerator = System.Collections.IEnumerator;

namespace Lotus.Network.Commands;

[Localized("Commands.BugReports")]
[Command("report")]
public class BugReportCommand : ICommandReceiver
{
    [Localized(nameof(WaitingOnReport))] public static string WaitingOnReport = "This report has been voided as we are waiting on a response for your previous report.";
    [Localized(nameof(FailedReport))] public static string FailedReport = "An error occured while sending the request. Please try again in a minute.\nError: {0}";
    [Localized(nameof(ReportingBug))] public static string ReportingBug = "Your request has went through and we are now reporting your bug. Thanks!";
    [Localized(nameof(ReportedBug))] public static string ReportedBug = "Your bug has been successfully reported. Again, thank you!";
    [Localized(nameof(ReportSuggest))] public static string ReportSuggest = "{0} suggests you to report \"{1}\".";
    private const string boundary = "---------------------------7d5e3d6f4509c";
    private static readonly List<byte> reportingPlayerIds = new();
    public void Receive(PlayerControl source, CommandContext context)
    {
        if (!ProjectLotus.DevVersion)
        {
            ChatHandlers.NotPermitted().Send(source);
            return;
        }

        if (PrivacyPolicyInfo.Instance != null)
        {
            if (!PrivacyPolicyInfo.Instance.ConnectWithAPI) return;
        }
        if (reportingPlayerIds.Contains(source.PlayerId))
        {
            ChatHandlers.NotPermitted(WaitingOnReport).Send(source);
            return;
        }
        reportingPlayerIds.Add(source.PlayerId);
        ChatHandler.Of(ReportingBug).Send(source);

        Async.Execute(SendWebRequest(source, context));
    }

    private IEnumerator SendWebRequest(PlayerControl source, CommandContext context)
    {
        string message = string.Join(" ", context.Args);
        if (!source.IsHost())
        {
            ChatHandler.Of(ReportSuggest.Formatted(source.name, message)).Send(PlayerControl.LocalPlayer);
            reportingPlayerIds.Remove(source.PlayerId);
            yield break;
        }
        bool flag = PrivacyPolicyInfo.Instance != null && PrivacyPolicyInfo.Instance.AnonymousBugReports;
        if (source == PlayerControl.LocalPlayer && message == "log")
        {
            reportingPlayerIds.Remove(source.PlayerId);
            FileInfo? lastLogFile = GetLatestLogFile();
            if (lastLogFile == null)
            {
                LogManager.SendInGame("Could not find latest log file. Please make sure logs are enabled.");
                yield break;
            }

            byte[] fileData = File.ReadAllBytes(lastLogFile.FullName);
            string fileName = lastLogFile.Name;

            // why can't we just use a WWWFORM. like whyyyyyy man
            UnityWebRequest request = UnityWebRequest.Post(NetConstants.Host + "uploadlog", UnityWebRequest.kHttpVerbPOST);
            StringBuilder sb = new();
            sb.Append("--" + boundary + "\r\n");
            sb.Append("Content-Disposition: form-data; name=\"logFile\"; filename=\"" + fileName + "\"\r\n"); // Use "logFile" as the field name
            sb.Append("Content-Type: application/octet-stream\r\n\r\n");

            // Prepare the headers and file data
            byte[] headerBytes = Encoding.UTF8.GetBytes(sb.ToString());
            byte[] footerBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n" +
                "Content-Disposition: form-data; name=\"hostName\"\r\n\r\n" + GetName() + "\r\n" +
                "--" + boundary + "--\r\n");

            // Combine header, file data, and footer
            byte[] bodyRaw = new byte[headerBytes.Length + fileData.Length + footerBytes.Length];
            Buffer.BlockCopy(headerBytes, 0, bodyRaw, 0, headerBytes.Length);
            Buffer.BlockCopy(fileData, 0, bodyRaw, headerBytes.Length, fileData.Length);
            Buffer.BlockCopy(footerBytes, 0, bodyRaw, headerBytes.Length + fileData.Length, footerBytes.Length);

            // Set up the request
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "multipart/form-data; boundary=" + boundary);

            // Send the request and wait for a response
            yield return request.SendWebRequest();

            // Handle the result
            if (request.result == UnityWebRequest.Result.Success)
            {
                ChatHandler.Of(ReportedBug).Send(source);
            }
            else
            {
                ChatHandler.Of("Result: {0}\nError: {1}\nResponseCode: {2}\nSever Response: {3}".Formatted(request.result.ToString(), request.error,
                    request.responseCode, request.downloadHandler.text)).LeftAlign().Send(source);
            }

            yield break;
        }
        if (string.IsNullOrEmpty(message) || message.Length < 7)
        {
            reportingPlayerIds.Remove(source.PlayerId);
            yield break;
        }
        FrozenPlayer frozenPlayer = new(source);
        string? errorMessage = null;

        // send report.

        string jsonData = $@"{{
            ""playerName"": ""{GetName()}"",
            ""friendCode"": ""{GetFriendCode()}"",
            ""message"": ""{message}""
        }}";
        byte[] postData = new System.Text.UTF8Encoding().GetBytes(jsonData);

        // Send POST request
        // Server checks for anon FC and sends it to the specified channel.
        UnityWebRequest webRequest = new(NetConstants.Host + "reportbug", UnityWebRequest.kHttpVerbPOST)
        {
            uploadHandler = new UploadHandlerRaw(postData),
            downloadHandler = new DownloadHandlerBuffer()
        };
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        switch (webRequest.result)
        {
            case UnityWebRequest.Result.Success:
                break;
            default:
                errorMessage = "Result: {0}\nError: {1}\nResponseCode: {2}\nSever Response: {3}".Formatted(webRequest.result.ToString(), webRequest.error,
                    webRequest.responseCode, webRequest.downloadHandler.text);
                break;
        }

        if (errorMessage == null) ChatHandler.Of(ReportedBug).Send(source);
        else ChatHandler.Of(FailedReport.Formatted(errorMessage)).Send(source);
        webRequest.Dispose();

        reportingPlayerIds.Remove(frozenPlayer.PlayerId);
        string GetName() => flag ? "Anonymous" : source.name;
        string GetFriendCode() => flag ? "anonymous#1234" : source.FriendCode;
    }

    public static FileInfo? GetLatestLogFile()
    {
        DirectoryInfo logsDirectory = new("logs");
        if (!logsDirectory.Exists) return null;
        FileInfo[] logs = logsDirectory.GetFiles();
        if (logs.Length == 0) return null;
        return logs
            .OrderByDescending(file => file.LastWriteTime)
            .FirstOrDefault();
    }
}