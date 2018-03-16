using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for ProjectHomeUserControl.xaml
    /// </summary>
    public partial class ProjectHomeUserControl : UserControl
    {
        private MainWindow theWindow = ((MainWindow)Application.Current.MainWindow);
        private SQLiteConnection dbConnection;
        internal Project project { get; }
        private Dictionary<String, TextBlock> topicList = new Dictionary<string, TextBlock>();
        private Dictionary<String, ToggleButton> tagsList = new Dictionary<String, ToggleButton>();
        //Could bind these to thier respective toggles, but...
        private Boolean andOperator = false;
        private Boolean orOperator = false;

        //displays homepage for the given project
        //the applications's current project should always be set to the given
        //project immediately after the ProjectHomeUserControl is constructed (see MainWindow.NavigateToProject())
        public ProjectHomeUserControl(Project givenProject)
        {
            dbConnection = theWindow.dbConnection;
            project = givenProject;
            //sets event handler for when this page becomes visible
            this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(this.OnPageVisible);
            InitializeComponent();
            SideBar.Content = new SideBarUserControl(this);
            FilterResultScrollViewer.Height = theWindow.ActualHeight / 1.55; //dear god
        }


        /** UI Updating Functionality **/

        //Reloads UI elements whenever the page becomes visible again in case something changed
        private void OnPageVisible(Object sender, DependencyPropertyChangedEventArgs e)
        {
            if(this.IsVisible) //update the UI source
            {
                //set title
                ProjectTitle.SetCurrentValue(TextBlock.TextProperty, project.title);
                //set subtitle
                ProjectSubtitle.SetCurrentValue(TextBlock.TextProperty, project.subtitle);
                if(HelpFunctions.StringIsBlank(ProjectSubtitle.Text)) { ProjectSubtitle.Visibility = Visibility.Collapsed; }
                else { ProjectSubtitle.Visibility = Visibility.Visible; }
                //update topic list
                UpdateTopicList();
                //update tag list
                UpdateTagList();
                //set default filter mode and clear tag selection
                SetDefaultFiltering();
                //update future elements...
            }
        }
        
        //Display every topic in this project in UI
        private void UpdateTopicList()
        {
            //add topics
            foreach (String topic in project.GetTopicList())
            {
                if(!topicList.ContainsKey(topic))
                {
                    AddTopicToUI(topic);
                }
            }
            //Remove topics that have been deleted
            List<String> oldTopics = new List<String>();
            foreach(String topic in this.topicList.Keys)
            {
                if (!this.project.ContainsTopic(topic))
                {
                    oldTopics.Add(topic);
                    TopicPanel.Children.Remove(topicList[topic]);
                }
            }
            //Removed afterwards to avoid modifiying the list while iterating through it
            foreach (String topic in oldTopics)
            {
                topicList.Remove(topic);
            }
        }

        //Adds a textblock representing a topic to the UI; links to the topic's page
        private void AddTopicToUI(String topic)
        {
            TextBlock topicEntry = new TextBlock();
            //appearance
            topicEntry.Text = topic;
            topicEntry.Margin = new Thickness(5);
            topicEntry.Foreground = Brushes.Blue;
            topicEntry.FontSize = 20;
            topicEntry.TextDecorations = TextDecorations.Underline;
            //funtionality
            topicEntry.PreviewMouseLeftButtonUp += OpenTopic_Clicked;
            topicEntry.Cursor = Cursors.Hand;
            //add the textblock to the UI
            topicList.Add(topic, topicEntry);
            TopicPanel.Children.Add(topicEntry);
        }

        //Display every tag in this project in UI
        private void UpdateTagList()
        {
            foreach(String tag in this.project.GetTagList())
            {
                if(!this.tagsList.Keys.Contains(tag))
                {
                    AddTagToUI(tag);
                }
            }
            //remove old tags that were deleted
            List<String> oldTags = new List<String>();
            foreach(String tag in this.tagsList.Keys)
            {
                if(!this.project.ContainsTag(tag))
                {
                    oldTags.Add(tag);
                    TagsPanel.Children.Remove(this.tagsList[tag]);
                }
            }
            //Removed afterwards to avoid modifiying the list while iterating through it
            foreach(String tag in oldTags)
            {
                this.tagsList.Remove(tag);
            }
        }

        //Adds a togglebutton representing a tag to the UI
        private void AddTagToUI(String tag)
        {
            ToggleButton tagUI = HelpFunctions.CreateTagToggle(tag);
            //right click context menu
            tagUI.ContextMenu = new ContextMenu();
            tagUI.ContextMenu.PlacementTarget = tagUI;
            tagUI.ContextMenu.Items.Add(new MenuItem());
            ((MenuItem)tagUI.ContextMenu.Items[0]).Header = "Add Topics";
            ((MenuItem)tagUI.ContextMenu.Items[0]).Click += AddTopicsToTag_Clicked;
            tagUI.ContextMenu.Items.Add(new MenuItem());
            ((MenuItem)tagUI.ContextMenu.Items[1]).Header = "Remove Topics";
            ((MenuItem)tagUI.ContextMenu.Items[1]).Click += RemoveTopicsFromTag_Clicked;
            tagUI.ContextMenu.Items.Add(new MenuItem());
            ((MenuItem)tagUI.ContextMenu.Items[2]).Header = "Delete Tag";
            ((MenuItem)tagUI.ContextMenu.Items[2]).Click += DeleteTag_Clicked;
            //event handlers for sorting
            tagUI.Checked += Tag_SelectionChanged;
            tagUI.Unchecked += Tag_SelectionChanged;
            //add to UI
            this.tagsList.Add(tag, tagUI);
            TagsPanel.Children.Add(tagUI);
        }

        
        /* Button Functionality */
        
        //unselects all tags
        private void ClearFilters_Clicked(Object sender, RoutedEventArgs e)
        {
            ClearTagFilters();
        }
        
        //Brings the user to the new topic page
        private void NewTopic_Clicked(Object sender, RoutedEventArgs e)
        {
            theWindow.NavigateToPage(new NewTopicUserControl(), theWindow.currentPage.project);
        }

        private void NewTag_Clicked(Object sender, RoutedEventArgs e)
        {
            if(!NewTagInputBox.Visibility.Equals(Visibility.Visible))
            {
                NewTagInputBox.Visibility = Visibility.Visible;
                Keyboard.Focus(NewTagInputBox);
            }
            else
            {
                if (!HelpFunctions.StringIsBlank(NewTagInputBox.Text))
                {
                    this.project.AddTag(NewTagInputBox.Text);
                    UpdateTagList();
                    NewTagInputBox.Visibility = Visibility.Collapsed;
                    NewTagInputBox.Text = "";
                }
            }
        }

        //opens the highlighted topic
        private void OpenTopic_Clicked(Object sender, RoutedEventArgs e)
        {
            if(sender is TextBlock)
            {
                Topic topicToBeOpened;
                topicToBeOpened = this.project.GetTopic(((TextBlock)sender).Text);
                theWindow.NavigateToPage(new ViewTopicUserControl(topicToBeOpened), theWindow.currentPage.project);
            }
        }

        //deletes a tag from the project
        private void DeleteTag_Clicked(Object sender, RoutedEventArgs e)
        {
            MessageBoxResult mb = MessageBox.Show("Are you sure you want to delete this tag? This action cannot be undone.",
                "", MessageBoxButton.YesNo);
            switch (mb)
            {
                case MessageBoxResult.Yes:
                    MenuItem m = sender as MenuItem;
                    ToggleButton tag = (m.Parent as ContextMenu).PlacementTarget as ToggleButton;
                    String tagName = tag.Content as String;
                    //remove from UI
                    TagsPanel.Children.Remove(tag);
                    tagsList.Remove(tagName);
                    //remove from memory/db
                    this.project.RemoveTag(tagName);
                    break;
            }
        }

        //deletes the project completely.
        private void DeleteProject_Clicked(Object sender, RoutedEventArgs e)
        {
            MessageBoxResult mb = MessageBox.Show("Are you sure you want to delete this project? This action cannot be undone.",
                "", MessageBoxButton.YesNo);
            switch(mb)
            {
                case MessageBoxResult.Yes:
                    theWindow.DeleteProject(theWindow.currentPage.project.title);
                    theWindow.ClearHistory();
                    theWindow.NavigateToPage(new WoPage(new AppNexusUserControl(), null));
                    break;
            }
        }
        
        //opens popup to associate topics with a tag
        private void AddTopicsToTag_Clicked(Object sender, RoutedEventArgs e)
        {
            MenuItem m = sender as MenuItem;
            ToggleButton tag = (m.Parent as ContextMenu).PlacementTarget as ToggleButton;
            String tagName = tag.Content as String;
            TTAssociationWindow addTopicsPopup = new TTAssociationWindow(tagName, this.project, true);
            addTopicsPopup.WindowStartupLocation = WindowStartupLocation.Manual;
            addTopicsPopup.Left = System.Windows.Forms.Control.MousePosition.X;
            addTopicsPopup.Top = System.Windows.Forms.Control.MousePosition.Y;
            addTopicsPopup.Show();
        }

        private void RemoveTopicsFromTag_Clicked(Object sender, RoutedEventArgs e)
        {
            MenuItem m = sender as MenuItem;
            ToggleButton tag = (m.Parent as ContextMenu).PlacementTarget as ToggleButton;
            String tagName = tag.Content as String;
            TTAssociationWindow removeTopicsPopup = new TTAssociationWindow(tagName, this.project, false);
            removeTopicsPopup.WindowStartupLocation = WindowStartupLocation.Manual;
            removeTopicsPopup.Left = System.Windows.Forms.Control.MousePosition.X;
            removeTopicsPopup.Top = System.Windows.Forms.Control.MousePosition.Y;
            removeTopicsPopup.Show();
        }


        /* Tag Filtering Functionality and Operations */
        
        //Filters topic list based on selected tags and the operator
        private void FilterTopicList(List<String> selectedTags)
        {
            if(andOperator && orOperator) { throw new Exception("Multiple filtering operator exception."); }

            this.topicList.Clear();
            TopicPanel.Children.Clear();

            //When no tags are selected, display all topics regardless of filter mode
            if (selectedTags.Count == 0)
            {
                foreach(String topic in this.project.GetTopicList())
                {
                    AddTopicToUI(topic);
                }
            }
            else if (orOperator)
            {
                foreach(String tag in selectedTags)
                {
                    foreach(String topic in this.project.GetAssociatedTopics(tag))
                    {
                        if(!this.topicList.Keys.Contains(topic))
                        {
                            AddTopicToUI(topic);  //the order in the UI will depend on the order the tags appear in, not the topics
                        }
                    }
                }
            }
            else if(andOperator)
            {
                foreach(String topic in this.project.GetTopicList())
                {
                    Boolean hasAlltags = true;
                    foreach (String tag in selectedTags)
                    {
                        if (!this.project.GetAssociatedTags(topic).Contains(tag)) { hasAlltags = false; break; }
                    }
                    if(hasAlltags) { AddTopicToUI(topic); }
                }
            }
        }
        
        //returns the names of the tags that are selected
        private List<String> GetSelectedTags()
        {
            List<String> selectedTags = new List<String>();
            foreach (String tag in this.tagsList.Keys)
            {
                if (tagsList[tag].IsChecked == true)
                {
                    selectedTags.Add(tag);
                }
            }
            return selectedTags;
        }
        
        //Updates filter results whenever a tag is selected/deselected
        private void Tag_SelectionChanged(object sender, RoutedEventArgs e)
        {
            FilterTopicList(GetSelectedTags());
        }
        
        //click function for the OR filter
        private void OrToggle_On(object sender, RoutedEventArgs e)
        {
            //Only one toggle can be on at a time
            if (AndToggle.IsChecked == true) { AndToggle.IsChecked = false; andOperator = false; }
            orOperator = true;
            OrToggle.IsChecked = true;

            FilterTopicList(GetSelectedTags());
        }
        
        //click function for the AND filter
        private void AndToggle_On(object sender, RoutedEventArgs e)
        {
            //Only one toggle can be on at a time
            if (OrToggle.IsChecked == true) { OrToggle.IsChecked = false; orOperator = false; }
            andOperator = true;
            AndToggle.IsChecked = true;
            FilterTopicList(GetSelectedTags());
        }

        //Turns off all tag sorting filters
        private void ClearTagFilters()
        {
            foreach(ToggleButton tag in TagsPanel.Children)
            {
                tag.IsChecked = false;
            }
        }

        //default filter mode is OR
        private void SetDefaultFiltering()
        {
            orOperator = true;
            OrToggle.IsChecked = true;
            andOperator = false;
            AndToggle.IsChecked = false;
            ClearTagFilters();
        }

    }

}
