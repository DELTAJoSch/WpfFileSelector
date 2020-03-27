// Copyright (c) 2020 Johannes Schiemer
// Licensed under the MIT License
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfFileSelector
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class FileBrowser : UserControl
    {
        /// <summary>
        /// This is the standard constructor for the FileBrowser UserControl
        /// </summary>
        public FileBrowser()
        {
            InitializeComponent();

            //Set the brushes to standard values
            //these are overwritten automatically if the user specified different colours in XAML or via Bindings
            if (SelectedBrush == null)
            {
                SelectedBrush = new SolidColorBrush();
                SelectedBrush.Color = Colors.LawnGreen;
            }

            if (FolderBackgroundBrush == null)
            {
                FolderBackgroundBrush = new SolidColorBrush();
                FolderBackgroundBrush.Color = Color.FromArgb(20, 255, 205, 0);
            }

            if (BorderBrush == null)
            {
                BorderBrush = new SolidColorBrush();
                BorderBrush.Color = Color.FromArgb(150, 102, 102, 102);
            }

            if (BackgroundBrush == null)
            {
                BackgroundBrush = new SolidColorBrush();
                BackgroundBrush.Color = Color.FromArgb(10, 80, 80, 80);
            }
        }

        private Dictionary<string, FolderResource> CurrentFolderContent = new Dictionary<string, FolderResource>();
        private Dictionary<string, FolderResource> QuickAccessContent = new Dictionary<string, FolderResource>();

        private string PreviousPath;
        private System.Windows.Controls.Grid previousGrid;
        private bool FirstRender = true;

        /// <summary>
        /// This is the dependency property for the start folder
        /// </summary>
        public static readonly DependencyProperty StartFolderProperty =
            DependencyProperty.Register(
            "StartFolder", typeof(string),
            typeof(FileBrowser));

        /// <summary>
        /// This is the dependency property for the selected path
        /// </summary>
        public static readonly DependencyProperty SelectedPathProperty =
            DependencyProperty.Register(
            "SelectedPath", typeof(string),
            typeof(FileBrowser));

        /// <summary>
        /// This is the dependency property for the selected file
        /// </summary>
        public static readonly DependencyProperty SelectedFileProperty =
            DependencyProperty.Register(
            "SelectedFile", typeof(string),
            typeof(FileBrowser));

        /// <summary>
        /// This is the dependency property for the selected brush
        /// </summary>
        public static readonly DependencyProperty SelectedBrushProperty =
            DependencyProperty.Register(
            "SelectedBrush", typeof(SolidColorBrush),
            typeof(FileBrowser));

        /// <summary>
        /// This is the dependency property for the folder background brush
        /// </summary>
        public static readonly DependencyProperty FolderBackgroundBrushProperty =
            DependencyProperty.Register(
            "FolderBackgroundBrush", typeof(SolidColorBrush),
            typeof(FileBrowser));

        /// <summary>
        /// This is the dependency property for the border brush
        /// </summary>
        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register(
            "BorderBrush", typeof(SolidColorBrush),
            typeof(FileBrowser));

        /// <summary>
        /// This is the dependency property for the background brush
        /// </summary>
        public static readonly DependencyProperty BackgroundBrushProperty =
            DependencyProperty.Register(
            "BackgroundBrush", typeof(SolidColorBrush),
            typeof(FileBrowser));

        /// <summary>
        /// This is the dependency property for the file filter
        /// </summary>
        public static readonly DependencyProperty FileFilterProperty =
            DependencyProperty.Register(
            "FileFilter", typeof(string),
            typeof(FileBrowser));

        /// <summary>
        /// The StartFolder string contains a path to accessible via the quickaccess menu
        /// Sets the starting point for the program
        /// </summary>
        public string StartFolder
        {
            get { return (string)GetValue(StartFolderProperty); }
            set { SetValue(StartFolderProperty, value); }
        }

        /// <summary>
        /// This contains a path to file selected by the user
        /// </summary>
        public string SelectedFile
        {
            get { return (string)GetValue(SelectedFileProperty); }
            set { SetValue(SelectedFileProperty, value); }
        }

        /// <summary>
        /// This contains a path to the current folder
        /// </summary>
        public string SelectedPath
        {
            get { return (string)GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); SetMainFolderView(); }
        }

        /// <summary>
        /// This is the brush used to colour the selected file
        /// </summary>
        public SolidColorBrush SelectedBrush
        {
            get { return (SolidColorBrush)GetValue(SelectedBrushProperty); }
            set { SetValue(SelectedBrushProperty, value); }
        }

        /// <summary>
        /// This is the brush used to colour each of the file- and folder- entries
        /// </summary>
        public SolidColorBrush FolderBackgroundBrush
        {
            get { return (SolidColorBrush)GetValue(FolderBackgroundBrushProperty); }
            set { SetValue(FolderBackgroundBrushProperty, value); }
        }

        /// <summary>
        /// This is the brush used to colour the border of the file- and folder- entries
        /// </summary>
        public SolidColorBrush BorderBrush
        {
            get { return (SolidColorBrush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        /// <summary>
        /// This is the brush used to colour the background of the control
        /// </summary>
        public SolidColorBrush BackgroundBrush
        {
            get { return (SolidColorBrush)GetValue(BackgroundBrushProperty); }
            set { SetValue(BackgroundBrushProperty, value); }
        }

        /// <summary>
        /// This is a regex that can be set by the user.
        /// It automatically mathes against the whole filename, do not use ^ and $ to mark beginning and end
        /// Example: ([^\t\n\r\.\\\*])*(\.txt|\.docx|\.jpg) shows only .docx, .txt and .jpg files
        /// </summary>
        public string FileFilter
        {
            get { return (string)GetValue(FileFilterProperty); }
            set { SetValue(FileFilterProperty, value); }
        }

        /// <summary>
        /// This overrides the standard OnRender method
        /// On the first render, the QuickAccess menu is drawn and the background colours are applied
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (FirstRender)
            {
                //Add the selectable StartFolder and the standard folders to the QuickAccess menu
                QuickAccessContent.Add("StartFolderButton", new FolderResource(StartFolder, "Start Folder"));
                QuickAccessContent.Add("DocumentsButton", new FolderResource(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Documents"));
                QuickAccessContent.Add("PicturesButton", new FolderResource(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Pictures"));
                QuickAccessContent.Add("MusicButton", new FolderResource(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Music"));
                QuickAccessContent.Add("VideosButton", new FolderResource(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Videos"));

                foreach (KeyValuePair<string, FolderResource> valuePair in QuickAccessContent)
                {
                    System.Windows.Controls.Button button = new Button();
                    button.Click += GenericQuickAccessButton_Click;
                    button.Content = valuePair.Value.DisplayName;
                    button.Name = valuePair.Key;
                    button.Background = FolderBackgroundBrush;
                    button.BorderBrush = BorderBrush;
                    button.BorderThickness = new Thickness(0, 0.5, 0, 0.5);
                    button.Margin = new Thickness(0, 0.25, 0, 0.25);

                    QuickAccessView.Children.Add(button);
                }

                QuickAccessView.Background = BackgroundBrush;
                FolderView.Background = BackgroundBrush;
                attributionLabel.Background = BackgroundBrush;

                FirstRender = false;
            }
        }

        /// <summary>
        /// This method is called by all buttons contained in the QuickAccess menu
        /// </summary>
        /// <param name="sender">the calling button/object</param>
        /// <param name="e">the EventArgs associated with the call</param>
        private void GenericQuickAccessButton_Click(object sender, EventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            FolderResource res;

            QuickAccessContent.TryGetValue(button.Name, out res);

            SelectedPath = res.Path;
        }

        /// <summary>
        /// This method is called by all buttons associated with file entries contained in the FolderView menu
        /// </summary>
        /// <param name="sender">the calling button/object</param>
        /// <param name="e">the EventArgs associated with the call</param>
        private void GenericFileButton_Click(object sender, EventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            FolderResource res;

            CurrentFolderContent.TryGetValue(button.Name, out res);

            SelectedFile = res.Path;

            //get the parent (grid) of the button and change the colour to selected
            System.Windows.Controls.Grid grid = (System.Windows.Controls.Grid)button.Parent;
            grid.Background = SelectedBrush;

            if (previousGrid != null)
                previousGrid.Background = FolderBackgroundBrush;

            previousGrid = grid;
        }

        /// <summary>
        /// This method is called by all buttons receiving a double click associated with folder entries contained in the FolderView menu
        /// </summary>
        /// <param name="sender">the calling button/object</param>
        /// <param name="e">the EventArgs associated with the call</param>
        private void GenericFolderButton_DoubleClick(object sender, EventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            FolderResource res;

            CurrentFolderContent.TryGetValue(button.Name, out res);

            SelectedPath = res.Path;
        }

        /// <summary>
        /// This creates the correct entries for the FolderView when the path changes
        /// </summary>
        private void SetMainFolderView()
        {
            CurrentFolderContent.Clear();

            ClearFolderView();

            GenerateBackButton();

            //check the current path on validity
            if (!String.IsNullOrEmpty(SelectedPath))
            {
                //analyze the directory and add to list of buttons
                AnalyzeDirectory();

                //create the correct child elements
                foreach (KeyValuePair<string, FolderResource> valuePair in CurrentFolderContent)
                {
                    var grid = CreateGrid();
                    var button = CreateButton(valuePair);
                    var img = CreateImage(valuePair);

                    Grid.SetColumn(button, 1);

                    grid.Children.Add(button);
                    grid.Children.Add(img);

                    System.Windows.Controls.Border border = new Border();
                    border.BorderThickness = new Thickness(0, 0.5, 0, 0.5);
                    border.Background = FolderBackgroundBrush;
                    border.BorderBrush = BorderBrush;
                    border.Margin = new Thickness(0, 0.25, 0, 0.25);
                    border.Child = grid;

                    FolderView.Children.Add(border);
                }
            }
        }

        /// <summary>
        /// creates a button for the FolderView
        /// </summary>
        /// <param name="valuePair">FolderResource containing information used in the creating of the button</param>
        /// <returns>a button element</returns>
        private System.Windows.Controls.Button CreateButton(KeyValuePair<string, FolderResource> valuePair)
        {
            var label = CreateLabel(valuePair);

            System.Windows.Controls.Button button = new Button();
            if (valuePair.Value.IsFolder)
            {
                button.MouseDoubleClick += GenericFolderButton_DoubleClick;
            }
            else
            {
                button.Click += GenericFileButton_Click;
            }
            button.Content = label;
            button.Name = valuePair.Key;
            button.Height = 28;
            button.BorderThickness = new Thickness(0, 0, 0, 0);
            button.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            button.VerticalAlignment = VerticalAlignment.Stretch;

            return button;
        }

        /// <summary>
        /// Creates an image for the FolderView
        /// Icon depends on wether the entry is a file or a folder
        /// </summary>
        /// <param name="valuePair">FolderResource containing information used in the creating of the image</param>
        /// <returns>an image control</returns>
        private System.Windows.Controls.Image CreateImage(KeyValuePair<string, FolderResource> valuePair)
        {
            System.Windows.Controls.Image img = new Image();
            Uri uri = new Uri(@"pack://application:,,,/WpfFileSelector;component/Images/001-file.png");
            if (valuePair.Value.IsFolder)
            {
                uri = new Uri(@"pack://application:,,,/WpfFileSelector;component/Images/002-folder.png");
            }
            img.Source = new BitmapImage(uri);
            img.Stretch = Stretch.Uniform;
            img.HorizontalAlignment = HorizontalAlignment.Left;
            img.VerticalAlignment = VerticalAlignment.Center;
            img.Height = 28;

            return img;
        }

        /// <summary>
        /// This method creates a label control to be used in conjunction with a button
        /// </summary>
        /// <param name="valuePair">FolderResource containing information used in the creating of the label</param>
        /// <returns>a label control</returns>
        private System.Windows.Controls.Label CreateLabel(KeyValuePair<string, FolderResource> valuePair)
        {
            System.Windows.Controls.Label label = new Label();
            label.Content = valuePair.Value.DisplayName;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            label.FontSize = 10;

            return label;
        }

        /// <summary>
        /// This method creates a standard grid to arrange the button and the image for a FolderView entry on
        /// </summary>
        /// <returns>a grid</returns>
        private System.Windows.Controls.Grid CreateGrid()
        {
            System.Windows.Controls.Grid grid = new Grid();

            System.Windows.Controls.ColumnDefinition col1 = new ColumnDefinition();
            col1.Width = new GridLength(50, GridUnitType.Auto);

            System.Windows.Controls.ColumnDefinition col2 = new ColumnDefinition();
            col2.Width = new GridLength(1000, GridUnitType.Auto);

            grid.ColumnDefinitions.Add(col1);
            grid.ColumnDefinitions.Add(col2);
            grid.HorizontalAlignment = HorizontalAlignment.Stretch;
            grid.VerticalAlignment = VerticalAlignment.Stretch;
            grid.Background = FolderBackgroundBrush;

            return grid;
        }

        /// <summary>
        /// Clears the FolderView control from all of its child elements
        /// </summary>
        private void ClearFolderView()
        {
            foreach (object o in FolderView.Children)
            {
                try
                {
                    var border = (System.Windows.Controls.Border)o;
                    var grid = (System.Windows.Controls.Grid)border.Child;

                    foreach (object b in grid.Children)
                    {
                        if (b.GetType() == typeof(System.Windows.Controls.Button))
                        {
                            var btn = (System.Windows.Controls.Button)b;
                            btn.Click -= GenericFileButton_Click;
                            btn.MouseDoubleClick -= GenericFolderButton_DoubleClick;
                        }
                    }
                }
                catch (InvalidCastException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            FolderView.Children.Clear();
        }

        /// <summary>
        /// This method generates the topmost button, the "back"-button
        /// </summary>
        private void GenerateBackButton()
        {
            //calculate the path of the parent folder by removing the last folder level
            //and add a back-button
            var path = SelectedPath.Split("\\");

            StringBuilder builder = new StringBuilder();
            builder.Append(path[0]);
            for (int i = 1; i < path.Length - 1; i++)
            {
                builder.Append("\\");
                builder.Append(path[i]);
            }
            PreviousPath = builder.ToString();
            CurrentFolderContent.Add("btn_back", new FolderResource(PreviousPath, "...", true));
        }

        /// <summary>
        /// Indexes all files and folders in a given folder and adds them to the list of contents
        /// </summary>
        private void AnalyzeDirectory()
        {
            int x = 0;

            // add all folders in the current path
            foreach (string s in Directory.EnumerateDirectories(SelectedPath))
            {
                string[] name = s.Split("\\");

                CurrentFolderContent.Add("btn_" + x.ToString(), new FolderResource(s, name.Last(), true));
                x++;
            }

            //add all files or all files matching the filter in the currently selected folder
            foreach (string s in Directory.EnumerateFiles(SelectedPath))
            {
                string[] name = s.Split("\\");

                if (String.IsNullOrEmpty(FileFilter))
                {
                    CurrentFolderContent.Add("btn_" + x.ToString(), new FolderResource(s, name.Last(), false));
                }
                else
                {
                    if (Regex.IsMatch(name.Last(), $"^{FileFilter}$"))
                    {
                        CurrentFolderContent.Add("btn_" + x.ToString(), new FolderResource(s, name.Last(), false));
                    }
                }

                x++;
            }
        }
    }
}
