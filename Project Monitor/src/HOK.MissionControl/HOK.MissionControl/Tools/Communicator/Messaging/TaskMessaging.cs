﻿using System.Collections.Generic;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Sheets;

namespace HOK.MissionControl.Tools.Communicator.Messaging
{
    public class FamilyTaskUpdatedMessage
    {
        public string FamilyName { get; set; }
        public FamilyData FamilyStat { get; set; }
        public string OldTaskId { get; set; }
    }

    public class FamilyTaskDeletedMessage
    {
        public List<string> DeletedIds { get; set; }
    }

    public class FamilyTaskAddedMessage
    {
        public string FamilyName { get; set; }
        public FamilyData FamilyStat { get; set; }
    }

    public class TaskAssistantClosedMessage
    {
        public bool IsClosed { get; set; }
    }

    public class SheetsTaskAddedMessage
    {
        public SheetItem Sheet { get; set; }
        public SheetTask Task { get; set; }
    }

    public class SheetsTaskUpdatedMessage
    {
        public SheetItem Sheet { get; set; }
        public SheetTask Task { get; set; }
    }

    public class SheetsTaskSheetAddedMessage
    {
        public List<SheetItem> NewSheets { get; set; }
    }

    public class SheetsTaskApprovedMessage
    {
        public string Identifier { get; set; }
        public SheetItem Sheet { get; set; }
    }

    public class SheetsTaskApprovedNewSheetMessage
    {
        public string Identifier { get; set; }
        public SheetItem Sheet { get; set; }
    }

    public class SheetsTaskDeletedMessage
    {
        public string Identifier { get; set; }
        public List<string> Deleted { get; set; }
    }

    public class TaskAssistantClosingMessage
    {
    }

    public class SheetTaskCompletedMessage
    {
        public bool Completed { get; set; }
        public string Message { get; set; }
    }
}
