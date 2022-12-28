using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;
using static System.Net.WebRequestMethods;

namespace XNATWL
{
    public class FileSystemObject
    {
        public enum FileSystemObjectType
        {
            FILE,
            DIRECTORY,
            ROOT
        }

        private FileSystemObjectType _type;
        private string _path;

        public bool IsDirectory
        {
            get
            {
                return _type == FileSystemObjectType.DIRECTORY || _type == FileSystemObjectType.ROOT;
            }
        }

        public bool IsFolder
        {
            get
            {
                return _type == FileSystemObjectType.DIRECTORY || _type == FileSystemObjectType.ROOT;
            }
        }

        public bool IsFile
        {
            get
            {
                return _type == FileSystemObjectType.FILE;
            }
        }

        public bool IsHidden
        {
            get
            {
                if (_type == FileSystemObjectType.FILE)
                {
                    return System.IO.File.GetAttributes(this.@_path).HasFlag(System.IO.FileAttributes.Hidden);
                }
                else if (_type == FileSystemObjectType.FILE)
                {
                    return new System.IO.DirectoryInfo(this._path).Attributes.HasFlag(System.IO.FileAttributes.Hidden);
                }
                else
                {
                    return false;
                }
            }
        }

        public FileSystemObject Parent
        {
            get
            {
                if (_type == FileSystemObjectType.FILE)
                {
                    string parentDirectoryPath = System.IO.Path.GetDirectoryName(this.@_path);
                    if (parentDirectoryPath == null)
                    {
                        return null;
                    }

                    var parentDirectory = new System.IO.DirectoryInfo(parentDirectoryPath);

                    if (parentDirectory.Parent == null)
                    {
                        return new FileSystemObject(FileSystemObjectType.ROOT, parentDirectoryPath);
                    }
                    else
                    {
                        return new FileSystemObject(FileSystemObjectType.DIRECTORY, parentDirectoryPath);
                    }
                }
                else if (_type == FileSystemObjectType.DIRECTORY)
                {
                    var directory = new System.IO.DirectoryInfo(this._path);

                    if (directory.Parent == null)
                    {
                        return null;
                    }
                    else
                    {
                        return new FileSystemObject(FileSystemObjectType.DIRECTORY, directory.Parent.FullName);
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public string Name
        {
            get
            {
                if (_type == FileSystemObjectType.FILE)
                {
                    return System.IO.Path.GetFileName(this._path);
                }
                else
                {
                    var directory = new System.IO.DirectoryInfo(this._path);
                    return directory.Name;
                }
            }
        }

        public string Path
        {
            get
            {
                return _path;
            }
        }

        public long LastModified
        {
            get
            {
                if (_type == FileSystemObjectType.FILE)
                {
                    return System.IO.File.GetLastWriteTime(this.@_path).Ticks;
                }
                else
                {
                    var directory = new System.IO.DirectoryInfo(this._path);
                    return directory.LastWriteTime.Ticks;
                }
            }
        }

        public long Size
        {
            get
            {
                if (_type == FileSystemObjectType.FILE)
                {
                    return new System.IO.FileInfo(this.@_path).Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is FileSystemObject)
            {
                return System.IO.Path.GetFullPath(((FileSystemObject)obj).Path) == System.IO.Path.GetFullPath(this._path);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.Path.GetHashCode();
        }

        public static string RelativePath(FileSystemObject from, FileSystemObject to)
        {
            int levelFrom = _countLevel(from);
            int levelTo = _countLevel(to);
            int prefixes = 0;

            StringBuilder sb = new StringBuilder();

            while (!from.Equals(to))
            {
                int diff = levelTo - levelFrom;
                if (diff <= 0)
                {
                    ++prefixes;
                    --levelFrom;

                    from = from.Parent;
                }
                if (diff >= 0)
                {
                    sb.Insert(0, System.IO.Path.PathSeparator);
                    sb.Insert(0, to.Name);

                    --levelTo;

                    to = to.Parent;
                }
            }

            while (prefixes-- > 0)
            {
                sb.Insert(0, ".. " + System.IO.Path.PathSeparator);
            }

            return sb.ToString();
        }

        private static int _countLevel(FileSystemObject file)
        {
            int level = 0;
            while (file != null)
            {
                file = file.Parent;
                level++;
            }
            return level;
        }

        public FileSystemObject[] ListFolder(FileFilter filter)
        {
            if (this.IsFile)
            {
                throw new ArgumentOutOfRangeException("You cannot list a file's files");
            }

            string[] files;
            Dictionary<string, FileSystemObject> cache = new Dictionary<string, FileSystemObject>();

            if (filter == null)
            {
                files = System.IO.Directory.GetFiles(this.Path);
            }
            else
            {
                files = Directory.EnumerateFiles(this.Path, "*.*", SearchOption.TopDirectoryOnly).Where(s =>
                {
                    FileSystemObject fileSystemObject = FileSystemObject.FromPath(s);
                    bool accepted = filter.Accept(fileSystemObject);
                    if (accepted)
                    {
                        cache.Add(s, fileSystemObject);
                    }
                    return accepted;
                }).ToArray();
            }

            FileSystemObject[] filesAsObjects = new FileSystemObject[files.Length];

            int i = 0;
            foreach (string file in files)
            {
                if (cache.ContainsKey(file))
                {
                    filesAsObjects[i] = cache[file];
                }
                else
                {
                    filesAsObjects[i] = FileSystemObject.FromPath(file);
                }
            }

            return filesAsObjects;
        }

        public static FileSystemObject FromPath(string path)
        {
            FileAttributes attr = System.IO.File.GetAttributes(@path);

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                string parentDirectoryPath = System.IO.Path.GetDirectoryName(path);
                if (parentDirectoryPath == null)
                {
                    return new FileSystemObject(FileSystemObjectType.ROOT, path);
                }

                var parentDirectory = new System.IO.DirectoryInfo(parentDirectoryPath).Parent;
                if (parentDirectory == null)
                {
                    return new FileSystemObject(FileSystemObjectType.ROOT, path);
                }

                return new FileSystemObject(FileSystemObjectType.DIRECTORY, path);
            }
            else
            {
                return new FileSystemObject(FileSystemObjectType.FILE, path);
            }
        }

        public static FileSystemObject[] Roots
        {
            get
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                FileSystemObject[] fileSystemObjects = new FileSystemObject[drives.Length];
                for (int i = 0; i < drives.Length; i++)
                {
                    fileSystemObjects[i] = FileSystemObject.FromPath(drives[i].RootDirectory.FullName);
                }
                return fileSystemObjects;
            }
        }

        public FileSystemObject(FileSystemObjectType type, string path)
        {
            this._type = type;
            this._path = path;
        }
    }
}
