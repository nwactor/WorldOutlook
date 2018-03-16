using System;
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
using System.Windows.Shapes;

namespace IdeaV0
{
    /// <summary>
    /// Interaction logic for CreateNewProject.xaml
    /// </summary>
    public partial class CreateNewProjectWindow : Window
    {
        MainWindow parentWindow = ((MainWindow)Application.Current.MainWindow);
        SQLiteConnection dbConnection;

        public CreateNewProjectWindow()
        {
            dbConnection = parentWindow.dbConnection;
            InitializeComponent();
            FocusManager.SetFocusedElement(this, TitleInput);
        }

        private void CreateProject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (HelpFunctions.StringIsBlank(TitleInput.Text))
            {
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = true;
            }
        }

        private void CreateProjectCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            String title = TitleInput.Text;
            Project newProject = new Project(title);
            dbConnection.Open();

            //try to add project to project table in db
            SQLiteCommand command = dbConnection.CreateCommand();
            command.CommandText = "select title from UserProjects where title='" + title + "';";

            SQLiteDataReader reader = command.ExecuteReader();
            if(reader.Read()) // returns true if a project of that title already exists
            {
                //pop up box with 'project name already exists'
                reader.Close();
                dbConnection.Close();
            }
            else
            {
                //no need to specify projectID because the database automatically gives it one: https://sqlite.org/autoinc.html
                reader.Close();
                command.CommandText = "insert into UserProjects(title, subtitle, background_image) values ((@title), null, null )";
                command.Parameters.AddWithValue("@title", title);
                command.ExecuteNonQuery();
                dbConnection.Close();
                parentWindow.NavigateToProject(newProject);
                this.Close();
            }
        }

    }
}
