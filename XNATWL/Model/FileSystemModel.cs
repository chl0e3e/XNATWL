using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface FileSystemModel
    {
        string Separator
        {
            get;
        }

        object FileByPath(string path);

        object Parent(object file);

        bool IsFolder(object file);

        bool IsFile(object file);

        bool IsHidden(object file);

        string NameOf(object file);

        string PathOf(object file);

        string RelativePath(object from, object to);

        long SizeOf(object file);

        long LastModifiedOf(object file);

        bool Equals(object file1, object file2);

        int Find(object[] list, object file);

        object[] ListRoots();

        object[] ListFolder(object file, FileFilter filter);

        object SpecialFolder(string key);

        FileStream OpenStream(object file);
    }

    public interface FileFilter
    {
        bool Accept(object file);
    }
}
