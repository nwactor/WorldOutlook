using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal SQLiteConnection dbConnection { get; set; }
        internal WoPage currentPage;
        private Stack<WoPage> prevPages = new Stack<WoPage>();
        private Stack<WoPage> forwardPages = new Stack<WoPage>();
        public HashSet<String> appProjects = new HashSet<String>();

        //constructor
        public MainWindow()
        {
            InitializeDB();
            LoadProjectList();
            InitializeComponent();

            if (currentPage.project == null)
            {
                woDisplayedContent.Content = new AppNexusUserControl();
            }
            else
            {
                //open to the homepage of the current project
                woDisplayedContent.Content = new ProjectHomeUserControl(currentPage.project);
            }

            currentPage.control = (UserControl)woDisplayedContent.Content;
        }


        /** Database Management **/

        //Initializes the database on startup
        private void InitializeDB()
        {
            dbConnection = new SQLiteConnection("Data Source=WOdatabase.sqlite;Version=3;");
            dbConnection.Open();
            //create database if it doesn't exist (first time launch)
            //fewer tables with more rows make for far more efficient databases: https://stackoverflow.com/questions/21273672/many-tables-or-rows-which-one-is-more-efficient-in-sql
            SQLiteCommand command = dbConnection.CreateCommand();
            
            //Projects table
            //decided not to support projects with identical names
            //if i do decide to do that again, this time specify "autoincrement" on projectID
            command.CommandText = "create table if not exists UserProjects (" +
                "title varChar(100), " +
                "subtitle varChar(100), " +
                "background_image blob, " +
                "primary key(title) " +
                ")";
            command.ExecuteNonQuery();
            //Topics table
            command.CommandText = "create table if not exists UserTopics (" +
                "parent_project varChar(100), " +
                "title varChar(100), " +
                "text_body blob, " +
                "main_image blob, " +
                "image_gallery blob, " +
                "primary key(parent_project, title), " +
                "foreign key(parent_project) references UserProjects(title)" +
                ")";
            command.ExecuteNonQuery();
            //Tags table
            command.CommandText = "create table if not exists UserTags (" +
                "parent_project varChar(100), " +
                "name varChar(100), " +
                "primary key(parent_project, name), " +
                "foreign key(parent_project) references UserProjects(title)" +
                ")";
            command.ExecuteNonQuery();
            // Join/Junction/Intersection table for topics and tags
            command.CommandText = "create table if not exists TopicTags (" +
                "parent_project varChar(100), " +
                "topic_title varChar(100), " +
                "tag_name varChar(100), " +
                "primary key(parent_project, topic_title, tag_name), " +
                "foreign key(parent_project) references UserProjects(title), " +
                "foreign key(topic_title) references UserTopics(title), " +
                "foreign key(tag_name) references UserTags(name)" +
                ")";
            command.ExecuteNonQuery();
            //populate project list with project names
            command.CommandText = "select title from UserProjects";
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                //Console.Write(reader.GetString(0) + "\n");
            }
            reader.Close();

            dbConnection.Close();
        }

        //loads all projects from DB when app is opened
        private void LoadProjectList()
        {
            SQLiteCommand command = dbConnection.CreateCommand();
            command.CommandText = "select title from UserProjects";
            dbConnection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                appProjects.Add(reader.GetString(0));
            }
            reader.Close();
            dbConnection.Close();
        }

        //Deletes the project with the given title
        public void DeleteProject(String projectToDelete)
        {
            dbConnection.Open();
            SQLiteCommand command = dbConnection.CreateCommand();
            command.CommandText = "delete from UserProjects where title = '" + projectToDelete + "'";
            command.ExecuteNonQuery();
            command.CommandText = "delete from UserTopics where parent_project = '" + projectToDelete + "'";
            command.ExecuteNonQuery();
            command.CommandText = "delete from UserTags where parent_project = '" + projectToDelete + "'";
            command.ExecuteNonQuery();
            command.CommandText = "delete from TopicTags where parent_project = '" + projectToDelete + "'";
            command.ExecuteNonQuery();
            dbConnection.Close();
            appProjects.Remove(projectToDelete);
        }

        //Given an SQL select command, loads simple data from DB
        private String LoadString(String sqlCommandText)
        {
            dbConnection.Open();
            Object loadedData;

            SQLiteCommand command = dbConnection.CreateCommand();
            command.CommandText = sqlCommandText;
            loadedData = command.ExecuteScalar();

            dbConnection.Close();
            return (String) loadedData;
        }


        /**Application Navigation Functionality**/

        //updates the navigation history; called when a new page is visited
        private void UpdateHistoryNew(WoPage newpage)
        {
            //Don't add pages with unsaved input to history
            if (! (currentPage.control is NewTopicUserControl || currentPage.control is EditTopicUserControl))
            {
                prevPages.Push(currentPage);
            }
            currentPage = newpage;
            //clear the forward history, since a new path has been entered
            forwardPages.Clear();
        }

        //clears the history
        public void ClearHistory()
        {
            prevPages.Clear();
            forwardPages.Clear();
        } 

        //removes the last page from history
        //used when a page has been edited to remove the old version of the page from history
        //(pages with unsaved input never go to history anyway)
        public void RemoveOldPageVersion()
        {
            prevPages.Pop();
        }

        public void ReturnToPrevPage()
        {
            currentPage = prevPages.Pop();
            woDisplayedContent.Content = currentPage.control;
        }

        private void ForwardButton_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (forwardPages.Count > 0) e.CanExecute = true;
            }

        //updates the navigation history; called when forward button is clicked
        private void ForwardButton_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            prevPages.Push(currentPage);
            currentPage = forwardPages.Pop();
            woDisplayedContent.Content = currentPage.control;
        }

        private void BackButton_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (prevPages.Count > 0) e.CanExecute = true;
        }

        //updates the navigation history; called when back button is clicked
        private void BackButton_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            forwardPages.Push(currentPage);
            currentPage = prevPages.Pop();
            woDisplayedContent.Content = currentPage.control;
        }

        //navigates to a WoPage
        public void NavigateToPage(WoPage newpage)
        {
            woDisplayedContent.Content = newpage.control;
            UpdateHistoryNew(newpage);
        }

        //navigates to a WoPage without the caller having to provide a WoPage struct
        public void NavigateToPage(UserControl control, Project project)
        {
            WoPage newpage = new WoPage();
            newpage.control = control;
            newpage.project = project;
            NavigateToPage(newpage);
            TheWindowScrollBar.ScrollToTop();
        }

        //opens up the home page of project using title
        public void NavigateToProject(String projectTitle)
        {
            NavigateToProject(new Project(projectTitle));
        }

        //navigate to a project using a project object
        public void NavigateToProject(Project project)
        {
            String projectTitle = project.title;
            if(!appProjects.Contains(projectTitle)) { appProjects.Add(projectTitle); } //for newly created projects


            //load fields of project from DB if they are not already in memory
            if(project.subtitle == null)
            {
                //currentProject.subtitle = LoadString("select subtitle from UserProjects where title = '" + projectTitle + "'");
            }
            if (project.background_image == null)
            {
                //currentProject.background_image = (Image)LoadProjectData("select background_image from UserProjects where title = '" + projectTitle + "'");
            }
            project.LoadTopicsIfNeeded();
            project.LoadTagsIfNeeded();
            project.LoadConnectionsIfNeeded();

            //Can't call navigate to page because a different order of events is required
            WoPage newpage = new WoPage();
            newpage.project = project;
            newpage.control = new ProjectHomeUserControl(newpage.project);
            UpdateHistoryNew(newpage);
            woDisplayedContent.Content = newpage.control;
        }


        /**Menu Bar Functionality**/

        private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        //called to close the application
        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            dbConnection.Close();
            Application.Current.Shutdown();
        }

        //true if current page is not the neutral "appNexus"
        private void NewTopic_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if(currentPage.control is AppNexusUserControl)
                {
                    e.CanExecute = false;
                }
                else
                {
                    e.CanExecute = true;
                }
        }

        private void NewTopicCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            WoPage newpage = new WoPage();
            newpage.project = currentPage.project;
            newpage.control = new NewTopicUserControl();
            NavigateToPage(newpage);
        }

        //matching 'executed' method not implemented because it's handled by the child controls
        private void SaveTopicCommand_CanExecute(Object sender, CanExecuteRoutedEventArgs e)
        {
            if(currentPage.control is NewTopicUserControl || currentPage.control is EditTopicUserControl)
            {
                e.CanExecute = true;
            }
        }

        private void NewProject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        //opens the new project window
        private void NewProjectCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Window newProjectWindow = new CreateNewProjectWindow();
            newProjectWindow.Show();
        }

        private void OpenProject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        //opens the open project window
        private void OpenProjectWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Window opwindow = new OpenProjectWindow();
            opwindow.Show();
        }
    }


    public static class CustomCommands
    {
            
        /* System Commands */

        public static readonly RoutedUICommand Exit = new RoutedUICommand
            (
                "Exit",
                "Exit",
                 typeof(CustomCommands),
                    new InputGestureCollection()
                    {
                        new KeyGesture(Key.F4, ModifierKeys.Alt)
                    }
            );

        public static readonly RoutedUICommand Back = new RoutedUICommand
            (
                "Back",
                "Back",
                 typeof(CustomCommands)
            );

        public static readonly RoutedUICommand Forward = new RoutedUICommand
            (
                "Forward",
                "Forward",
                 typeof(CustomCommands)
            );


        /* Topic Level Commands */

        public static readonly RoutedUICommand BeginNewTopic = new RoutedUICommand
            (
                "Begin a New Topic",
                "NewTopic",
                typeof(CustomCommands),
                    new InputGestureCollection()
                    {
                        new KeyGesture(Key.N, ModifierKeys.Control)
                    }
            );

        //creates/modifies a topic and saves in the database
        public static readonly RoutedUICommand SaveTopic = new RoutedUICommand
                (
                    "Save Topic",
                    "SaveTopic",
                    typeof(CustomCommands),
                    new InputGestureCollection()
                        {
                            new KeyGesture(Key.S, ModifierKeys.Control)
                        }
                );

        
        /* Project Level Commands */

        //opens new project window
        public static readonly RoutedUICommand BeginNewProject = new RoutedUICommand
                (
                    "Begin a New Project",
                    "NewProject",
                    typeof(CustomCommands),
                    new InputGestureCollection()
                        {
                            new KeyGesture(Key.N, ModifierKeys.Control | ModifierKeys.Shift)
                        }
                );
            
            //creates a project with given data
            public static readonly RoutedUICommand CreateProject = new RoutedUICommand
                (
                    "Create Project",
                    "CreateProject",
                    typeof(CustomCommands)
                );
            
            //opens the open project window
            public static readonly RoutedUICommand OpenOpenProjectWindow = new RoutedUICommand
                (
                    "Open an existing project",
                    "OpenProject",
                    typeof(CustomCommands),
                    new InputGestureCollection()
                        {
                            new KeyGesture(Key.O, ModifierKeys.Control)
                        }
                );
            
    }

    public struct WoPage
    {
        internal UserControl control { get; set; }
        internal Project project { get; set; }

        public WoPage(UserControl control, Project project)
        {
            this.control = control;
            this.project = project;
        }
    }

}
