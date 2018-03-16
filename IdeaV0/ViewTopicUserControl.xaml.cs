using System;
using System.Collections.Generic;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IdeaV0
{
    /// <summary>
    /// Interaction logic for ViewTopicUserControl.xaml
    /// </summary>
    public partial class ViewTopicUserControl : UserControl
    {
        private MainWindow theWindow = ((MainWindow)Application.Current.MainWindow);
        private SQLiteConnection dbConnection;
        public Topic topic { get; }

        //open a topic already loaded in memory
        public ViewTopicUserControl(Topic topicToLoad)
        {
            dbConnection = theWindow.dbConnection;
            this.topic = topicToLoad;
            InitializeComponent();
            DisplayTopicInfo();
            SideBar.Content = new SideBarUserControl(this);
        }

        /*public ViewTopicUserControl(String titleOfLoadingTopic)
        {
        //open a topic from its title (from memory if possible, if not goes to db)
        //LOADING FROM DB not implemented; since method should probably never be called
        //should never be called because if a topic exists it should always be in memory
        //when this constructor has the potential to be called
            dbConnection = theWindow.dbConnection;

            if (theWindow.currentPage.project.ContainsTopic(titleOfLoadingTopic)) 
            {
                //load from memory
                new ViewTopicUserControl(theWindow.currentPage.project.GetTopic(titleOfLoadingTopic));
            }
            else
            {
                //load from db

                InitializeComponent();
            }
        }
        */

        //shows the title, body, images, and tags
        private void DisplayTopicInfo()
        {
            //display title
            TopicTitle.SetCurrentValue(TextBlock.TextProperty, topic.title);

            //display rich text
            TopicTextScrollViewer.Document = new FlowDocument();
            byte[] byteArray = Encoding.ASCII.GetBytes(topic.encoded_text);
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                TextRange tr = new TextRange(TopicTextScrollViewer.Document.ContentStart, TopicTextScrollViewer.Document.ContentEnd);
                tr.Load(ms, DataFormats.Rtf);
            }

            //display image
            MainImage.Source = topic.main_image;//new BitmapImage(new Uri("D:/Nick/Pictures/photo.jpg"));

            //display gallery

            //display tags
            LoadTags();
        }

        //called by DisplayTopicInfo
        private void LoadTags()
        {
            foreach (String tag in theWindow.currentPage.project.GetAssociatedTags(this.topic.title))
            {
                TagsPanel.Children.Add(HelpFunctions.CreateTagBlock(tag));
            }
        }

        //Open the edit topic page
        public void EditButton_Clicked(Object sender, RoutedEventArgs e)
        {
            theWindow.NavigateToPage(new EditTopicUserControl(this.topic), theWindow.currentPage.project);
        }

        //calls project.RemoveTopic, removes the page from history, and navigates away
        public void DeleteButton_Clicked(Object sender, RoutedEventArgs e)
        {
            MessageBoxResult mb = MessageBox.Show("Are you sure you want to delete this project? This action cannot be undone.", "", MessageBoxButton.YesNo);
            switch (mb)
            {
                case MessageBoxResult.Yes:
                    theWindow.currentPage.project.RemoveTopic(this.topic.title);
                    theWindow.NavigateToPage(new WoPage(new ProjectHomeUserControl(theWindow.currentPage.project), theWindow.currentPage.project));
                    theWindow.ClearHistory();
                    break;
            }
        }
    }

    public class TopicContentViewer : FlowDocumentScrollViewer
    {
        public TopicContentViewer() : base() {
            //this.MouseWheel -= UIElement.OnMouseWheel;
        }
        public void blah(Object sedner, MouseWheelEventArgs e) { e.Handled = false;}
    }
}
