# World Outlook

World Outlook is a desktop application for Windows designed to help users organize notes, projects, and documents from the security of their own local hard-drive. It provides a wiki-like format in which entries are organized by "tags," allowing for quick and intuitive searching and filtering.

## Status
Proof-of-Concept

## Goal
World Outlook was created as the answer to several questions concerning organization and productivity: 
How can I access the information in my notes as quickly as possible, so that I don't interrupt my train of thought navigating to them? 
How can I clearly and intuitively present it? 
And how can I keep it all secure? 
There are other applications out there that offer similar things, but many have less flexible features, or store user data remotely. World Outlook is a tool for people who value the ability to self-structure, having everything in one place, and privacy.


## Use
To begin, create a new project using the button on the application's homepage (or the file menu). Projects are self-contained areas which are meant to contain all of your information about a subject. Generally, projects should cover a broad category that contains many individual topics that you want to address. For instance, if you are a student, you might create projects for the classes you are taking, like "History" and "Chemistry".

Once you've created a project you will be brought to the project's homepage. Here, you can create new topics, which are where the actual information will be stored. Creating new topics fills the list in the center of the project's homepage, and you can navigate to a topic by clicking on it. Navigation is also made easier by the 'back' and 'forward' buttons at the top left of the window. Topics work like word processing documents; you can write anything you want in them, and save and edit them at any time.

After you've made several topics you can start to organize them. This is where tags come in. Tags are like categories that you can organize your topics into, with the distinction that a topic can have multiple tags. For example, a historian with a project about World War I might have the tags "Person" and "French." Both of these tags would apply to a French general like Joseph Joffre, but only "French" would apply to Paris. You can create a tag from the right-hand side of your project's homepage.

When a tag is added it appears under the project's title. You can associate tags with topics in two ways: right click the tag in the project home, or click "add tag" when you are editing a topic. Tags act as filters, and clicking on a tag on the project's home page will activate it, filtering out topics that it doesn't belong to. 

Activating multiple tags has a different outcome based on your filtering mode: With the OR mode, you will get all the topics that include any of the activated tags, but with the AND mode, you will only get the topics that contain all of the tags.


## Implementation
World Outlook was created using Microsoft's Windows Presentation Foundation (WPF), a subset
of .Net. It stores the user's information locally in a SQLite database.


## Planned Features
*Editing Project Info
*Horizontal Line button for text-editor taskbar
*Common formatting options for text-editor taskbar
*Search Bar Functionality
*Document linking/backup option
*"About" page
*Topic list heading display filters on project home
*Image Gallery / Entire Image panel/frame
*Templates  for topics (Including being able to set a default template for the project, or a tag)
*Hyperlinking to another topic/project/location in text from text within a topic
	(Window Popup with project, topic dropdown lists; project defaults to the current one, topic defaults to "choose topic")
*Opening to last viewed page
*Transfer topic to different project
*Side-by-side topic view
*Edit button, save topic button, etc follow the user as they scroll
*Section-based topics instead of single document?
*Asynchronous database access?


## Known Bugs & Issues
*Line Spacing on New Topic page
*wrap Project Home topic blocks
*Fix picutres saving in RTB
*Escape should cancel editing
*Finish adding keyboard shortcuts, less clunky ways to add tags
*ctrl+n shouldn't work while editing, or should create a message box saying "are you sure you want to navigate away"
*More text editing polishing  (the selection bug?)
*App scrolling layout
*Add home button to menu next to forward button
*Auto-sort topic and tag lists when a new one is added  (binary insert)


## Author
Nick Wactor