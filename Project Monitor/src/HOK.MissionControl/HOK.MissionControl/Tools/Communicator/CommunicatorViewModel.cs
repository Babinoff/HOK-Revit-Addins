﻿using System.Collections.ObjectModel;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Tools.Communicator.HealthReport;
using HOK.MissionControl.Tools.Communicator.Tasks;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.Communicator
{
    public class CommunicatorViewModel : ViewModelBase
    {
        public ObservableCollection<TabItem> TabItems { get; set; }

        public CommunicatorViewModel()
        {
            FamilyStat familyStats = null;

            //TODO: What happens when multiple MissionControl tracked models are open in the same session? Only one HrData/SheetsData is stored.
            var familyStatsId = AppCommand.HrData.familyStats;
            var sheetsData = AppCommand.SheetsData;
            if (familyStatsId == null && sheetsData == null) return;

            if (familyStatsId != null)
            {
                familyStats = ServerUtilities.FindOne<FamilyStat>("families/" + familyStatsId);
            }

            TabItems = new ObservableCollection<TabItem>();

            if (familyStats != null)
            {
                var reportTab = new TabItem
                {
                    Content =
                        new CommunicatorHealthReportView { DataContext = new CommunicatorHealthReportViewModel(familyStats) },
                    Header = "Health Report"
                };
                TabItems.Add(reportTab);

                var taskTab = new TabItem
                {
                    Content = new CommunicatorTasksView
                    {
                        DataContext = new CommunicatorTasksViewModel(new CommunicatorTasksModel(familyStats, sheetsData))
                    },
                    Header = "Tasks"
                };
                TabItems.Add(taskTab);


            }
            else if (sheetsData != null)
            {
                var taskTab = new TabItem
                {
                    Content = new CommunicatorTasksView
                    {
                        DataContext = new CommunicatorTasksViewModel(new CommunicatorTasksModel(null, sheetsData))
                    },
                    Header = "Tasks"
                };
                TabItems.Add(taskTab);
            }
        }
    }
}
