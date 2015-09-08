﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
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
using BCFDBManager.BCFUtils;
using BCFDBManager.DatabaseUtils;
using Microsoft.Win32;

namespace BCFDBManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string dbFile = "";
        private Dictionary<string/*fileId*/, BCFZIP> bcfDictionary = new Dictionary<string, BCFZIP>();
        private BCFZIP selectedFile = null;
        private Markup selectedMarkup = null;
        private Comment selectedComment = null;

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        private delegate void UpdateStatusLabelDelegate(System.Windows.DependencyProperty dp, Object value);

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "BCF Database Manager v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void DisplayUI()
        {
            try
            {
                List<BCFZIP> bcfZips = bcfDictionary.Values.OrderBy(o => o.ZipFileName).ToList();
                comboBoxFile.ItemsSource = null;
                comboBoxFile.ItemsSource = bcfZips;
                comboBoxFile.DisplayMemberPath = "ZipFileName";
                comboBoxFile.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Display BCF Information.\n" + ex.Message, "Display UI", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonNewDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Title = "Select a BCFZip File to Create a Database File";
                openDialog.DefaultExt = ".bcfzip";
                openDialog.Filter = "BCF (.bcfzip)|*.bcfzip";
                if ((bool)openDialog.ShowDialog())
                {
                    string bcfPath = openDialog.FileName;
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Title = "Specify a Database File Location";
                    saveDialog.DefaultExt = ".sqlite";
                    saveDialog.Filter = "SQLITE File (.sqlite)|*.sqlite";
                    saveDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(bcfPath);
                    if ((bool)saveDialog.ShowDialog())
                    {
                        dbFile = saveDialog.FileName;

                        double value = 0;
                        UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                        Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

                        UpdateStatusLabelDelegate updateLabelDelegate = new UpdateStatusLabelDelegate(statusLable.SetValue);
                        Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Gathering information from the BCF file..." });

                        progressBar.Visibility = System.Windows.Visibility.Visible;
                        progressBar.Value = 0;
                        progressBar.Maximum = 4;

                        BCFZIP bcfzip = BCFFileManager.ReadBCF(bcfPath);
                        value++;
                        Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

                        bcfzip = BCFFileManager.MapBinaryData(bcfzip);
                        value++;
                        Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

                        if (bcfzip.BCFComponents.Count > 0)
                        {
                            Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Writing BCF Information to the database..." });
                            bool created = DBManager.CreateTables(dbFile, bcfzip);
                            value++;
                            Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

                            bool written = DBManager.WriteDatabase(dbFile, bcfzip);
                            value++;
                            Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

                            if (created && written)
                            {
                                textBoxDB.Text = dbFile;

                                MessageBox.Show("The database file has been successfully created!!\n" + dbFile, "Database Created", MessageBoxButton.OK, MessageBoxImage.Information);
                                bcfDictionary = new Dictionary<string, BCFZIP>();
                                bcfDictionary.Add(bcfzip.FileId, bcfzip);
                                DisplayUI();
                            }
                            else
                            {
                                MessageBox.Show("The datbase file has not been successfully created.\nPlease check the log file.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            MessageBox.Show("An invalid BCFZip file has been selected.\n Please select another BCFZip file to create a database file.", "Invalid BCFZip", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        progressBar.Visibility = System.Windows.Visibility.Hidden;
                        statusLable.Text = "Ready";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to import BCF.\n" + ex.Message, "Import BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonConnectDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Title = "Select a database to be connected";
                openDialog.DefaultExt = ".sqlite";
                openDialog.Filter = "SQLITE File (.sqlite)|*.sqlite";
                if ((bool)openDialog.ShowDialog())
                {
                    dbFile = openDialog.FileName;
                    bcfDictionary = DBManager.ReadDatabase(dbFile);
                    DisplayUI();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to connect Database.\n"+ex.Message, "Connect Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxFile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxFile.SelectedItem)
                {
                    selectedFile = (BCFZIP)comboBoxFile.SelectedItem;
                    if (null != selectedFile)
                    {
                        Dictionary<string, BCFComponent> components = selectedFile.BCFComponents;
                        if (components.Count > 0)
                        {
                            var topics = from component in components.Values select component.MarkupInfo.Topic;
                            if (topics.Count() > 0)
                            {
                                List<Topic> topicList = topics.OrderBy(o => o.Index).ToList();
                                comboBoxIssue.ItemsSource = null;
                                comboBoxIssue.ItemsSource = topicList;
                                comboBoxIssue.DisplayMemberPath = "Title";
                                comboBoxIssue.SelectedIndex = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select a BCF File Item.\n"+ex.Message, "BCF File Item Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxIssue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxIssue.SelectedItem && null!= selectedFile)
                {
                    Topic selectedTopic = (Topic)comboBoxIssue.SelectedItem;
                    string guid = selectedTopic.Guid;

                    List<Comment> comments = new List<Comment>();
                    if (selectedFile.BCFComponents.ContainsKey(guid))
                    {
                        BCFComponent bcfComponent = selectedFile.BCFComponents[guid];
                        if (null != bcfComponent.MarkupInfo)
                        {
                            selectedMarkup = bcfComponent.MarkupInfo;
                            if (null != selectedMarkup)
                            {
                                comments = selectedMarkup.Comment;
                                comments = comments.OrderBy(o => o.Comment1).ToList();
                            }
                        }
                    }

                    dataGridComments.ItemsSource = null;
                    dataGridComments.ItemsSource = comments;
                    dataGridComments.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select an issue item.\n"+ex.Message, "Issue Item Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void dataGridComments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != dataGridComments.SelectedItem && null != selectedFile && null != selectedMarkup)
                {
                    selectedComment = (Comment) dataGridComments.SelectedItem;
                    string viewPointId = selectedComment.Viewpoint.Guid;
                    if (!string.IsNullOrEmpty(viewPointId))
                    {
                        var viewpoints = from vp in selectedMarkup.Viewpoints where vp.Guid == viewPointId select vp;
                        if (viewpoints.Count() > 0)
                        {
                            ViewPoint viewPoint = viewpoints.First();
                            if (null != viewPoint.SnapshotImage)
                            {
                                using (MemoryStream stream = new MemoryStream(viewPoint.SnapshotImage))
                                {
                                    imageSnapshot.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select a comment item.\n" + ex.Message, "Comment Item Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAddComment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxFile.SelectedItem && null != comboBoxIssue.SelectedItem)
                {
                    Comment comment = new Comment();
                    comment.Guid = Guid.NewGuid().ToString();
                    CommentViewpoint viewPoint = new CommentViewpoint();
                    viewPoint.Guid = selectedMarkup.Viewpoints.First().Guid;
                    comment.Viewpoint = viewPoint;

                    CommentWindow commentWindow = new CommentWindow(selectedMarkup, comment, CommentMode.ADD);
                    if (commentWindow.ShowDialog() == true)
                    {
                        selectedMarkup = commentWindow.SelectedMarkup;
                        selectedComment = commentWindow.SelectedComment;
                       
                        bool added = CommentManager.AddComment(selectedMarkup, selectedComment, dbFile);

                        string commentGuid = selectedComment.Guid;
                        int fileIndex = comboBoxFile.SelectedIndex;
                        int issueIndex = comboBoxIssue.SelectedIndex;

                        string issueId = selectedMarkup.Topic.Guid;
                        if (selectedFile.BCFComponents.ContainsKey(issueId))
                        {
                            BCFComponent bcfComponent = selectedFile.BCFComponents[issueId];
                            bcfComponent.MarkupInfo = selectedMarkup;

                            selectedFile.BCFComponents.Remove(issueId);
                            selectedFile.BCFComponents.Add(issueId, bcfComponent);

                            bcfDictionary.Remove(selectedFile.FileId);
                            bcfDictionary.Add(selectedFile.FileId, selectedFile);

                            DisplayUI();
                            comboBoxFile.SelectedIndex = fileIndex;
                            comboBoxIssue.SelectedIndex = issueIndex;

                            List<Comment> comments = (List<Comment>)dataGridComments.ItemsSource;
                            int commentIndex = comments.FindIndex(o => o.Guid == commentGuid);
                            dataGridComments.SelectedIndex = commentIndex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add an comment item.\n"+ex.Message, "Add Comment", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonEditComment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxFile.SelectedItem && null != comboBoxIssue.SelectedItem && null != dataGridComments.SelectedItem)
                {
                    CommentWindow commentWindow = new CommentWindow(selectedMarkup, selectedComment, CommentMode.EDIT);
                    if (commentWindow.ShowDialog() == true)
                    {
                        selectedMarkup = commentWindow.SelectedMarkup;
                        selectedComment = commentWindow.SelectedComment;
                        bool updated = CommentManager.EditComment(selectedComment, dbFile);

                        string commentGuid = selectedComment.Guid;
                        int fileIndex = comboBoxFile.SelectedIndex;
                        int issueIndex = comboBoxIssue.SelectedIndex;

                        string issueId = selectedMarkup.Topic.Guid;
                        if (selectedFile.BCFComponents.ContainsKey(issueId))
                        {
                            BCFComponent bcfComponent = selectedFile.BCFComponents[issueId];
                            bcfComponent.MarkupInfo = selectedMarkup;

                            selectedFile.BCFComponents.Remove(issueId);
                            selectedFile.BCFComponents.Add(issueId, bcfComponent);

                            bcfDictionary.Remove(selectedFile.FileId);
                            bcfDictionary.Add(selectedFile.FileId, selectedFile);

                            DisplayUI();
                            comboBoxFile.SelectedIndex = fileIndex;
                            comboBoxIssue.SelectedIndex = issueIndex;

                            List<Comment> comments = (List<Comment>)dataGridComments.ItemsSource;
                            int commentIndex = comments.FindIndex(o => o.Guid == commentGuid);
                            dataGridComments.SelectedIndex = commentIndex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to edit a comment item.\n" + ex.Message, "Edit Comment", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonDeleteComment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxFile.SelectedItem && null != comboBoxIssue.SelectedItem && null != dataGridComments.SelectedItem)
                {
                    bool deleted = CommentManager.DeleteComment(selectedComment, dbFile);
                    int commentIndex = selectedMarkup.Comment.FindIndex(o => o.Guid == selectedComment.Guid);
                    if (commentIndex > -1)
                    {
                        selectedMarkup.Comment.RemoveAt(commentIndex);
                    }

                    int fileIndex = comboBoxFile.SelectedIndex;
                    int issueIndex = comboBoxIssue.SelectedIndex;

                    string issueId = selectedMarkup.Topic.Guid;
                    if (selectedFile.BCFComponents.ContainsKey(issueId))
                    {
                        BCFComponent bcfComponent = selectedFile.BCFComponents[issueId];
                        bcfComponent.MarkupInfo = selectedMarkup;

                        selectedFile.BCFComponents.Remove(issueId);
                        selectedFile.BCFComponents.Add(issueId, bcfComponent);

                        bcfDictionary.Remove(selectedFile.FileId);
                        bcfDictionary.Add(selectedFile.FileId, selectedFile);

                        DisplayUI();
                        comboBoxFile.SelectedIndex = fileIndex;
                        comboBoxIssue.SelectedIndex = issueIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete an comment item.\n" + ex.Message, "Delete Comment", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        

    }
}