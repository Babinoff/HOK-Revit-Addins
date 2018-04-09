﻿using System;
using System.Windows.Interop;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.LinksManager
{
    [Name(nameof(Properties.Resources.LinksManager_Name), typeof(Properties.Resources))]
    [Description(nameof(Properties.Resources.LinksManager_Description), typeof(Properties.Resources))]
    [Image(nameof(Properties.Resources.LInksManager_ImageName), typeof(Properties.Resources))]
    [PanelName(nameof(Properties.Resources.LinksManager_PanelName), typeof(Properties.Resources))]
    [ButtonText(nameof(Properties.Resources.LinksManager_ButtonText), typeof(Properties.Resources))]
    [Namespace(nameof(Properties.Resources.LinksManager_Namespace), typeof(Properties.Resources))]
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class LinksManagerCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var doc = uiApp.ActiveUIDocument.Document;
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                var unused1 = AddinUtilities.PublishAddinLog(
                    new AddinLog("MissionControl-LinksManager", commandData.Application.Application.VersionNumber), LogPosted);

                var viewModel = new LinksManagerViewModel(doc);
                var view = new LinksManagerView
                {
                    DataContext = viewModel
                };

                var unused = new WindowInteropHelper(view)
                {
                    Owner = Process.GetCurrentProcess().MainWindowHandle
                };

                view.ShowDialog();
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }

        /// <summary>
        /// Callback method for when Addin-info is published.
        /// </summary>
        /// <param name="data"></param>
        private static void LogPosted(AddinData data)
        {
            Log.AppendLog(LogMessageType.INFO, "Addin info was published: " 
                + (string.IsNullOrEmpty(data.Id) ? "Unsuccessfully." : "Successfully."));
        }
    }
}
