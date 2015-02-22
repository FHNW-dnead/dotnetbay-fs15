using System.IO;

using Newtonsoft.Json;

namespace DotNetBay.Data.FileStorage
{
    public class FileSystemMainRepository : InMemoryMainRepository
    {
        // Poor-Mans locking mechanism
        private readonly string fullPath;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly string rootDirectory;

        private readonly string binaryDataDirectory;

        public FileSystemMainRepository(string fileName)
        {
            // It's good practice to expect either absolute or relative paths and handle both the same
            this.fullPath = Path.GetFullPath(fileName);
            this.rootDirectory = Path.GetDirectoryName(this.fullPath);
            this.binaryDataDirectory = Path.Combine(this.rootDirectory, "data");

            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
        }

        #region Persistence

        internal override DataRootElement LoadData()
        {
            if (!File.Exists(this.fullPath))
            {
                var file = File.Create(this.fullPath);
                file.Close();
            }

            var content = File.ReadAllText(this.fullPath);

            var restored = JsonConvert.DeserializeObject<DataRootElement>(content, this.jsonSerializerSettings);
            return restored;
        }

        internal override void SaveData(DataRootElement data)
        {
            var content = JsonConvert.SerializeObject(data, this.jsonSerializerSettings);
            File.WriteAllText(this.fullPath, content);
        }

        #endregion

        #region Before / After Save Hooks

        internal override void AfterLoad(DataRootElement data)
        {
            // Reload Images from FS
            foreach (var auction in data.Auctions)
            {
                auction.Image = this.LoadBinary(string.Format("auction-{0}-image1.jpg", auction.Id));
            }
        }

        internal override void BeforeLoad(DataRootElement data)
        {
            // Ensure existence of directory
            Directory.CreateDirectory(this.binaryDataDirectory);
        }

        internal override void AfterSave(DataRootElement data)
        {
            // Reload Images from FS
            foreach (var auction in data.Auctions)
            {
                auction.Image = this.LoadBinary(string.Format("auction-{0}-image1.jpg", auction.Id));
            }
        }

        internal override void BeforeSave(DataRootElement data)
        {
            // Ensure existence of directory
            Directory.CreateDirectory(this.binaryDataDirectory);

            // Remove byte values from images and save individually
            foreach (var auction in data.Auctions)
            {
                this.SaveBinary(string.Format("auction-{0}-image1.jpg", auction.Id), auction.Image);
                auction.Image = null;
            }
        }

        #endregion

        #region Binary-Fields (Save/Loading)

        private void SaveBinary(string fileName, byte[] fileContent)
        {
            var fullFileName = Path.Combine(this.binaryDataDirectory, fileName);

            if (fileContent == null)
            {
                try
                {
                    File.Delete(fullFileName);
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                File.WriteAllBytes(fullFileName, fileContent);
            }
        }

        private byte[] LoadBinary(string fileName)
        {
            var fullFileName = Path.Combine(this.binaryDataDirectory, fileName);

            if (File.Exists(fullFileName))
            {
                return File.ReadAllBytes(fullFileName);
            }

            return null;
        }

        #endregion
    }
}