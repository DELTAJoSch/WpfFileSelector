# WpfFileSelector
A File/Folder selection user control for WPF (.Net Core)

# How to install
There ar currently two options:
1. Download the precompilde binary from releases
2. Build from source

## Download precompiled binary
Got to the "releases" page and download the desired version.
Add the binary to your project and enjoy

## Build from source
To build from source either clone or download this repository to your machine. Open the .sln file in Visual Studio. Once Visual Studio has opened, make sure to set "Solution Configurations" to Build. Press the Build button and copy the compiled dll from YOUR_LOCATION\WpfFileSelector\WpfFileSelector\bin\Release\netcoreapp3.1\WpfFileSelector.dll
Add this binary to your project and enjoy!

# Usage
Add this file to your project in Visual Studio. Do this by copying the binary into your project folder and the going into Visual Studio. Right-Click on your project in the solution explorer and press add --> reference. Select Browse and navigate to your file. Press ok.

To include the Control in your XAML:
Add this reference: xmlns:WpfFileSelector="clr-namespace:WpfFileSelector;assembly=WpfFileSelector"
and then create a control using this XAML-Code: <WpfFileSelector:FileBrowser x:Name="YourNameHere"/>

There are multiple options this control includes:
You can set the start folder of the control by giving it a start path: StartFolder="C://YOUR_PATH_HERE"
You can get the currently selected folder by querying or binding to: SelectedPath
You can get the currently selected file by querying or binding to: SelectedFile
The appearance of the control can be cahnged using these colors:
SelectedBrush - This is the brush used to colour the selected file
BackgroundBrush - This is the brush used to colour the background of the control
BorderBrush - This is the brush used to colour the border of the file- and folder- entries
FolderBackgroundBrush - This is the brush used to colour each of the file- and folder- entries

You can also set a filter to only show files matching this filter:
FileFilter="YOUR_FILTER_HERE"
This Filter should be a string that contains a regular expression. You can pretty much put anything in here. This regular expression is matched against the filename of each file in any currently selected folder. Please note: Refrain from using ^ and $ to denote that the whole filename should be matched. This has already been done for you.

For simple file-ending filtering this regex will do the trick: ([^\t\n\r\\.\\\\\\\*])\*(\\.txt|\\.docx|\\.jpg)
Simply replace the currently inserted fileendings with the ones you wish to filter for. Add more fileendings by adding another |\\.FILEENDING. The same goes for removing fileendings: remove by deleting |\\.FILEENDING
