// Copyright (c) 2020 Johannes Schiemer
// Licensed under the MIT License
using System;
using System.Collections.Generic;
using System.Text;

namespace WpfFileSelector
{
    class FolderResource
    {
        public string Path;
        public string DisplayName;
        public string ImagePath;
        public bool IsFolder = false;

        public FolderResource()
        {

        }

        public FolderResource(string Path, string DisplayName)
        {
            this.Path = Path;
            this.DisplayName = DisplayName;
        }

        public FolderResource(string Path, string DisplayName, bool IsFolder)
        {
            this.Path = Path;
            this.DisplayName = DisplayName;
            this.IsFolder = IsFolder;
        }

        public FolderResource(string Path, string DisplayName, string ImagePath)
        {
            this.Path = Path;
            this.DisplayName = DisplayName;
            this.ImagePath = ImagePath;
        }

        public FolderResource(string Path, string DisplayName, string ImagePath, bool IsFolder)
        {
            this.Path = Path;
            this.DisplayName = DisplayName;
            this.ImagePath = ImagePath;
            this.IsFolder = IsFolder;
        }
    }
}
