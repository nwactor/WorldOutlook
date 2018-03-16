WORLD OUTLOOK (name subject to change) is a Windows desktop application designed to
help users organize their notes, projects, and documentation from the security of their own
hard-drive. It provides a wiki-like format in which entries are organized by "tags," allowing
for quick and intuitive searching and filtering.


Status---
Proof-of-Concept


Guide---
To begin, create a new project using the file menu or the button on the application's homepage.
Projects are self-contained areas which encapsulate all of the information you input about a subject.
Generally, projects should cover a broad set of ideas with many individual aspects that you want to
address individually and then see how they all connect. For instance, if you are a student, you may have
projects for "History" and "Chemistry". If you are an author, you might have a project for each
series that you are writing.

Once you've created your project you will be brought to its new homepage. Here, you can create new topics,
which are where the actual information will be stored. As you create new topics, the topic list in the center
of the page will be filled, and you can navigate to any topic by clicking on its name. (Note: navigation is
also made easier by the 'back' and 'forward' buttons at the top left of the window.) You can think of
topics like the documents on your computer.

After you've made several topics, you may be wondering how you can organize them. This is where tags come in.
You can create a tag from the right-hand side of your project's homepage. Tags are like categories that you
can organize your topics into, with the distinction that a topic can have multiple tags. For example, a historian
with a project about World War I might have the tags "Person" and "French." Both of these tags would apply to
a French general like Joseph Joffre, but only "French" would apply to Paris.

When you add a tag, you will see that it appears under the project's title. You can associate tags with topics in
two ways: right click the tag in the project home, or click "add tag" when you are editing a topic. Tags act as filters, 
and clicking on a tag on the project's home page will activate it, filtering out topics that it doesn't belong to. 
Activating multiple tags has a different outcome based on your filtering mode: With the OR mode, you will get all the 
topics that include any of the activated tags, but with the AND mode, you will only get the topics that contain all of 
the tags. Try it out for quick filtering and sorting!


Goal---
World Outlook was created to solve problems of organization and productivity: How can I access the information in my notes
as quickly as possible, so that I don't interrupt my train of thought navigating to them? How can I clearly and intuitively
present it? And how can I keep it all secure? There are a lot of applications out there that offer similar things, but many 
are web-apps that store the user's data remotely, or have less-flexible features. World Outlook is there to fill a niche for
those who desire the ability to self-structure, have everything in one place, and privacy.


Implementation---
World Outlook was created using Microsoft's Windows Presentation Foundation (WPF), a subset
of .Net. It stores the user's information locally in a SQLite database.


Planned Features---
*Editing Project Info
*Horizontal Line button for text-editor taskbar
*Common formatting options for text-editor taskbar
*Search Bar Functionality
*Document linking/backup option
*"About" page
*Topic list heading display filters on project home
*Image Gallery / Entire Image panel/frame
*Templates  for topics (Including being able to set a default template for the project, or a tag)
	"Warning: Changing template will delete the body of this topic. Make sure you have saved anything you want to keep in a different location."
*Hyperlinking to another topic/project/location in text from text within a topic
	(Window Popup with project, topic dropdown lists; project defaults to the current one, topic defaults to "choose topic")
*Opening to last viewed page
*Transfer topic to different project
*Side-by-side topic view
*Edit button, save topic button, etc follow the user as they scroll
*Section-based topics instead of single document?
*Multithreading for accessing database


Known Bugs & Issues---
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


Author---
Nick Wactor