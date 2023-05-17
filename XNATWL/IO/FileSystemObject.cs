using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;
using static System.Net.WebRequestMethods;

namespace XNATWL.IO
{
    /// <summary>
    /// A class for using an interface similar to <i>java.io.File</i> code in C#.
    /// This means that the referenced object can represent a directory or a file.
    /// </summary>
    public class FileSystemObject
    {
        public enum FileSystemObjectType
        {
            File,
            Directory,
            Root
        }

        private FileSystemObjectType _type;
        private string _path;

        /// <summary>
        /// get only: return <b>TRUE</b> if the FileSystemObject is a directory
        /// </summary>
        public bool IsDirectory
        {
            get
            {
                return _type == FileSystemObjectType.Directory || _type == FileSystemObjectType.Root;
            }
        }

        /// <summary>
        /// get only: return <b>TRUE</b> if the FileSystemObject is a folder
        /// </summary>
        public bool IsFolder
        {
            get
            {
                return _type == FileSystemObjectType.Directory || _type == FileSystemObjectType.Root;
            }
        }

        /// <summary>
        /// get only: return <b>TRUE</b> if the FileSystemObject is a file
        /// </summary>
        public bool IsFile
        {
            get
            {
                return _type == FileSystemObjectType.File;
            }
        }

        /// <summary>
        /// get only: return <b>TRUE</b> if the FileSystemObject is hidden by the OS
        /// </summary>
        public bool IsHidden
        {
            get
            {
                if (_type == FileSystemObjectType.File)
                {
                    return System.IO.File.GetAttributes(this.@_path).HasFlag(System.IO.FileAttributes.Hidden);
                }
                else if (_type == FileSystemObjectType.Directory)
                {
                    return new System.IO.DirectoryInfo(this._path).Attributes.HasFlag(System.IO.FileAttributes.Hidden);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// get only: return the parent directory of either a file or a direcctory
        /// </summary>
        public FileSystemObject Parent
        {
            get
            {
                if (_type == FileSystemObjectType.File)
                {
                    // get the directory knowing that the path is suffixed with a filename
                    string parentDirectoryPath = System.IO.Path.GetDirectoryName(this.@_path);
                    if (parentDirectoryPath == null)
                    {
                        return null;
                    }

                    // look up the info for the parent directory
                    var parentDirectory = new System.IO.DirectoryInfo(parentDirectoryPath);

                    if (parentDirectory.Parent == null)
                    {
                        // files without a parent directory must belong to a root directory
                        return new FileSystemObject(FileSystemObjectType.Root, parentDirectoryPath);
                    }
                    else
                    {
                        // the info tells us the path of the directory containing the file
                        return new FileSystemObject(FileSystemObjectType.Directory, parentDirectoryPath);
                    }
                }
                else if (_type == FileSystemObjectType.Directory)
                {
                    var directory = new System.IO.DirectoryInfo(this._path);

                    if (directory.Parent == null)
                    {
                        // this must actually be a root directory, but we are returning null anyways
                        return null;
                    }
                    else
                    {
                        return new FileSystemObject(FileSystemObjectType.Directory, directory.Parent.FullName);
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the name of the file or directory
        /// </summary>
        public string Name
        {
            get
            {
                if (_type == FileSystemObjectType.File)
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

        /// <summary>
        /// Return the path tracked by the FileSystemObject
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
        }

        /// <summary>
        /// Return the timestamp for when the FSO was last modified
        /// </summary>
        public long LastModified
        {
            get
            {
                if (_type == FileSystemObjectType.File)
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

        /// <summary>
        /// Return the size of the file, or 0 for a directory.
        /// A recursive scan for the directory size is omitted unless later required.
        /// </summary>
        public long Size
        {
            get
            {
                if (_type == FileSystemObjectType.File)
                {
                    return new System.IO.FileInfo(this.@_path).Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Test if two file system objects represent the same entity on the file system
        /// </summary>
        /// <param name="obj">object to compare</param>
        /// <returns><b>TRUE</b> if both the file system objects represent the same path</returns>
        public override bool Equals(object obj)
        {
            if (obj is FileSystemObject)
            {
                // resolve the paths and compare them
                return System.IO.Path.GetFullPath(((FileSystemObject)obj).Path) == System.IO.Path.GetFullPath(this._path);
            }

            return false;
        }

        /// <summary>
        /// Return the hash code of the FSO
        /// </summary>
        /// <returns>The HashCode for the path represented by the FileSystemObject.</returns>
        public override int GetHashCode()
        {
            return this.Path.GetHashCode();
        }

        /// <summary>
        /// Open a read stream to the FileSystemObject
        /// </summary>
        /// <returns>FileStream in read mode</returns>
        public FileStream OpenRead()
        {
            return System.IO.File.OpenRead(this._path);
        }

        /// <summary>
        /// Find a common path between 2 FileSystemObjects
        /// </summary>
        /// <param name="from">Where to start looking</param>
        /// <param name="to">What to compare</param>
        /// <returns>Relative path of both objects</returns>
        public static string RelativePath(FileSystemObject from, FileSystemObject to)
        {
            int levelFrom = CountLevel(from);
            int levelTo = CountLevel(to);
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

        /// <summary>
        /// Count the number of levels on the OS' file tree by recursively finding a parent
        /// </summary>
        /// <param name="file">File to count levels</param>
        /// <returns>Number of levels</returns>
        private static int CountLevel(FileSystemObject file)
        {
            int level = 0;
            while (file != null)
            {
                file = file.Parent;
                level++;
            }
            return level;
        }

        /// <summary>
        /// List the items in this FileSystemObject that representing a directory
        /// </summary>
        /// <param name="filter">a file filter to sieve the results</param>
        /// <returns>array of matching FIleSystemObjects</returns>
        /// <exception cref="ArgumentOutOfRangeException">THrown when called on a file</exception>
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
                // raw lookup; find the files *anyways*
                files = Directory.GetFiles(this.Path);
            }
            else
            {
                // apply the filter by calling EnumerateFiles on the directory
                files = Directory.EnumerateFiles(this.Path, "*.*", SearchOption.TopDirectoryOnly).Where(s =>
                {
                    // annoyingly, we have to make the object before we run it through the filter.
                    // this means sometimes we may be unnecessarily creating FSO objects
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

            // utilise the responses from either the cache (filter active) or straight forward raw lookup 
            int i = 0;
            foreach (string file in files)
            {
                if (cache.ContainsKey(file))
                {
                    filesAsObjects[i] = cache[file];
                }
                else
                {
                    // create the necessary object
                    filesAsObjects[i] = FileSystemObject.FromPath(file);
                }
            }

            return filesAsObjects;
        }

        /// <summary>
        /// Parse a path into a new FileSystemObject
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FileSystemObject FromPath(string path)
        {
            FileAttributes attr = System.IO.File.GetAttributes(@path); // this works on directories too, despite being in System.IO.File

            // rule out the path represeenting a directory
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                string parentDirectoryPath = System.IO.Path.GetDirectoryName(path);
                if (parentDirectoryPath == null) // gracefully treat the path as a root directory
                {
                    return new FileSystemObject(FileSystemObjectType.Root, path);
                }

                var parentDirectory = new System.IO.DirectoryInfo(parentDirectoryPath).Parent;
                if (parentDirectory == null) // gracefully treat the path as a root directory
                {
                    return new FileSystemObject(FileSystemObjectType.Root, path);
                }

                return new FileSystemObject(FileSystemObjectType.Directory, path);
            }
            // well that wasn't a file...
            else
            {
                return new FileSystemObject(FileSystemObjectType.File, path);
            }
        }

        /// <summary>
        /// Get list of file system roots (usually different drives under Windows, Linux untested)
        /// </summary>
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

        /// <summary>
        /// Create a new file system object (warning: this does not check if the object exists)
        /// </summary>
        /// <param name="type">Type of file system object</param>
        /// <param name="path">Path to the object</param>
        public FileSystemObject(FileSystemObjectType type, string path)
        {
            this._type = type;
            this._path = path;
        }

        /// <summary>
        /// Create a new file system object relative to the given parentDirectory
        /// </summary>
        /// <param name="parentDirectory">Containing folder of the file</param>
        /// <param name="fileName">Name of the file</param>
        public FileSystemObject(FileSystemObject parentDirectory, string fileName) : this(FileSystemObjectType.File, System.IO.Path.Combine(parentDirectory.Path, fileName))
        {

        }
    }
}
