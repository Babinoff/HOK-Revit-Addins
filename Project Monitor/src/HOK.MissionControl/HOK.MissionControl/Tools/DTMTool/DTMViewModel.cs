﻿using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using Autodesk.Revit.DB;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.DTMTool
{
    public class DTMViewModel : ViewModelBase
    {
        private Document Doc { get; }
        private readonly ReportingElementInfo reportingInfo;
        public string DescriptionText { get; set; }
        public string Title { get; set; }

        public RelayCommand ButtonPlayCommand { get; }
        public RelayCommand<Window> ButtonIgnoreCommand { get; }
        public RelayCommand<Window> ButtonEditCommand { get; }

        public DTMViewModel(Document doc, ReportingElementInfo info)
        {
            Doc = doc;
            reportingInfo = info;
            Title = "DTM Tool v." + Assembly.GetExecutingAssembly().GetName().Version;

            if (!string.IsNullOrEmpty(info.Description)) DescriptionText = info.Description;
            else DescriptionText = reportingInfo.CategoryName + " are locked.";

            ButtonPlayCommand = new RelayCommand(OnButtonPlay);
            ButtonIgnoreCommand = new RelayCommand<Window>(OnButtonIgnore);
            ButtonEditCommand = new RelayCommand<Window>(OnButtonEdit);
        }

        private void OnButtonEdit(Window window)
        {
            try
            {
                //update category trigger settings
                var settingsUpdated = false;
                var centralPath = reportingInfo.CentralPath;
                if (MissionControlSetup.Configurations.ContainsKey(centralPath))
                {
                    var config = MissionControlSetup.Configurations[centralPath];
                    var updaterFound = config.updaters
                        .FirstOrDefault(x => string.Equals(x.updaterId.ToLower(), reportingInfo.UpdaterId.ToLower(),
                            StringComparison.Ordinal));
                    if (updaterFound != null)
                    {
                        var updaterIndex = config.updaters.IndexOf(updaterFound);
                        var triggerFound = updaterFound.CategoryTriggers
                            .FirstOrDefault(x => x.categoryName == reportingInfo.CategoryName);
                        if (triggerFound != null)
                        {
                            var triggerIndex = updaterFound.CategoryTriggers.IndexOf(triggerFound);
                            config.updaters[updaterIndex].CategoryTriggers[triggerIndex].isEnabled = false;

                            MissionControlSetup.Configurations.Remove(centralPath);
                            MissionControlSetup.Configurations.Add(centralPath, config);

                            //refresh category trigger
                            AppCommand.Instance.DtmUpdaterInstance.Unregister(Doc);
                            AppCommand.Instance.DtmUpdaterInstance.Register(Doc, config.updaters[updaterIndex]);

                            settingsUpdated = true;
                        }
                    }
                }

                if (settingsUpdated)
                {
                    //database updated
                    var record = new TriggerRecord
                    {
                        configId = reportingInfo.ConfigId,
                        centralPath = reportingInfo.CentralPath,
                        updaterId = reportingInfo.UpdaterId,
                        categoryName = reportingInfo.CategoryName,
                        elementUniqueId = reportingInfo.ReportingUniqueId,
                        edited = DateTime.UtcNow,
                        editedBy = Environment.UserName
                    };

                    ServerUtilities.PostTriggerRecords(record);

                    window.DialogResult = false;
                }
                window.Close();

            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private static void OnButtonIgnore(Window window)
        {
            window.DialogResult = true;
            window.Close();
        }

        private static void OnButtonPlay()
        {
            try
            {
                System.Diagnostics.Process.Start("https://youtu.be/qyUnSuYYs18");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}
