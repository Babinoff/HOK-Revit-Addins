﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace HOK.BetaToolsManager
{
    public class ToolManager
    {
        private string versionNumber = "";
        private string betaDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\";
        private string installDirectory = "";
        private string tempInstallDirectory = "";
        private Dictionary<ToolEnum, ToolProperties> toolInfoDictionary = new Dictionary<ToolEnum, ToolProperties>();

        public string VersionNumber { get { return versionNumber; } set { versionNumber = value; } }
        public string BetaDirectroy { get { return betaDirectory; } set { betaDirectory = value; } }
        public string InstallDirectory { get { return installDirectory; } set { installDirectory = value; } }
        public string TempInstallDirectory { get { return tempInstallDirectory; } set { tempInstallDirectory = value; } }
        public Dictionary<ToolEnum, ToolProperties> ToolInfoDictionary { get { return toolInfoDictionary; } set { toolInfoDictionary = value; } }

        public ToolManager(string version)
        {
            versionNumber = version;
            betaDirectory = betaDirectory + versionNumber + @"\HOK-Addin.bundle\Contents_Beta\";
            installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\" + versionNumber + @"\HOK-Addin.bundle\Contents_Beta\";
            tempInstallDirectory = Path.Combine(installDirectory, "Temp");

            if (ExistBetaContentFolder())
            {
                toolInfoDictionary = GetToolInfo();
            }
        }

        private bool ExistBetaContentFolder()
        {
            bool exist = false;
            try
            {
                if (!Directory.Exists(installDirectory))
                {
                    Directory.CreateDirectory(installDirectory);
                }
                if (Directory.Exists(tempInstallDirectory))
                {
                    //delete temp directory first
                    Directory.Delete(tempInstallDirectory, true);
                }

                string resourceFolder = Path.Combine(installDirectory, "Resources");
                if (Directory.Exists(installDirectory) && !Directory.Exists(resourceFolder))
                {
                    Directory.CreateDirectory(resourceFolder);
                }
                if (Directory.Exists(installDirectory) && !Directory.Exists(tempInstallDirectory))
                {
                    //copy files from install directory to temp directory.
                    string[] installedFiles = Directory.GetFiles(installDirectory);

                    Directory.CreateDirectory(tempInstallDirectory);
                    if (Directory.Exists(tempInstallDirectory))
                    {
                        foreach (string filePath in installedFiles)
                        {
                            string fileName = Path.GetFileName(filePath);
                            string tempFile = Path.Combine(tempInstallDirectory, fileName);
                            try { File.Copy(filePath, tempFile, true); }
                            catch { }
                        }
                    }

                    string tempResourceFolder = Path.Combine(tempInstallDirectory, "Resources");
                    if (!Directory.Exists(tempResourceFolder))
                    {
                        Directory.CreateDirectory(tempResourceFolder);
                    }
                    if (Directory.Exists(tempResourceFolder))
                    {
                        string[] installedResources = Directory.GetFiles(resourceFolder);
                        foreach (string filePath in installedResources)
                        {
                            string fileName = Path.GetFileName(filePath);
                            string tempFile = Path.Combine(tempResourceFolder, fileName);
                            try { File.Copy(filePath, tempFile, true); }
                            catch { }
                        }
                    }
                }
                
                if (Directory.Exists(resourceFolder))
                {
                    exist = true;  
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Beta directories cannot be created.\n" + ex.Message,"Beta Folder Exist", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            return exist;
        }

        private Dictionary<ToolEnum, ToolProperties> GetToolInfo()
        {
            Dictionary<ToolEnum, ToolProperties> dictionary = new Dictionary<ToolEnum, ToolProperties>();
            try
            {
                Array toolArray = Enum.GetValues(typeof(ToolEnum));
                
                foreach (ToolEnum tool in toolArray)
                {
                    ToolProperties tp = new ToolProperties();
                    tp.ToolEnumVal = tool;
                    tp.InstallingFiles = GetFiles(tool);

                    switch (tool)
                    {
                        case ToolEnum.ElementTools:
                            tp.ToolName = "Element Tools";
                            tp.DllName = "HOK.ElementTools.dll";
                            tp.BetaPath = betaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = installDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("element.ico");
                            
                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.ParameterTools:
                            tp.ToolName = "Parameter Tools";
                            tp.DllName = "HOK.ParameterTools.dll";
                            tp.BetaPath = betaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = installDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("parameter.ico");

                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.SheetManager:
                            tp.ToolName = "Sheet Manager";
                            tp.DllName = "HOK.SheetManager.dll";
                            tp.BetaPath =  betaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = installDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("sheet.ico");

                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.BCFReader:
                            tp.ToolName = "BCF Reader";
                            tp.DllName = "HOK.BCFReader.dll";
                            tp.BetaPath = betaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = installDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("comment.ico");

                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.MassTool:
                            tp.ToolName = "Mass Tools";
                            tp.DllName = "HOK.RoomsToMass.dll";
                            tp.BetaPath = betaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = installDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("cube.png");

                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.RevitData:
                            tp.ToolName = "Data Manager";
                            tp.DllName = "HOK.RevitDBManager.dll";
                            tp.BetaPath = betaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = installDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("editor.ico");

                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.AVF:
                            tp.ToolName = "Analysis Tools";
                            tp.DllName = "HOK.AVFManager.dll";
                            tp.BetaPath = betaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = installDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("chart.ico");

                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.LPDAnalysis:
                            tp.ToolName = "LPD Analysis";
                            tp.DllName = "HOK.LPDCalculator.dll";
                            tp.BetaPath = betaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = installDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("bulb.png");

                            dictionary.Add(tool, tp);
                            break;
#if RELEASE2015||RELEASE2016
                        case ToolEnum.LEEDView:
                            tp.ToolName = "LEED View Analysis";
                            tp.DllName = "HOK.ViewAnalysis.dll";
                            tp.BetaPath = betaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = installDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("eq.ico");

                            dictionary.Add(tool, tp);
                            break;

                        case ToolEnum.ElementFlatter:
                            tp.ToolName = "Element Flatter";
                            tp.DllName = "HOK.ElementFlatter.dll";
                            tp.BetaPath = betaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = installDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("create.ico");

                            dictionary.Add(tool, tp);
                            break;
#endif
                        case ToolEnum.Utility:
                            tp.ToolName = "Utility Tools";
                            tp.DllName = "HOK.Utilities.dll";
                            tp.BetaPath = betaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = installDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("height.png");

                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.ColorEditor:
                            tp.ToolName = "Color Editor";
                            tp.DllName = "HOK.ColorSchemeEditor.dll";
                            tp.BetaPath = betaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = installDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("color32.png");
                            dictionary.Add(tool, tp);
                            break;
                       
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get information about beta tools.\n" + ex.Message, "Get Tool Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return dictionary;
        }

        private string GetTempInstallPath(string installPath)
        {
            string tempPath="";
            try
            {
                string fileName = Path.GetFileName(installPath);
                tempPath = Path.Combine(tempInstallDirectory, fileName);
                if (!File.Exists(tempPath))
                {
                    tempPath = installPath;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return tempPath;
        }

        private List<string> GetFiles(ToolEnum toolName)
        {
            List<string> fileNames = new List<string>();
            try
            {
                switch (toolName)
                {
                    case ToolEnum.ElementTools:
                        fileNames.Add("HOK.ElementTools.dll");
                        break;

                    case ToolEnum.ParameterTools:
                        fileNames.Add("HOK.ParameterTools.dll");
                        break;

                    case ToolEnum.SheetManager:
                        fileNames.Add("HOK.SheetManager.dll");
                        break;

                    case ToolEnum.BCFReader:
                        fileNames.Add("HOK.BCFReader.dll");
                        fileNames.Add("ICSharpCode.SharpZipLib.dll");
                        fileNames.Add("Resources\\markup.xsd");
                        fileNames.Add("Resources\\visinfo.xsd");
                        break;

                    case ToolEnum.MassTool:
                        fileNames.Add("HOK.RoomsToMass.dll");
#if RELEASE2013||RELEASE2014
                        fileNames.Add("Resources\\Mass Shared Parameters.txt");
                        fileNames.Add("Resources\\Mass.rfa");
#endif
                        break;

                    case ToolEnum.RevitData:
                        fileNames.Add("HOK.RevitDBManager.dll");
                        fileNames.Add("Resources\\default.bmp");
                        fileNames.Add("Resources\\eye.ico");
                        break;

                    case ToolEnum.AVF:
                        fileNames.Add("HOK.AVFManager.dll");
                        fileNames.Add("Resources\\DefaultSettings.xml");
                        fileNames.Add("Resources\\PointOfView.rfa");
                        break;

                    case ToolEnum.LPDAnalysis:
                        fileNames.Add("HOK.LPDCalculator.dll");
                        break;

                    case ToolEnum.LEEDView:
                        fileNames.Add("HOK.ViewAnalysis.dll");
                        fileNames.Add("Resources\\Addins Shared Parameters.txt");
                        break;

                    case ToolEnum.ElementFlatter:
                        fileNames.Add("HOK.ElementFlatter.dll");
                        break;
                    case ToolEnum.Utility:
                        fileNames.Add("HOK.Utilities.dll");
                        fileNames.Add("HOK.Arrowhead.dll");
                        fileNames.Add("HOK.CeilingHeight.dll");
                        fileNames.Add("HOK.DoorRoom.dll");
                        fileNames.Add("HOK.FinishCreator.dll");
                        fileNames.Add("HOK.LevelManager.dll");
                        fileNames.Add("HOK.RoomElevation.dll");
                        fileNames.Add("HOK.RoomUpdater.dll");
                        fileNames.Add("HOK.ViewDepth.dll");
                        fileNames.Add("HOK.WorksetView.dll");
                        fileNames.Add("HOK.CameraDuplicator.dll");
                        fileNames.Add("HOK.RenameFamily.dll");
                        break;
                    case ToolEnum.ColorEditor:
                        fileNames.Add("HOK.ColorSchemeEditor.dll");
                        fileNames.Add("ICSharpCode.SharpZipLib.dll");
                        fileNames.Add("Xceed.Wpf.Toolkit.dll");
                        break;
                    case ToolEnum.SmartBCF:
                        break;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return fileNames;
        }

    }

    public class ToolProperties
    {
        private ToolEnum toolEnumVal = ToolEnum.None;
        private string toolName = "";
        private string dllName = "";
        //private string assemblyName = "";
        private string installPath = "";
        private string betaPath = "";
        private string tempAssemblyPath = "";
        private FileVersionInfo betaVersionInfo = null;
        private FileVersionInfo installedVersionInfo = null;
        private string betaVersionNumber = "Not Exist";
        private string betaReleasedDate = "";
        private string installVersionNumber = "Not Installed";
        private bool betaExist = false;
        private bool installExist = false;
        private bool isEnabled = false;
        private bool isSelected = false;
        private BitmapImage toolIcon = null;
        private List<string> installingFiles = new List<string>();

        public ToolEnum ToolEnumVal { get { return toolEnumVal; } set { toolEnumVal = value; } }
        public string ToolName { get { return toolName; } set { toolName = value; } }
        public string DllName { get { return dllName; } set { dllName = value; } }
        //public string AssemblyName { get { return assemblyName; } set { assemblyName = value; } }
        public string InstallPath { get { return installPath; } set { installPath = value; } }
        public string BetaPath { get { return betaPath; } set { betaPath = value; } }
        public string TempAssemblyPath { get { return tempAssemblyPath; } set { tempAssemblyPath = value; } }
        public FileVersionInfo BetaVersionInfo { get { return betaVersionInfo; } set { betaVersionInfo = value; } }
        public FileVersionInfo InstalledVersionInfo { get { return installedVersionInfo; } set { installedVersionInfo = value; } }
        public string BetaVersionNumber { get { return betaVersionNumber; } set { betaVersionNumber = value; } }
        public string BetaReleasedDate { get { return betaReleasedDate; } set { betaReleasedDate = value; } }
        public string InstallVersionNumber { get { return installVersionNumber; } set { installVersionNumber = value; } }
        public bool BetaExist { get { return betaExist; } set { betaExist = value; } }
        public bool InstallExist { get { return installExist; } set { installExist = value; } }
        public bool IsEnabled { get { return isEnabled; } set { isEnabled = value; } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }
        public BitmapImage ToolIcon { get { return toolIcon; } set { toolIcon = value; } }
        public List<string> InstallingFiles { get { return installingFiles; } set { installingFiles = value; } }

        public ToolProperties()
        {
        }

        public ToolProperties(ToolProperties tp)
        {
            this.ToolEnumVal = tp.ToolEnumVal;
            this.ToolName = tp.ToolName;
            this.DllName = tp.DllName;
            this.InstallPath = tp.InstallPath;
            this.BetaPath = tp.BetaPath;
            this.BetaVersionInfo = tp.BetaVersionInfo;
            this.InstalledVersionInfo = tp.InstalledVersionInfo;
            this.BetaVersionNumber = tp.BetaVersionNumber;
            this.BetaReleasedDate = tp.BetaReleasedDate;
            this.InstallVersionNumber = tp.InstallVersionNumber;
            this.BetaExist = tp.BetaExist;
            this.InstallExist = tp.InstallExist;
            this.IsEnabled = tp.IsEnabled;
            this.IsSelected = tp.IsSelected;
            this.ToolIcon = tp.ToolIcon;
            this.InstallingFiles = tp.InstallingFiles;
        }
    }

    public static class ImageUtil
    {
        public static BitmapImage LoadBitmapImage(string imageName)
        {
            BitmapImage image = new BitmapImage();
            try
            {
                string prefix = typeof(AppCommand).Namespace + ".Resources.";
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(prefix + imageName);

                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load the embedded resource image.\n" + ex.Message, "Load Bitmap Image", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return image;
        }
    }
}
