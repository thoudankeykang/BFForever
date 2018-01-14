﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BFForever.Riff
{
    // Package Manager?
    public class FEnvironment
    {
        private Dictionary<HKey, string> _packagePaths;
        private List<ZObject> _tempObjects;
        private Index2PackageEntry _tempObjectsPackageEntry;
        private readonly List<ZObject> _pendingChanges;

        public FEnvironment()
        {
            _packagePaths = new Dictionary<HKey, string>();
            _tempObjects = new List<ZObject>();
            _pendingChanges = new List<ZObject>();
        }
        
        public ZObject this[HKey index] => GetZObject(Index?.Entries.SingleOrDefault(x => x.FilePath == index));

        public void LoadPackage(string rootPath)
        {
            string fullRootPath = Path.GetFullPath(rootPath);
            string defDirectory = Path.Combine(rootPath, "packagedefs");

            // Loads PackageDef
            if (!Directory.Exists(fullRootPath) || !Directory.Exists(defDirectory)) return;
            
            string[] packageRifs = Directory.GetFiles(defDirectory, "*.rif", SearchOption.AllDirectories);
            if (packageRifs.Length <= 0) return;

            foreach (string packageRif in packageRifs)
            {
                RiffFile rif = RiffFile.FromFile(packageRif);
                PackageDef newPackage = rif.Objects.FirstOrDefault(x => x is PackageDef) as PackageDef;
                if (newPackage == null) continue;

                // Updates package path
                if (_packagePaths.ContainsKey(newPackage.FilePath))
                    _packagePaths.Remove(newPackage.FilePath);
                _packagePaths.Add(newPackage.FilePath, fullRootPath);

                // Updates packagedef object
                PackageDef oldPackage = Definition;
                if (oldPackage == null || newPackage.Version >= oldPackage.Version)
                    Definition = newPackage;
            }

            ReloadIndex(fullRootPath);
        }

        private ZObject GetZObject(Index2Entry entry)
        {
            if (entry == null || !entry.IsZObject()) return null;
            
            foreach(Index2PackageEntry pkEntry in entry.PackageEntries)
            {
                if (!_packagePaths.ContainsKey(pkEntry.Package)) continue;
                string filePath = Path.Combine(_packagePaths[pkEntry.Package], pkEntry.ExternalFilePath);

                // Checks if cached
                if (_tempObjectsPackageEntry != null
                    && _tempObjectsPackageEntry.ExternalFilePath == pkEntry.ExternalFilePath
                    && _tempObjectsPackageEntry.Package == pkEntry.Package)
                {
                    return _tempObjects.SingleOrDefault(x => x.FilePath == entry.FilePath);
                }

                // Checks if file exists
                if (!File.Exists(filePath)) continue;
                if (LoadRiffFile(filePath, pkEntry))
                    return _tempObjects.SingleOrDefault(x => x.FilePath == entry.FilePath);
            }
            
            return null;
        }

        private bool LoadRiffFile(string path, Index2PackageEntry packageEntry)
        {
            RiffFile rif = RiffFile.FromFile(path);
            if (rif == null) return false;

            _tempObjects.Clear();
            _tempObjectsPackageEntry = packageEntry;

            // Loads all zobjects from riff file
            foreach (ZObject obj in rif.Objects.Where(x => !(x is StringTable)))
            {
                _tempObjects.Add(obj);
            }

            return true;
        }

        private void ReloadIndex(string rootPath)
        {
            string indexPath = Path.Combine(rootPath, "index2.rif");
            if (Definition == null || !File.Exists(indexPath)) return;

            RiffFile rif = RiffFile.FromFile(indexPath);
            Index2 newIndex = rif.Objects.FirstOrDefault(x => x is Index2) as Index2;
            if (newIndex == null) return;

            // Updates index2 object
            Index2 oldIndex = Index;

            if (oldIndex == null || newIndex.Version >= oldIndex.Version)
            {
                Index = newIndex;

                // Creates directory paths for entries
                foreach (Index2Entry entry in Index.Entries)
                    entry.FilePath.GetParentDirectory();
            }
        }

        public void AddZObjectsAsPending(List<ZObject> objects)
        {
            _pendingChanges.AddRange(objects);
        }

        public void SavePendingChanges()
        {

            _pendingChanges.Clear();
        }

        public void UpdateIndexEntryAsPending(HKey filePath, HKey type, string physicalPath, HKey packageFilePath)
        {
        }

        public void ClearCache()
        {
            _tempObjects.Clear();
            _tempObjectsPackageEntry = null;
        }

        public PackageDef Definition { get; set; }
        public Index2 Index { get; set; }
        
        public static Localization Localization { get; set; } = Localization.English;
    }
}