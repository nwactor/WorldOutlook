using System;
using System.Collections.Generic;
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
    /// Interaction logic for SideBarUserControl.xaml
    /// </summary>
    public partial class SideBarUserControl : UserControl
    {
        private MainWindow theWindow = ((MainWindow)Application.Current.MainWindow);
        private UserControl parentControl;

        public SideBarUserControl()
        {
            InitializeComponent();
        }

        public SideBarUserControl(UserControl parentControl)
        {
            this.parentControl = parentControl;
            InitializeComponent();
            //populate the sidebar based on
            //what kind of page it is in
            if(parentControl is AppNexusUserControl)
            {
                FillAppNexusSideBar();
            }
            else if(parentControl is ProjectHomeUserControl)
            {
                TextBlock heading = new TextBlock();
                heading.Text = "Other projects:";
                heading.Margin = new Thickness(5, 0, 0, 0);
                Bar.Children.Add(heading);
                StackPanel OtherProjects = new StackPanel();
                OtherProjects.Margin = new Thickness(7, 5, 0, 0);
                Bar.Children.Add(OtherProjects);
                FillProjectSideBar(OtherProjects);
            }
            else if(parentControl is ViewTopicUserControl || parentControl is NewTopicUserControl || parentControl is EditTopicUserControl)
            {
                TextBlock projectBlock = new TextBlock();
                projectBlock.Text = theWindow.currentPage.project.title;
                projectBlock.FontSize = 20;
                projectBlock.Margin = new Thickness(5,2,0,2);
                projectBlock.HorizontalAlignment = HorizontalAlignment.Center;
                projectBlock.TextDecorations = TextDecorations.Underline;
                projectBlock.TextWrapping = TextWrapping.Wrap;
                projectBlock.Cursor = Cursors.Hand;
                projectBlock.MouseUp += LinkToCurrentProject;
                Bar.Children.Add(projectBlock);

                Bar.Children.Add(new Separator());

                TextBlock heading = new TextBlock();
                heading.Text = "Other Topics in " + theWindow.currentPage.project.title + ":";
                heading.Margin = new Thickness(5,0,0,0);
                Bar.Children.Add(heading);
                StackPanel OtherTopics = new StackPanel();
                OtherTopics.Margin = new Thickness(7,5,0,0);
                Bar.Children.Add(OtherTopics);
                FillTopicSideBar(OtherTopics);
            }
            else
            {
                
            }
        }

        private void FillAppNexusSideBar()
        {

        }

        private void FillProjectSideBar(StackPanel OtherProjects)
        {
            List<String> appProjects = new List<string>();
            foreach(String projectName in theWindow.appProjects)
            {
                if (!projectName.Equals((parentControl as ProjectHomeUserControl).project.title)) //don't give a link to the topic that's already displayed
                {
                    WrapPanel cursorWrap = new WrapPanel();
                    TextBlock projectLink = HelpFunctions.CreateSideBarLink(projectName);
                    projectLink.Cursor = Cursors.Hand;
                    projectLink.MouseUp += LinkToTargetProject;
                    cursorWrap.Children.Add(projectLink);
                    OtherProjects.Children.Add(cursorWrap);
                }
            }
        }

        private void FillTopicSideBar(StackPanel OtherTopics)
        {
            List<String> projectTopics = theWindow.currentPage.project.GetTopicList();
            foreach (String topicName in projectTopics)
            {
                String title = "";
                if(parentControl is ViewTopicUserControl) { title = (parentControl as ViewTopicUserControl).topic.title; }
                else if (parentControl is EditTopicUserControl) { title = (parentControl as EditTopicUserControl).oldTitle; }
                //NewTopicUserControl doesn't have a title yet  

                if(!topicName.Equals(title)) //don't give a link to the topic that's already displayed
                {
                    WrapPanel cursorWrap = new WrapPanel(); //makes the space next to the topic link not accidentally clickable
                    TextBlock topicLink = HelpFunctions.CreateSideBarLink(topicName);
                    topicLink.Cursor = Cursors.Hand;
                    topicLink.MouseUp += LinkToTopic;
                    cursorWrap.Children.Add(topicLink);
                    OtherTopics.Children.Add(cursorWrap);
                }                
            }
        }

        private void UpdateTopicSideBar()
        {

        }

        public void LinkToAppHome(Object sender, RoutedEventArgs e)
        {
            WoPage page = new WoPage(new AppNexusUserControl(), null);
            theWindow.NavigateToPage(page);
        }

        //Doesn't require a DB call
        public void LinkToCurrentProject(Object sender, RoutedEventArgs e)
        {
            Project project = theWindow.currentPage.project;
            theWindow.NavigateToProject(project);
        }

        //requires a DB call
        public void LinkToTargetProject(Object sender, RoutedEventArgs e)
        {
            String projectTitle = (sender as TextBlock).Text;
            theWindow.NavigateToProject(projectTitle);
        }

        public void LinkToTopic(Object sender, RoutedEventArgs e)
        {
            String topicTitle = (sender as TextBlock).Text;
            Project project = theWindow.currentPage.project;
            Topic topic = project.GetTopic(topicTitle);
            WoPage page = new WoPage(new ViewTopicUserControl(topic), project);
            theWindow.NavigateToPage(page);
        }

        //for use by other userControls
        public void AddSideBarText(String text)
        {
            Bar.Children.Add(new TextBlock());
        }

        
    }
}
