﻿using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace HOK.Core.Utilities
{
    public static class ButtonUtil
    {
        /// <summary>
        /// Retrieves the BitmapImage from given assembly by its name and namespace. 
        /// Image must live in Resources folder.
        /// </summary>
        /// <param name="assembly">Assembly object to retrieve image from.</param>
        /// <param name="nameSpace">Namespace.</param>
        /// <param name="imageName">Image name with its extension.</param>
        /// <returns></returns>
        public static BitmapImage LoadBitmapImage(Assembly assembly, string nameSpace, string imageName)
        {
            var image = new BitmapImage();
            try
            {
                var prefix = nameSpace + ".Resources.";
                var stream = assembly.GetManifestResourceStream(prefix + imageName);

                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch (Exception ex)
            {
                Log.AppendLog("HOK.Core.ButtonUtilities.LoadBitmapImage: " + ex.Message);
            }
            return image;
        }
    }
}
