﻿using System.Reflection;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows;

namespace HOK.MissionControl.Tools.HealthReport.ObjectTrackers
{
    public class FamilyMonitorViewModel : ViewModelBase
    {
        public RelayCommand<Window> ButtonSureCommand { get; }
        public RelayCommand<Window> ButtonNextTimeCommand { get; }
        public string Title { get; set; }

        public FamilyMonitorViewModel()
        {
            ButtonSureCommand = new RelayCommand<Window>(OnButtonSure);
            ButtonNextTimeCommand = new RelayCommand<Window>(OnButtonNextTime);
            Title = "DTM Tool v." + Assembly.GetExecutingAssembly().GetName().Version;
        }

        private static void OnButtonNextTime(Window window)
        {
            window.DialogResult = false;
            window.Close();
        }

        private static void OnButtonSure(Window window)
        {
            window.DialogResult = true;
            window.Close();
        }
    }
}
