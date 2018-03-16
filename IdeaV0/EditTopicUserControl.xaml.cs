using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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

namespace IdeaV0
{
    /// <summary>
    /// Interaction logic for EditTopicUserControl.xaml
    /// </summary>
    public partial class EditTopicUserControl : RTBSupportingControlBase
    {
        private MainWindow theWindow = ((MainWindow)Application.Current.MainWindow);
        private SQLiteConnection dbConnection;
        private Topic unsavedTopic;
        public String oldTitle { get; }
        private Project parentProject;
        private List<String> tagsToAdd = new List<string>(); //contains tags that were not present before editing and now have been added
        private List<String> tagsToRemove = new List<String>(); //contains tags that were present before editing and now have been removed
        private List<String> currentTags = new List<String>(); //tags that are active in the UI
        private Dictionary<String, MenuItem> possibleTags = new Dictionary<String, MenuItem>(); //all tags that aren't currently part of the topic

        public EditTopicUserControl(Topic topic)
        {
            dbConnection = theWindow.dbConnection;
            parentProject = theWindow.currentPage.project;
            unsavedTopic = topic;
            oldTitle = topic.title;

            InitializeComponent();

            LoadTopicInfo();
            SideBar.Content = new SideBarUserControl(this);
            base.ConnectUIElements(boldButton, italicButton, underlineButton, fontTypeBox, fontSizeBox, TopicRichTextBox);
            FocusManager.SetFocusedElement(theWindow, TopicRichTextBox);
            TopicRichTextBox.Height = theWindow.ActualHeight / 1.37; //placeholder to make it look pretty on my screen
        }

        //Loads the information of the topic to be edited
        private void LoadTopicInfo()
        {
            //load title
            this.GivenTitle.SetCurrentValue(TextBox.TextProperty, unsavedTopic.title);
            //load main image
            this.MainImage.Source = this.unsavedTopic.main_image;
            if(MainImage.Source == null) { MainImage.Visibility = Visibility.Collapsed; }
            else { MainImageButton.Visibility = Visibility.Collapsed; }
            //load tags
            LoadTags();
            //load rich text
            byte[] byteArray = Encoding.ASCII.GetBytes(unsavedTopic.encoded_text);
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                TextRange tr = new TextRange(TopicRichTextBox.Document.ContentStart, TopicRichTextBox.Document.ContentEnd);
                tr.Load(ms, DataFormats.Rtf);
            }
        }

        //called by LoadTopicInfo
        private void LoadTags()
        {
            //these don't get added to tagsToAdd, so the project doesn't try to add them again
            foreach (String tag in parentProject.GetAssociatedTags(unsavedTopic.title)) 
            {
                GivenTags.Children.Add(MakeTagBlock(tag));
                currentTags.Add(tag);
            }
            //tags that can still be added go into context menu
            foreach (String tag in this.parentProject.GetTagList())
            {
                if(!currentTags.Contains(tag))
                {
                    MenuItem item = MakeTagMenuItem(tag);
                    TagOptionsDropDown.Items.Add(item);
                    possibleTags.Add(tag, item);
                }
            }
        }

        private TextBlock MakeTagBlock(String tag)
        {
            TextBlock tagUI = HelpFunctions.CreateTagBlock(tag);
            tagUI.ContextMenu = new ContextMenu();
            tagUI.ContextMenu.PlacementTarget = tagUI;
            tagUI.ContextMenu.Items.Add(new MenuItem());
            ((MenuItem)tagUI.ContextMenu.Items[0]).Header = "Remove";
            ((MenuItem)tagUI.ContextMenu.Items[0]).Click += RemoveTag_Clicked;
            return tagUI;
        }
        
        private MenuItem MakeTagMenuItem(String tag)
        {
            MenuItem item = new MenuItem();
            item.Header = tag;
            item.Click += AddTag;
            return item;
        }

        //adds tag to UI
        private void AddTag(Object sender, RoutedEventArgs e)
        {
            String tagName = ((sender as MenuItem).Header) as String;
            TextBlock tagBlock = MakeTagBlock(tagName);
            GivenTags.Children.Add(tagBlock);
            currentTags.Add(tagName);
            if (tagsToRemove.Contains(tagBlock.Text)) { tagsToRemove.Remove(tagBlock.Text); }
            else { tagsToAdd.Add(tagBlock.Text); }
            //remove from contextmenu
            TagOptionsDropDown.Items.Remove(possibleTags[tagName]);
            possibleTags.Remove(tagName);
        }

        //opens add tag context menu
        private void AddTag_Clicked(Object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsOpen = true;
        }

        //removes the tag from the UI
        private void RemoveTag_Clicked(Object sender, RoutedEventArgs e)
        {
            MenuItem m = sender as MenuItem;
            TextBlock tagBlock = (m.Parent as ContextMenu).PlacementTarget as TextBlock;
            String tagName = tagBlock.Text as String;
            GivenTags.Children.Remove(tagBlock);
            currentTags.Remove(tagName);
            if (tagsToAdd.Contains(tagName)) { tagsToAdd.Remove(tagName); }
            else { tagsToRemove.Add(tagName); }
            //add the tag back to the context menu
            MenuItem removedTag = MakeTagMenuItem(tagName);
            TagOptionsDropDown.Items.Add(removedTag);
            possibleTags.Add(tagName, removedTag);
        }

        private void MainImage_Clicked(Object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png, *.jpg, *.bmp)|*.png;*.jpg*;.bmp;|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage image = new BitmapImage(new Uri(openFileDialog.FileName));
                //image.CacheOption = BitmapCacheOption.OnLoad;
                MainImage.Source = image;
                if(MainImage.Visibility.Equals(Visibility.Collapsed))
                {
                    MainImage.Visibility = Visibility.Visible;
                    MainImageButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        //cancels editing and returns to the ViewTopic page
        private void Cancel_Clicked(Object sender, RoutedEventArgs e)
        {
            theWindow.RemoveOldPageVersion(); //remove the previous ViewTopic from history
            theWindow.NavigateToPage(new WoPage(new ViewTopicUserControl(unsavedTopic), theWindow.currentPage.project));
        }
        
        private void SaveTopic_CanExecute(Object sender, CanExecuteRoutedEventArgs e)
        {
            if (!HelpFunctions.StringIsBlank(GivenTitle.Text)) { e.CanExecute = true; }
            else { e.CanExecute = false; }
        }

        //save/update the Topic and return to the ViewTopic page
        private void SaveTopic_Executed(Object sender, RoutedEventArgs e)
        {
            //get new title
            unsavedTopic.title = GivenTitle.Text;
            //get new main image
            unsavedTopic.main_image = MainImage.Source as BitmapImage;
            //get new body
            TextRange range = new TextRange(TopicRichTextBox.Document.ContentStart, TopicRichTextBox.Document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                range.Save(ms, DataFormats.Rtf); //saves the formatting of the text
                unsavedTopic.encoded_text = Encoding.ASCII.GetString(ms.ToArray());
            }

            //SAVE CHANGES .... Should try multithreading this
            parentProject.EditTopic(oldTitle, unsavedTopic);
            //Save tag connections
            parentProject.UpdateTopicTags(unsavedTopic.title, tagsToAdd, tagsToRemove);

            //remove the old version of the page from history
            theWindow.RemoveOldPageVersion();

            //navigate to topic
            theWindow.NavigateToPage(new ViewTopicUserControl(theWindow.currentPage.project.GetTopic(unsavedTopic.title)), theWindow.currentPage.project);
        }
    }

}