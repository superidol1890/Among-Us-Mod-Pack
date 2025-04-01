using System;
using InnerNet;
using System.Text;
using System.Security.Cryptography;

namespace Lotus.Extensions;

public static class ClientDataExtension
{
    // THANKS TOHE!!!!!!!!!!!!
    public static string GetHashedPuid(this ClientData player)
    {
        if (player == null) return "";
        string puid = player.ProductUserId;
        using SHA256 sha256 = SHA256.Create();

        // get sha-256 hash
        byte[] sha256Bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(puid));
        string sha256Hash = BitConverter.ToString(sha256Bytes).Replace("-", "").ToLower();

        // pick front 5 and last 4
        return string.Concat(sha256Hash.AsSpan(0, 5), sha256Hash.AsSpan(sha256Hash.Length - 4));
    }
}