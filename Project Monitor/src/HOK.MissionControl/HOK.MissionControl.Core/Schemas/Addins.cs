﻿using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HOK.MissionControl.Core.Schemas
{
    public class AddinLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string pluginName { get; set; }
        public string user { get; set; }
        public string revitVersion { get; set; }
        public int executionTime { get; set; }
        public DateTime createdOn { get; set; } = new DateTime();

        public AddinLog()
        {
        }

        public AddinLog(string name, Document doc)
        {
            pluginName = name;
            user = Environment.UserName;
            revitVersion = BasicFileInfo.Extract(doc.PathName).SavedInVersion;
        }
    }

    public class AddinData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public List<AddinLog> usageLogs { get; set; } = new List<AddinLog>();
    }
}
