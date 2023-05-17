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

using System.IO;
using System.Security.Policy;

namespace XNATWL.Model
{
    /// <summary>
    /// An generic file system abstraction which is used as base for file system widgets like FolderBrowser.
    /// </summary>
    public interface FileSystemModel
    {
        /// <summary>
        /// The separator character used to separate folder names in a path.
        /// This should usually be a string with one character.
        /// </summary>
        string Separator
        {
            get;
        }

        /// <summary>
        /// Returns the object which represents the specified file name.
        /// </summary>
        /// <param name="path">the file path</param>
        /// <returns>the object or null if the file was not found</returns>
        object FileByPath(string path);

        /// <summary>
        /// Returns the parent folder of the specified file or folder
        /// </summary>
        /// <param name="file">the file or folder - needs to be a valid file or folder</param>
        /// <returns>the parent folder or null if the file parameter was invalid or was a root node</returns>
        object Parent(object file);

        /// <summary>
        /// Returns <b>true</b> if the object is a valid folder in this file system
        /// </summary>
        /// <param name="file">the object to check</param>
        /// <returns><b>true</b> if it is a folder</returns>
        bool IsFolder(object file);

        /// <summary>
        /// Returns true if the object is a valid file in this file system
        /// </summary>
        /// <param name="file">the object to check</param>>
        /// <returns><b>true</b> if it is a file</returns>
        bool IsFile(object file);

        /// <summary>
        /// Checks if the specified object is a hidden file or folder.
        /// </summary>
        /// <param name="file">the object to check</param>
        /// <returns>if it is a valid file or folder and is hidden</returns>
        bool IsHidden(object file);

        /// <summary>
        /// Returns the name of the specified object
        /// </summary>
        /// <param name="file">the object to query</param>
        /// <returns>the name or null if it was not a valid file or folder</returns>
        string NameOf(object file);

        /// <summary>
        /// Returns the path of the specified object
        /// </summary>
        /// <param name="file">the object to query</param>
        /// <returns>the path or null if it was not a valid file or folder</returns>
        string PathOf(object file);

        /// <summary>
        /// Computes a relative path from <paramref name="from"/> to <paramref name="to"/>
        /// </summary>
        /// <param name="from">starting point for the relative path - must be a folder</param>
        /// <param name="to">the destination for the relative path</param>
        /// <returns>the relative path or null if it could not be computed</returns>
        string RelativePath(object from, object to);

        /// <summary>
        /// Returns the size of the file
        /// </summary>
        /// <param name="file">the object to query</param>
        /// <returns>the size of the file or -1 if it's not a valid file</returns>
        long SizeOf(object file);

        /// <summary>
        /// Returns the last modified date/time of the file or folder
        /// </summary>
        /// <param name="file">the object to query</param>
        /// <returns>the last modified date/time or 0</returns>
        long LastModifiedOf(object file);

        /// <summary>
        /// Checks if the two objects specify the same file or folder
        /// </summary>
        /// <param name="file1">the first object</param>
        /// <param name="file2">the second object</param>
        /// <returns><b>true</b> if they are the same</returns>
        bool Equals(object file1, object file2);

        /// <summary>
        /// Finds the index of a file or folder in a list of objects.
        /// </summary>
        /// <param name="list">the list of objects</param>
        /// <param name="file">the object to search</param>
        /// <returns>the index or -1 if it was not found</returns>
        int Find(object[] list, object file);

        /// <summary>
        /// Lists all file system roots
        /// </summary>
        /// <returns>the file system roots</returns>
        object[] ListRoots();

        /// <summary>
        /// Lists all files or folders in the specified folder.
        /// </summary>
        /// <param name="file">the folder to list</param>
        /// <param name="filter">an optional filter - can be null</param>
        /// <returns>the (filtered) content of the folder</returns>
        object[] ListFolder(object file, FileFilter filter);

        /// <summary>
        /// Locates a special folder such as the user profile
        /// </summary>
        /// <param name="key">the special folder key</param>
        /// <returns>the object for this folder or null if it couldn't be located</returns>
        object SpecialFolder(string key);

        /// <summary>
        /// Opens a FileStream for the specified file
        /// </summary>
        /// <param name="file">the file system object to be read</param>
        /// <returns>a FileStream or null if the file couldn't be found</returns>
        FileStream OpenStream(object file);
    }

    /// <summary>
    /// An interface for using a filter to list files on the file system model
    /// </summary>
    public interface FileFilter
    {
        /// <summary>
        /// Decide whether or not to list file
        /// </summary>
        /// <param name="file">File to accept/decline</param>
        /// <returns>Whether or not the file will be listed</returns>
        bool Accept(object file);
    }
}
