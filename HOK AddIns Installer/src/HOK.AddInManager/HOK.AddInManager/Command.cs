﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using HOK.AddInManager.Classes;
using HOK.AddInManager.UserControls;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.AddInManager
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class Command : IExternalCommand
    {
        private readonly Dictionary<string/*toolName*/, LoadType> tempSettings = new Dictionary<string, LoadType>();

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(new AddinLog("AddinManager", commandData.Application.Application.VersionNumber));

                var addins = AppCommand.thisApp.addins;
                StoreTempCollection(addins.AddinCollection);
                var viewModel = new AddInViewModel(addins);
                var mainWindow = new MainWindow {DataContext = viewModel};
                if (true == mainWindow.ShowDialog())
                {
                    //write setting
                    AppCommand.thisApp.addins = mainWindow.ViewModel.AddinsObj;

                    //load addins
                    AppCommand.thisApp.RemoveToolsBySettings();
                    AppCommand.thisApp.AddToolsBySettings();
                    AppCommand.thisApp.LoadToolsBySettings();
                }
                else
                {
                    OverrideTempSettings();
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }

        private void StoreTempCollection(ObservableCollection<AddinInfo> origCollection)
        {
            try
            {
                tempSettings.Clear();
                foreach (var addin in origCollection)
                {
                    tempSettings.Add(addin.ToolName, addin.ToolLoadType);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void OverrideTempSettings()
        {
            try
            {
                foreach (var addinName in tempSettings.Keys)
                {
                    var addinFound = AppCommand.thisApp.addins.AddinCollection
                        .Where(x => x.ToolName == addinName)
                        .ToList();
                    if (!addinFound.Any()) continue;

                    var index = AppCommand.thisApp.addins.AddinCollection.IndexOf(addinFound.First());
                    AppCommand.thisApp.addins.AddinCollection[index].ToolLoadType = tempSettings[addinName];
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}
