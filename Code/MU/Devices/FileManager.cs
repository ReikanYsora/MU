using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MU.Devices
{
    public static class FileManager
    {
        public static async Task<StorageFile> GetFileAsync(string fileName)
        {
            StorageFile fileResult = null;

            try
            {
                var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                var folders = await folder.GetFoldersAsync();

                foreach (StorageFolder sf in folders)
                {
                    var folders2 = await sf.GetFoldersAsync();

                    foreach (StorageFolder sf2 in folders2)
                    {
                        if (sf2.Name == "HTML")
                        {
                            var f = await sf2.GetFilesAsync();

                            foreach (StorageFile f1 in f)
                            {
                                string toto = f1.Name;
                                string titi = f1.Path;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

            return fileResult;
        }
    }
}
