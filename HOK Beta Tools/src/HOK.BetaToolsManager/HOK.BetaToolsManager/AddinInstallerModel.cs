﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using HOK.Core.Utilities;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace HOK.BetaToolsManager
{
    public class AddinInstallerModel
    {
        public string VersionNumber { get; set; }
        //public string BetaDirectory { get; set; } = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\";
        public string BetaDirectory { get; set; } = @"C:\Users\konrad.sobon\Desktop\test_beta_location\";
        public string InstallDirectory { get; set; }
        //public string AddinDirectory { get; set; }
        public ObservableCollection<AddinWrapper> Addins { get; set; }

        public AddinInstallerModel(string version)
        {
            VersionNumber = version;
            BetaDirectory = BetaDirectory + VersionNumber + @"\"; // at HOK Group drive
            InstallDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                               + @"\Autodesk\Revit\Addins\"
                               + VersionNumber + @"\"; // user roaming location
            //AddinDirectory = Path.GetFullPath(Path.Combine(InstallDirectory, @"..\..\"));

            LoadAddins();
        }

        public void UninstallAddins(ObservableCollection<AddinWrapper> addins)
        {
            //foreach (var addin in addins)
            //{
            //    if(!addin.Install) continue;

            //    if (File.Exists(InstallDirectory + addin.HostDllName))
            //    {
            //        try
            //        {
            //            File.Delete(InstallDirectory + addin.HostDllName);
            //        }
            //        catch (Exception e)
            //        {
            //            // ignored
            //        }
            //    }

            //    // TODO: There are all of the reference files still in the install directory
            //    // TODO: Is there a way to delete them safely without affecting other plugins?

            //    if (File.Exists(AddinDirectory + addin.AddinName))
            //    {
            //        try
            //        {
            //            File.Delete(AddinDirectory + addin.AddinName);
            //        }
            //        catch (Exception e)
            //        {
            //            // ignored
            //        }
            //    }

            //    // (Konrad) Button needs to be disabled after DLLs were removed since it doesn't work anyways.
            //    var app = AppCommand.Instance.m_app;
            //    var panel = app.GetRibbonPanels("  HOK - Beta").FirstOrDefault(x => x.Name == addin.Panel);
            //    var button = panel?.GetItems().FirstOrDefault(x => x.ItemText == addin.ButtonText);
            //    if (button != null)
            //    {
            //        button.Enabled = false;
            //    }

            //    // Reset installed version flag to update datagrid control
            //    addin.InstalledVersion = "Not installed";
            //}
        }

        public void InstallUpdateAddins(ObservableCollection<AddinWrapper> addins)
        {
            //foreach (var addin in addins)
            //{
            //    if(!addin.Install) continue;

            //    // Copy host DLL
            //    if (File.Exists(InstallDirectory + addin.HostDllName))
            //    {
            //        File.Delete(InstallDirectory + addin.HostDllName);
            //    }
            //    File.Copy(BetaDirectory + addin.HostDllName, InstallDirectory + addin.HostDllName);

            //    // Copy all references
            //    foreach (var resource in addin.ReferencedAssembliesNames)
            //    {
            //        if (File.Exists(InstallDirectory + resource))
            //        {
            //            File.Delete(InstallDirectory + resource);
            //        }
            //        if (File.Exists(BetaDirectory + resource))
            //        {
            //            File.Copy(BetaDirectory + resource, InstallDirectory + resource);
            //        }
            //    }

            //    // Copy addin manifest
            //    if (File.Exists(AddinDirectory + addin.AddinName))
            //    {
            //        File.Delete(AddinDirectory + addin.AddinName);
            //    }
            //    File.Copy(BetaDirectory + addin.AddinName, AddinDirectory + addin.AddinName);

            //    // Reset installed version flag to update datagrid control
            //    addin.InstalledVersion = addin.Version;
            //}
        }

        private static string ParseXml(string file)
        {
            var value = string.Empty;
            var response = File.ReadAllText(file);
            var doc = XDocument.Parse(response);

            foreach (var element in doc.Descendants("Assembly"))
            {
                value = (string)element;
                break;
            }
            return value;
        }

        private void LoadAddins()
        {
            var dic = new Dictionary<string, AddinWrapper>();
            if (Directory.Exists(BetaDirectory))
            {
                foreach (var file in Directory.GetFiles(BetaDirectory, "*.addin"))
                {
                    var dllRelativePath = ParseXml(file);
                    var dllPath = BetaDirectory + dllRelativePath;

                    // (Konrad) Using LoadFrom() instead of LoadFile() because
                    // LoadFile() doesn't load dependent assemblies causing exception later.
                    var assembly = Assembly.LoadFrom(dllPath);
                    Type[] types;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        types = e.Types;
                    }
                    foreach (var t in types.Where(x => x != null && (x.GetInterface("IExternalCommand") != null || x.GetInterface("IExternalApplication") != null)))
                    {
                        MemberInfo info = t;
                        var nameAttr = (NameAttribute)info.GetCustomAttributes(typeof(NameAttribute), true).FirstOrDefault();
                        var descAttr = (DescriptionAttribute)t.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
                        var imageAttr = (ImageAttribute)t.GetCustomAttributes(typeof(ImageAttribute), true).FirstOrDefault();
                        var namespaceAttr = (NamespaceAttribute)t.GetCustomAttributes(typeof(NamespaceAttribute), true).FirstOrDefault();

                        if (nameAttr == null || descAttr == null || imageAttr == null || namespaceAttr == null) continue;

                        var bitmap = (BitmapSource)ButtonUtil.LoadBitmapImage(assembly, namespaceAttr.Namespace, imageAttr.ImageName);
                        var version = assembly.GetName().Version.ToString();
                        //var referencedAssemblies = GetNamesOfAssembliesReferencedBy(assembly);

                        var installedVersion = "Not installed";
                        if (File.Exists(InstallDirectory + Path.GetFileName(file)))
                        {
                            var a = Assembly.LoadFile(InstallDirectory + dllRelativePath);
                            installedVersion = a.GetName().Version.ToString();
                        }

                        var aw = new AddinWrapper
                        {
                            Name = nameAttr.Name,
                            Description = descAttr.Description,
                            Image = bitmap,
                            ImageName = imageAttr.ImageName,
                            FullName = t.FullName,
                            Version = version,
                            HostDllName = Path.GetFileName(file),
                            InstalledVersion = installedVersion
                        };

                        if (t.GetInterface("IExternalCommand") != null)
                        {
                            var buttonTextAttr = (ButtonTextAttribute)t.GetCustomAttributes(typeof(ButtonTextAttribute), true).FirstOrDefault();
                            var panelNameAttr = (PanelNameAttribute)t.GetCustomAttributes(typeof(PanelNameAttribute), true).FirstOrDefault();

                            aw.Panel = panelNameAttr?.PanelName;
                            aw.ButtonText = buttonTextAttr?.ButtonText;
                            aw.ExternalCommand = true;
                        }

                        dic.Add(aw.Name, aw);
                    }
                }
            }
            var output = new ObservableCollection<AddinWrapper>(dic.Values.ToList().OrderBy(x => x.Name));
            Addins = output;
        }

        public List<string> GetNamesOfAssembliesReferencedBy(Assembly assembly)
        {
            return assembly.GetReferencedAssemblies()
                .Select(assemblyName => assemblyName.Name + ".dll").ToList();
        }
    }

    public static class Serialization
    {
        /// <summary>
        /// Deserializes JSON string into Collection of AddinWrappers.
        /// </summary>
        /// <param name="filePath">Path to file.</param>
        /// <returns></returns>
        public static ObservableCollection<AddinWrapper> Deserialize(string filePath)
        {
            var jsonString = File.ReadAllText(filePath);
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            var deserialized = JsonConvert.DeserializeObject<ObservableCollection<AddinWrapper>>(jsonString, settings);

            return deserialized;
        }

        /// <summary>
        /// Serializes Addins selections for future use.
        /// </summary>
        /// <param name="filePath">Path to file.</param>
        /// <param name="addins">Collection of AddinWrappers to be serialized.</param>
        /// <returns></returns>
        public static string Serialize(string filePath, ObservableCollection<AddinWrapper> addins)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            var jsonString = JsonConvert.SerializeObject(addins, Formatting.Indented, settings);
            File.WriteAllText(filePath, jsonString);

            return new FileInfo(filePath).Length > 0 ? filePath : string.Empty;
        }
    }
}
