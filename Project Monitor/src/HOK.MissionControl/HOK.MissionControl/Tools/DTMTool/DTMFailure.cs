﻿using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using HOK.Core;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Utils;

namespace HOK.MissionControl.Tools.DTMTool
{
    public static class DTMFailure
    {
        public static bool IsElementModified { get; set; }
        public static ReportingElementInfo ElementModified { get; set; }
        public static Document CurrentDoc { get; set; }

        public static void ProcessFailure(object sender, FailuresProcessingEventArgs args)
        {
            try
            {
                if (!IsElementModified || CurrentDoc == null) return;

                FailureProcessor.IsFailureProcessing = true;
                var fa = args.GetFailuresAccessor();

                var dtmViewModel = new DTMViewModel(CurrentDoc, ElementModified);
                var dtmWindow = new DTMWindow { DataContext = dtmViewModel };
                var showDialog = dtmWindow.ShowDialog();
                if (showDialog != null && (bool)showDialog)
                {
                    args.SetProcessingResult(FailureProcessingResult.ProceedWithRollBack);
                    var option = fa.GetFailureHandlingOptions();
                    option.SetClearAfterRollback(true);
                    fa.SetFailureHandlingOptions(option);
                }
                IsElementModified = false;
                FailureProcessor.IsFailureProcessing = false;
            }
            catch (Exception ex)
            {
                LogUtilities.AppendLog("DTMFailure-ProcessFailure:" + ex.Message);
            }
        }
    }
}
