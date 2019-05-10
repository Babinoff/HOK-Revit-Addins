﻿#region References

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Settings;
using HOK.MissionControl.Core.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

#endregion

namespace HOK.MissionControl.Core.Schemas
{
    public class InfoItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class AddinLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("pluginName")]
        public string PluginName { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("revitVersion")]
        public string RevitVersion { get; set; }

        [JsonProperty("office")]
        public string Office { get; set; }

        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [JsonProperty("detailInfo")]
        public List<InfoItem> DetailInfo { get; set; } = new List<InfoItem>();

        public AddinLog()
        {
        }

        public AddinLog(string name, string version)
        {
            PluginName = name;
            User = Environment.UserName.ToLower();
            RevitVersion = version;
            Office = GetOffice();
        }

        /// <summary>
        /// Retrieves office name from machine name ex. NY
        /// </summary>
        /// <returns>Office name.</returns>
        private static string GetOffice()
        {
            try
            {
                var settings = AppSettings.Instance.UserLocation;
                string sourceString;
                switch (settings.Source)
                {
                    case UserLocationSources.MachineName:
                        sourceString = Environment.MachineName;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (string.IsNullOrWhiteSpace(sourceString)) return string.Empty;

                // (Konrad) If we got a Source string ie. Machine Name we can proceed
                // to extract the user location/office from it using regex pattern.
                var match = Regex.Match(sourceString, settings.Pattern, RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count >= settings.Group)
                {
                    return match.Groups[settings.Group].Value;
                }

                return string.Empty;
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                return string.Empty;
            }
        }
    }
}
