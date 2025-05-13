﻿using System;
using System.IO;
using System.Text;
using System.Text.Json;
using LevelImposter.Core;

namespace LevelImposter.Shop;

public static class LIDeserializer
{
    private const float JS_MAX_SAFE_INTEGER = 9007199254740991;

    public static LIMap? DeserializeMap(Stream dataStream, bool spriteDB = true, string? filePath = null)
    {
        try
        {
            // Parse Legacy
            var firstByte = (byte)dataStream.ReadByte();
            dataStream.Position = dataStream.Length - 1;
            var lastByte = (byte)dataStream.ReadByte();
            dataStream.Position = 0;
            var fileExtension = Path.GetExtension(filePath ?? "");

            var isLegacy = firstByte == '{' && lastByte == '}' && fileExtension != "lim2";
            if (isLegacy)
            {
                var legacyMap = JsonSerializer.Deserialize<LIMap>(dataStream);
                if (legacyMap == null)
                    LILogger.Error("Failed to deserialize legacy map data");
                else
                    LegacyConverter.UpdateMap(legacyMap);
                return legacyMap;
            }

            // Map Data Length
            var mapLengthBytes = new byte[4];
            dataStream.Read(mapLengthBytes, 0, 4);
            var mapLength = BitConverter.ToInt32(mapLengthBytes, 0);

            // Read Map Data
            var mapDataBytes = new byte[mapLength];
            dataStream.Read(mapDataBytes, 0, mapLength);
            var mapDataString = Encoding.UTF8.GetString(mapDataBytes);
            var mapData = JsonSerializer.Deserialize<LIMap>(mapDataString);

            // Check Map Data
            if (mapData == null)
                throw new Exception("Failed to deserialize map data");

            // Abort if no SpriteDB
            if (!spriteDB)
                return mapData;

            // Read SpriteDB
            mapData.mapAssetDB = new MapAssetDB();
            while (dataStream.Position < dataStream.Length)
            {
                // Read ID
                var idBytes = new byte[36];
                dataStream.Read(idBytes, 0, 36);
                var idString = Encoding.UTF8.GetString(idBytes);
                var isValidGUID = Guid.TryParse(idString, out var spriteID);
                if (!isValidGUID)
                {
                    LILogger.Error($"Failed to parse sprite ID: {idString}");
                    continue;
                }

                // Read Length
                var lengthBytes = new byte[4];
                dataStream.Read(lengthBytes, 0, 4);
                var dataLength = BitConverter.ToInt32(lengthBytes, 0);

                // Check Length
                if (dataLength <= 0)
                {
                    LILogger.Error($"Invalid data length: {dataLength}");
                    continue;
                }

                // Read Data
                if (filePath != null)
                {
                    // Reading from a file, just save the File Stream offset
                    var fileChunk = new FileChunk(filePath, dataStream.Position, dataLength);
                    mapData.mapAssetDB.Add(spriteID, fileChunk);
                    dataStream.Position += dataLength;
                }
                else
                {
                    // Reading from a stream, save the raw data to memory
                    var buffer = new byte[dataLength];
                    dataStream.Read(buffer, 0, dataLength);
                    mapData.mapAssetDB.Add(spriteID, buffer);
                }
            }

            // Migrate
            MigrateMap(mapData);

            // Return
            return mapData;
        }
        catch (Exception e)
        {
            LILogger.Error(e.Message);
            return null;
        }
    }

    private static void MigrateMap(LIMap map)
    {
        // TODO: Add advanced map migration logic
        foreach (var element in map.elements)
            if (element.type == "util-layer" && map.v < 3)
            {
                element.x = 0;
                element.y = 0;
                element.z = 0;
                element.xScale = 1;
                element.yScale = 1;
                element.rotation = 0;
            }
    }
}