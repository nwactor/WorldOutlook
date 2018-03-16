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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IdeaV0
{
    /// <summary>
    /// Interaction logic for AppNexusUserControl.xaml
    /// </summary>
    public partial class AppNexusUserControl : UserControl
    {
        private HashSet<String> projects;
        private Dictionary<String, TextBlock> projectLinks = new Dictionary<String, TextBlock>();
        private SQLiteConnection dbConnection = ((MainWindow)Application.Current.MainWindow).dbConnection;

        public AppNexusUserControl()
        {
            PopulateProjectList();
            InitializeComponent();
            UpdateProjectListUI();
        }

        private void PopulateProjectList()
        {
            projects = ((MainWindow)Application.Current.MainWindow).appProjects;
        }

        //could use some optimization, here and in project home control
        private void UpdateProjectListUI()
        {
            //Add new projects to the UI
            foreach (String projectTitle in projects)
            {
                if(!projectLinks.Keys.Contains(projectTitle))
                {
                    CreateProjectLink(projectTitle);
                }
            }
            //remove deleted projects from the UI
            List<String> oldProjects = new List<String>(); //avoid concurrent modification
            foreach(String projectTitle in projectLinks.Keys)
            {
                if(!projects.Contains(projectTitle))
                {
                    ProjectsPanel.Children.Remove(projectLinks[projectTitle]);
                    oldProjects.Add(projectTitle);
                }
            }
            foreach(String s in oldProjects)
            {
                projectLinks.Remove(s);
            }
        }

        private void CreateProjectLink(String projectTitle)
        {
            TextBlock projectBlock = new TextBlock();
            projectBlock.Text = projectTitle;
            projectBlock.Margin = new Thickness(0,5,0,5);
            projectBlock.FontSize = 15;
            projectBlock.FontWeight = FontWeights.DemiBold;
            projectBlock.MouseUp += ProjectLink_Clicked;
            projectBlock.Cursor = Cursors.Hand;
            projectLinks.Add(projectTitle, projectBlock);
            ProjectsPanel.Children.Add(projectBlock);
        }

        private void ProjectLink_Clicked(Object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).NavigateToProject((sender as TextBlock).Text);
        }
    }
}
