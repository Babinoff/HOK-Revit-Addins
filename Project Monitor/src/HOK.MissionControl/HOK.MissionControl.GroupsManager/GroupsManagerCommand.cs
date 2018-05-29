﻿using System;
using System.Diagnostics;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.GroupsManager
{
    /// <summary>
    /// Class attributes are used for beta tools management.
    /// </summary>
    [Name(nameof(Properties.Resources.GroupsManager_Name), typeof(Properties.Resources))]
    [Description(nameof(Properties.Resources.GroupsManager_Description), typeof(Properties.Resources))]
    [Image(nameof(Properties.Resources.GroupsManager_ImageName), typeof(Properties.Resources))]
    [PanelName(nameof(Properties.Resources.GroupsManager_PanelName), typeof(Properties.Resources))]
    [ButtonText(nameof(Properties.Resources.GroupsManager_ButtonText), typeof(Properties.Resources))]
    [Namespace(nameof(Properties.Resources.GroupsManager_Namespace), typeof(Properties.Resources))]
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class GroupsManagerCommand : IExternalCommand
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
                    new AddinLog("MissionControl-GroupsManager", commandData.Application.Application.VersionNumber), LogPosted);

                var model = new GroupsManagerModel(doc);
                var viewModel = new GroupsManagerViewModel(model);
                var view = new GroupsManagerView
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