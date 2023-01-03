using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.IO;
using static System.Net.WebRequestMethods;

namespace XNATWL.Model
{
    public class DotNetFileSystemModel : FileSystemModel
    {
        public static string USERPROFILE_FOLDER = "UserProfile";

        private static DotNetFileSystemModel INSTANCE = new DotNetFileSystemModel();

        public static DotNetFileSystemModel getInstance()
        {
            return INSTANCE;
        }

        public string Separator
        {
            get
            {
                return System.IO.Path.PathSeparator.ToString();
            }
        }

        public new bool Equals(object file1, object file2)
        {
            return ((FileSystemObject)file1).Equals((FileSystemObject)file2);
        }

        public object FileByPath(string path)
        {
            return FileSystemObject.FromPath(path);
        }

        public int Find(object[] list, object file)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (file == list[i])
                {
                    return i;
                }
            }

            return -1;
        }

        public bool IsFile(object file)
        {
            return ((FileSystemObject)file).IsFile;
        }

        public bool IsFolder(object file)
        {
            return ((FileSystemObject)file).IsFolder;
        }

        public bool IsHidden(object file)
        {
            return ((FileSystemObject)file).IsHidden;
        }

        public long LastModifiedOf(object file)
        {
            return ((FileSystemObject)file).LastModified;
        }

        public object[] ListFolder(object file, FileFilter filter)
        {
            return ((FileSystemObject)file).ListFolder(filter);
        }

        public object[] ListRoots()
        {
            return (object[])FileSystemObject.Roots;
        }

        public string NameOf(object file)
        {
            return ((FileSystemObject)file).Name;
        }

        public FileStream OpenStream(object file)
        {
            throw new NotImplementedException();
        }

        public object Parent(object file)
        {
            return ((FileSystemObject)file).Parent;
        }

        public string PathOf(object file)
        {
            return ((FileSystemObject)file).Path;
        }

        public string RelativePath(object from, object to)
        {
            return FileSystemObject.RelativePath((FileSystemObject)from, (FileSystemObject)to);
        }

        public long SizeOf(object file)
        {
            return ((FileSystemObject)file).Size;
        }

        public object SpecialFolder(string key)
        {
            if (key == "UserProfile")
            {
                return FileSystemObject.FromPath(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            }

            throw new NotImplementedException();
        }
    }
}
