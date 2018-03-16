using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SQLite;
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
using Microsoft.Win32;

namespace IdeaV0
{
    /// <summary>
    /// Interaction logic for NewTopicUserControl.xaml
    /// </summary>
    public partial class NewTopicUserControl : RTBSupportingControlBase
    {
        private MainWindow theWindow = ((MainWindow)Application.Current.MainWindow);
        private SQLiteConnection dbConnection;
        private Project parentProject;
        private Topic unsavedTopic = new Topic();
        private Dictionary<String, TextBlock> tagsToAdd = new Dictionary<string, TextBlock>(); //tags that will be added
        private Dictionary<String, MenuItem> possibleTags = new Dictionary<string, MenuItem>(); //tags that can be added

        public NewTopicUserControl()
        {
            dbConnection = theWindow.dbConnection;
            this.parentProject = theWindow.currentPage.project;
            unsavedTopic.parent_project = this.parentProject.title;
            this.Loaded += new RoutedEventHandler(this.OnLoad);

            InitializeComponent();

            SideBar.Content = new SideBarUserControl(this);
            PopulateTagsDropDownList();
            base.ConnectUIElements(boldButton, italicButton, underlineButton, fontTypeBox, fontSizeBox, TopicRichTextBox);
            this.MainImage.Visibility = Visibility.Collapsed;
        }

        //Pete Huber's answer in https://stackoverflow.com/questions/1345391/set-focus-on-textbox-in-wpf
        //Sets focus on the name textbox
        private void OnLoad(Object sender, RoutedEventArgs e)
        {
            //No idea why this class needs this here and not so in other classes
            FocusManager.SetFocusedElement(this, this.GivenTitleTextBox);
        }

        private void PopulateTagsDropDownList()
        {
            foreach(String tag in this.parentProject.GetTagList())
            {
                MenuItem item = new MenuItem();
                item.Header = tag;
                item.Click += AddTag;
                TagOptionsDropDown.Items.Add(item);
                possibleTags.Add(tag, item);
            }
        }

        private void AddTag(Object sender, RoutedEventArgs e)
        {
            String tagName = ((sender as MenuItem).Header) as String;
            TextBlock tagBlock = HelpFunctions.CreateTagBlock(tagName);
            TagsPanel.Children.Add(tagBlock);
            tagsToAdd.Add(tagName, tagBlock);
            //remove from contextmenu
            TagOptionsDropDown.Items.Remove(possibleTags[tagName]);
        }
        private void AddTag_Clicked(Object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsOpen = true;
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
                if (MainImage.Visibility.Equals(Visibility.Collapsed))
                {
                    MainImage.Visibility = Visibility.Visible;
                    MainImageButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void CreateTopic_CanExecute(Object sender, CanExecuteRoutedEventArgs e)
        {
            if (HelpFunctions.StringIsBlank(GivenTitleTextBox.Text))
            {
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = true;
            }
        }

        private void CreateTopic_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CreateAndSaveTopic();
        }

        private void CreateAndSaveTopic()
        {
            unsavedTopic.title = GivenTitleTextBox.Text;

            //check that the topic can be added
            SQLiteCommand command = dbConnection.CreateCommand();            
            command.CommandText = "select title from UserTopics " +
                "where title = (@title) and parent_project = (@parent)";
            command.Parameters.AddWithValue("@title", unsavedTopic.title);
            command.Parameters.AddWithValue("@parent", unsavedTopic.parent_project);

            dbConnection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read()) // read() returns true if a project of that title already exists
            {
                MessageBox.Show("Topic already exists.");
                reader.Close();
                dbConnection.Close();
            }
            else
            {
                reader.Close();
                dbConnection.Close();

                //set the image
                unsavedTopic.main_image = MainImage.Source as BitmapImage;

                //convert RichTextBox to ASCII encoding (we'll see how the performance is)
                //source: https://stackoverflow.com/questions/15983278/storing-data-of-rich-text-box-to-database-with-formatting
                TextRange range = new TextRange(TopicRichTextBox.Document.ContentStart, TopicRichTextBox.Document.ContentEnd);
                using (MemoryStream ms = new MemoryStream())
                {
                    range.Save(ms, DataFormats.Rtf); //saves the formatting of the text
                    unsavedTopic.encoded_text = Encoding.ASCII.GetString(ms.ToArray());
                }

                //save the topic
                this.parentProject.AddTopic(unsavedTopic);
                //save the tag associations
                this.parentProject.UpdateTopicTags(unsavedTopic.title, new List<String>(tagsToAdd.Keys), null);

                //open the now-saved topic!
                theWindow.NavigateToPage(new ViewTopicUserControl(unsavedTopic), theWindow.currentPage.project); //go to the new topic
            }
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            theWindow.ReturnToPrevPage();
        }
    }
}
