using System;
using System.Collections.Generic;
using System.IO;
/*
 * Copyright (c) 2008-2012, Matthias Mann
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *     * Redistributions of source code must retain the above copyright notice,
 *       this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Matthias Mann nor the names of its contributors may
 *       be used to endorse or promote products derived from this software
 *       without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using XNATWL.IO;

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
