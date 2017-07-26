﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.Core.Utilities;

namespace HOK.BetaToolsManager
{
    public class AddinInstallerViewModel : ViewModelBase
    {
        public AddinInstallerModel Model;
        public RelayCommand<Window> CloseCommand { get; }
        public RelayCommand CheckAll { get; }
        public RelayCommand CheckNone { get; }
        public RelayCommand<Window> InstallCommand { get; }
        public RelayCommand<Window> UninstallCommand { get; }
        public RelayCommand<Window> WindowLoaded { get; }
        public RelayCommand<Window> WindowClosing { get; }

        public AddinInstallerViewModel(AddinInstallerModel model)
        {
            Model = model;
            Addins = Model.Addins;

            CloseCommand = new RelayCommand<Window>(OnCloseCommand);
            CheckAll = new RelayCommand(OnCheckAll);
            CheckNone = new RelayCommand(OnCheckNone);
            InstallCommand = new RelayCommand<Window>(OnInstall);
            UninstallCommand = new RelayCommand<Window>(OnUninstall);
            WindowLoaded = new RelayCommand<Window>(OnWindowLoaded);
            WindowClosing = new RelayCommand<Window>(OnWindowClosing);
        }

        private void OnWindowClosing(Window obj)
        {
            foreach (var addin in Addins)
            {
                if (AutoUpdateStatus != null) addin.AutoUpdate = (bool) AutoUpdateStatus;
            }
        }

        private void OnWindowLoaded(Window win)
        {
            OnCheckNone();
            AutoUpdateStatus = Addins.FirstOrDefault()?.AutoUpdate;
        }

        private void OnUninstall(Window win)
        {
            Model.UninstallAddins(Addins);
            win.Close();
        }

        private void OnInstall(Window win)
        {
            Model.InstallUpdateAddins(Addins);
            win.Close();
        }

        private void OnCheckNone()
        {
            foreach (var addin in Addins)
            {
                addin.IsSelected = false;
            }
        }

        private void OnCheckAll()
        {
            foreach (var addin in Addins)
            {
                addin.IsSelected = true;
            }
        }

        private static void OnCloseCommand(Window win)
        {
            win.Close();
        }

        private ObservableCollection<AddinWrapper> _addins = new ObservableCollection<AddinWrapper>();
        public ObservableCollection<AddinWrapper> Addins
        {
            get => _addins;
            set { _addins = value; RaisePropertyChanged(() => Addins); }
        }

        private bool? _autoUpdateStatus;
        public bool? AutoUpdateStatus
        {
            get => _autoUpdateStatus;
            set { _autoUpdateStatus = value; RaisePropertyChanged(() => AutoUpdateStatus); }
        }
    }
}
