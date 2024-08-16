# Description of the project
This is a small project for review and practice. The current project includes books and users.The project is still ongoing, not completed. In the future, it may develop into a bookstore library.

# Programming language, project type and technologies used
- C#
- Asp.Net Core Web API, .NET 8.0.
- Visual studio, database PostgreSQL.

# Features
- Get data from the link and save it to the database.
- API methods GET, POST, PUT ,DELETE.
- Model in database: Books, Users, FavoriteBooks.
- The project is still ongoing, not yet completed. So other specific functions will be updated later.

# Project installation guide
- Step 1: Clone project.
```bash
$ git clone https://gitlab.kyanon.digital/Training-Intern-2024/khanhbook.git
```
- Step 2: connect the project to the PostgreSQL database.
In project, go to "appsettings.json". Change the information in the "ConnectionString" section to correspond to your postgreSQL database information.
Using Package Manager Console to run this line "update-database".

*Note: If you get an error in "add-migration" in visual studio, do the following:
Ctrl+R search cmd, open it and enter the following commands:
```bash
cd: yourfolder (ex: cd:D:\bop\congviec\ontap\c#\testbook\khanhbook)
```
```bash
D:
```
```bash
dotnet ef database update
```
IF you want add-migration:
```bash
dotnet ef migrations add NameYouWantToCreate
```
- Step 3: Start project.
Read Basic Auth information are in "appsettings.json" to access swagger when running the program.
Build and run the program.



