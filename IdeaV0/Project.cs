using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace IdeaV0
{
    public class Project
    {
        public String title { get; set; }
        public String subtitle { get; set; }
        public Image background_image { get; set; }
        private Dictionary<String, Topic> topics = new Dictionary<string, Topic>();
        private List<String> tags = new List<String>();
        //Representing the n:m relationship this way is space inefficient, 
        //but hopefully much faster since it will be able to directly find
        //every tag/topic in each topic/tag immediately, instead of checking every
        //single one in the project to see if it matches.
        //It also has the advantage keeping the pairings in one place, rather than risking
        //only updating the pairing status of one of the objects.
        //If I have to choose one or the other, keep tagConnections, since there are likely
        //to be less tags than topics, so when looking for a topic's tags it won't have to check as many
        private Dictionary<String, List<String>> topicConnections = new Dictionary<String, List<String>>(); //key = topics
        private Dictionary<String, List<String>> tagConnections = new Dictionary<String, List<String>>(); //key = tags
        private SQLiteConnection dbConnection = ((MainWindow)Application.Current.MainWindow).dbConnection;

        public Project(String title)
        {
            this.title = title;
        }

        public Project(String title, String subtitle, Image background_image)
        {
            this.title = title;
            this.subtitle = subtitle;
            this.background_image = background_image;
        }

        //If the project has no topics loaded in memory,
        //tries to load some from the DB
        public void LoadTopicsIfNeeded()
        {
            if (this.topics.Count == 0)
            {
                SQLiteCommand command = dbConnection.CreateCommand();
                command.CommandText = "select * from UserTopics where parent_project = (@project)";
                command.Parameters.AddWithValue("@project", this.title);
                dbConnection.Open();
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Topic current = new Topic();
                    current.parent_project = reader.GetString(0);
                    current.title = reader.GetString(1);
                    current.encoded_text = reader.GetString(2);
                    if(reader["main_image"] != null && !Convert.IsDBNull(reader["main_image"]))
                    {
                        byte[] bytes = (byte[]) reader["main_image"];
                        current.main_image = HelpFunctions.LoadImageFromBytes(bytes);
                    }
                    //current.image_gallery = reader.GetBlob(4, false);
                    topics.Add(current.title, current);
                    topicConnections.Add(current.title, new List<String>());
                    //Console.WriteLine("topic loaded: " + reader.GetString(1));
                }
                reader.Close();
                dbConnection.Close();
            }
        }

        public void LoadTagsIfNeeded()
        {
            if(this.tags.Count == 0)
            {
                SQLiteCommand command = dbConnection.CreateCommand();
                command.CommandText =
                    "select name from UserTags where parent_project = (@project)";
                command.Parameters.AddWithValue("@project", this.title);
                dbConnection.Open();
                SQLiteDataReader reader = command.ExecuteReader();
                while(reader.Read())
                {
                    String current;
                    current = reader.GetString(0);
                    tags.Add(current);
                    tagConnections.Add(current, new List<String>());
                    //Console.WriteLine("tag loaded: " + reader.GetString(0));
                }
                reader.Close();
                dbConnection.Close();
            }
        }
        
        public void LoadConnectionsIfNeeded()
        {
            //check if there are any connections in memory already;
            //if so, no need to check the DB for more, so return
            foreach (String tag in this.tags)
            {
                if (GetAssociatedTopics(tag).Count > 0) { return; }
            }

            SQLiteCommand command = dbConnection.CreateCommand();
                command.CommandText = "select topic_title, tag_name from TopicTags where parent_project = (@project)";
                command.Parameters.AddWithValue("@project", this.title);
                dbConnection.Open();
                SQLiteDataReader reader = command.ExecuteReader();
                while(reader.Read())
                {
                    String topic = reader.GetString(0);
                    String tag = reader.GetString(1);
                    
                    this.topicConnections[topic].Add(tag);
                    this.tagConnections[tag].Add(topic);
                }
                reader.Close();
                dbConnection.Close();
        }

        //Gets a topic from this project given it's name
        public Topic GetTopic(String topicTitle)
        {
            return topics[topicTitle];
        }
        
        //Adds a topic to this project's memory instance
        public void AddTopic(Topic topic)
        {
            AddTopicToMemory(topic);

            //add to DB
            SQLiteCommand command = dbConnection.CreateCommand();
            command.CommandText = "insert into UserTopics(parent_project, title, text_body, main_image, image_gallery) values " +
                    "((@parent)," +
                    "(@title)," +
                    "(@body)," +
                    "null, null)";
            command.Parameters.AddWithValue("@parent", topic.parent_project);
            command.Parameters.AddWithValue("@title", topic.title);
            command.Parameters.AddWithValue("@body", topic.encoded_text);
            dbConnection.Open();
            command.ExecuteNonQuery();
            dbConnection.Close();
        }
        private void AddTopicToMemory(Topic topic)
        {
            //Dictionaries only take unique keys
            topics.Add(topic.title, topic);
            topicConnections.Add(topic.title, new List<String>());
        }

        //Removes a topic from this project and the DB
        public void RemoveTopic(String topicTitle)
        {
            RemoveTopicFromMemory(topicTitle);
            
            //remove from DB
            SQLiteCommand command = dbConnection.CreateCommand();
            command.CommandText = "delete from UserTopics where title = (@title) and parent_project = (@parent)";
            command.Parameters.AddWithValue("@title", topicTitle);
            command.Parameters.AddWithValue("@parent", this.title);
            dbConnection.Open();
            command.ExecuteNonQuery();
            dbConnection.Close();
            command.CommandText = "delete from TopicTags where topic_title = (@title) and parent_project = (@parent)";
            command.Parameters.AddWithValue("@title", topicTitle);
            command.Parameters.AddWithValue("@parent", this.title);
            dbConnection.Open();
            command.ExecuteNonQuery();
            dbConnection.Close();
        }
        private void RemoveTopicFromMemory(String topicTitle)
        {
            /*references to the topic's title are stored in three places:
            this.topics, this.topicConnections, and this.tagConnections.
            any references to the title must be removed from all three
            of those places.*/
            
            //remove from topicConnections
            topicConnections.Remove(topicTitle);
            //remove from tagConnections
            foreach (String tag in tagConnections.Keys)
            {
                tagConnections[tag].Remove(topicTitle);
            }
            //remove from this.topics
            topics.Remove(topicTitle);
        }

        //Updates all references to a project with the info of a new one
        public void EditTopic(String oldTitle, Topic newTopic)
        {
            //update memory
            if (!newTopic.title.Equals(oldTitle))
            {
                //add references to new title, remove references from old title
                AddTopicToMemory(newTopic);
                List<String> associatedTopics = new List<String>(topicConnections[oldTitle]);
                foreach (String tag in associatedTopics)
                {
                    CreateTopicTagPairing(newTopic.title, tag);
                    DeleteTopicTagPairing(oldTitle, tag);
                }
                RemoveTopicFromMemory(oldTitle);
            }
            else
            {
                topics[oldTitle] = newTopic;
            }

            //update database

            //convert complex fields to binary
            byte[] mainImageBytes = HelpFunctions.ImageToBytes(newTopic.main_image);

            //update UserTopics
            SQLiteCommand command = dbConnection.CreateCommand();
            command.CommandText =
                "update UserTopics set " +
                "title = (@title), " +
                "text_body = (@body), " +
                "main_image = (@image) " + //don't forget to add comma back in
                                           //"set image_gallery = (@gallery) " +
                "where parent_project = (@parent) " +
                "and title = (@oldTitle)";

            command.Parameters.AddWithValue("@title", newTopic.title);
            command.Parameters.AddWithValue("@body", newTopic.encoded_text);
            command.Parameters.AddWithValue("@image",mainImageBytes);
            //command.Parameters.AddWithValue("@gallery", );
            command.Parameters.AddWithValue("@parent", newTopic.parent_project);
            command.Parameters.AddWithValue("@oldTitle", oldTitle);
            dbConnection.Open();
            command.ExecuteNonQuery();
            dbConnection.Close();
        }

        //Checks if this project contains a topic of the given name
        public Boolean ContainsTopic(String topicName)
        {
            return topics.Keys.Contains(topicName);
        }
        
        //Gets a list of the names of the project's topics
        public List<String> GetTopicList()
        {
            return topics.Keys.ToList<String>();
        }

        //Adds a tag to this project and save it in DB
        public void AddTag(String tag)
        {
            if(this.ContainsTag(tag))
            {
                Console.WriteLine("Tag already exists");
                return;
            }
            else
            {
                //add the tag to the database
                SQLiteCommand command = dbConnection.CreateCommand();
                command.CommandText =
                    "insert into UserTags(parent_project, name) values " +
                    "((@parent), (@name))";
                command.Parameters.AddWithValue("@parent", this.title);
                command.Parameters.AddWithValue("@name", tag);
                dbConnection.Open();
                command.ExecuteNonQuery();
                dbConnection.Close();

                //add the tag in memory
                tags.Add(tag);
                tagConnections.Add(tag, new List<String>());
            }
        }
        
        //Removes a tag from this project and the DB
        public void RemoveTag(String tagName)
        {
            //remove all connections with topics in memory
            tagConnections.Remove(tagName);
            foreach(String topic in topicConnections.Keys)
            {
                topicConnections[topic].Remove(tagName);
            }

            //remove the tag from this project's memory instance
            tags.Remove(tagName);

            //remove the tag from the DB
            SQLiteCommand command = dbConnection.CreateCommand();
            command.CommandText = "delete from UserTags where name = (@name) and parent_project = (@project)";
            command.Parameters.AddWithValue("@name", tagName);
            command.Parameters.AddWithValue("@project", this.title);
            dbConnection.Open();
            command.ExecuteNonQuery();
            dbConnection.Close();
            command.CommandText = "delete from TopicTags where  tag_name = (@name) and parent_project = (@project)";
            command.Parameters.AddWithValue("@name", tagName);
            command.Parameters.AddWithValue("@project", this.title);
            dbConnection.Open();
            command.ExecuteNonQuery();
            dbConnection.Close();
        }

        //returns true if this project contains the given tag
        public Boolean ContainsTag(String tagName)
        {
            return tags.Contains(tagName);
        }

        //returns a clone of this.tags
        public List<String> GetTagList()
        {
            List<String> list = new List<String>();
            foreach (String tag in tags)
            {
                list.Add(tag);
            }
            return list;
        }

        /** Takes input of the form (tagName, tagName, tagName)**/
        public void AddTagsToTopicFromString(String tags, Topic topic)
        {
            String[] tagsToAdd = tags.Split(',');
            foreach(String tag in tagsToAdd)
            {
                if (tag != null && this.tags.Contains(tag))
                {
                    CreateTopicTagPairing(topic.title, tag);
                }
            }
        }

        //Assumes that tagsToAdd and tagsToRemove contain mutually exclusive elements
        public void UpdateTopicTags(String topic, List<String> tagsToAdd, List<String> tagsToRemove)
        {
            if(tagsToRemove != null)
            {
                foreach (String tagToRemove in tagsToRemove)
                {
                    if(this.GetAssociatedTags(topic).Contains(tagToRemove))
                    {
                        this.DeleteTopicTagPairing(topic, tagToRemove);
                    }
                }
            }
            if(tagsToAdd != null)
            {
                foreach(String tagToAdd in tagsToAdd)
                {
                    if(!this.GetAssociatedTags(topic).Contains(tagToAdd))
                    {
                        this.CreateTopicTagPairing(topic, tagToAdd);
                    }
                }
            }   
        }

        //Pairs a topic and tag
        private void CreateTopicTagPairing(String topic, String tag)
        {
            CreateAssociationInMemory(topic, tag);

            //add entry to db join table
            SQLiteCommand command = dbConnection.CreateCommand();
            command.CommandText = "insert into TopicTags (parent_project, topic_title, tag_name) " +
                "values ((@parent), (@topic), (@tag))";
            command.Parameters.AddWithValue("@parent", this.title);
            command.Parameters.AddWithValue("@topic", topic);
            command.Parameters.AddWithValue("@tag", tag);
            dbConnection.Open();
            command.ExecuteNonQuery();
            dbConnection.Close();
        }
        private void CreateAssociationInMemory(String topic, String tag)
        {
            //add tag to list of tags associated with the topic
            this.topicConnections[topic].Add(tag);

            //add topic to list of tags associated with the tag
            this.tagConnections[tag].Add(topic);
        }

        //Deletes pairing between topic and tag
        private void DeleteTopicTagPairing(String topic, String tag)
        {
            DeleteAssociationInMemory(topic, tag);

            //remove entry from db join table
            SQLiteCommand command = dbConnection.CreateCommand();
            command.CommandText = "delete from TopicTags where parent_project = (@parent) and topic_title = (@topic) and tag_name = (@tag)";
            command.Parameters.AddWithValue("@parent", this.title);
            command.Parameters.AddWithValue("@topic", topic);
            command.Parameters.AddWithValue("@tag", tag);
            dbConnection.Open();
            command.ExecuteNonQuery();
            dbConnection.Close();
        }
        private void DeleteAssociationInMemory(String topic, String tag)
        {
            //remove tag from list of tags associated with the topic
            this.topicConnections[topic].Remove(tag);

            //remove topic from list of topics associated with the tag
            this.tagConnections[tag].Remove(topic);
        }

        //Returns the topics that are associated with one of this project's tags
        public List<String> GetAssociatedTopics(String tag)
        {
            return tagConnections[tag];
        }

        //Returns the tags that are associated with one of this project's topics
        public List<String> GetAssociatedTags(String topic)
        {
            return topicConnections[topic];
        }
    }

    /*public class Tag
    {
        private String tag_name;
        private String parent_project;
        //private List<Topic> topics = new List<Topic>();

        public Tag(String name, String project)
        {
            tag_name = name;
            parent_project = project;
        }

        public String GetName()
        {
            return tag_name;
        }

        public void ChangeName(String newName)
        {
            tag_name = newName;
        }

        public String GetParent()
        {
            return parent_project;
        }
    }
    */

}
