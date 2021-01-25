# Tool-WPF-Trello-Manage-Task
This tool helps to quickly plan your task in trello as well as export the task in excel, PDF...

## App-Config
You need to correct some app key & value in src\SupportTool\app.config file in order to start the application
    <add key="TrelloBoardId" value="<board id>" />
    <add key="TrelloKey" value="<dev key>" />
    <add key="TrelloToken" value="<dev token>" />
 
To get your board id:
 + When you have your short URL that looks like: https://trello.com/b/PyCAPoNt/task-reports (your current address in the browser)
 + Take that URL and add .json to the end as follows: https://trello.com/b/PyCAPoNt/task-reports.json
 => You will see the first "id" value. It's board id.

To get your key & token: You can refer this document https://trello.com/app-key

## Trello Config: Automatically functional will be improved as feature later
List name need to created:
 + Todo
 + Next Todo
 + In-Progress
 + Pending
 + Blocked
 + Completed
 + Closed

Need to add Power-Ups named "Custom Fields" into board then add the following custom fields:
 + Effort In day (hour) - Number
 + Percent - Number
 + Duedate At - Date
 + Assignee - Text
 + Finish Date At - Date