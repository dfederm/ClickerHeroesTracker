# Getting started
## Prereqs
1. **Clone the Repo** - There should be a "Clone" button to the right on this page.
1. If you want to use **Visual Studio**
    1. Install [Visual Studio 2017 Preview 15.3](https://www.visualstudio.com/vs/preview/)
    1. Install [.NET Core 2.0 Preview 1](https://www.microsoft.com/net/core/preview)
    1. Skip the next step.
1. If you'd like to build from the **command line** and use an editor like [Visual Studio Code](https://code.visualstudio.com/), install the [.NET Core SDK for Windows](https://go.microsoft.com/fwlink/?LinkID=809122).
1. (Optional) If you want to use **Sqlite** for the local database, install [SqliteBrowser](http://sqlitebrowser.org/) to easily query Sqlite during local development.
1. (Optional) If you want to use **SQL** for the local database
	1. Add a new empty database to your local SQL server, eg. "ClickerHeroesTrackerData"
	1. Update `appsettings.json` with the database connection string and change the Database Kind to "SqlServer".
	1. The required schema structure will be set up automatically when starting the application for the first time.
1. (Optional) Install [Azure Storage Emulator](https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409) to locally mock a storage account.

# Admin user
To make a user a site admin:
1. Create a user via registration normally, or use an existing one.
1. Get the user id from the database. In this example, suppose it's `71584f37-4165-4cd6-90af-0762ea996b6e`
1. Ensure the Admin role exists in the `AspNetRoles` table. If it does not, add it with:

       INSERT INTO AspNetRoles(Id, ConcurrencyStamp, Name, NormalizedName)
       VALUES(NEWID(), NEWID(), 'Admin', 'ADMIN');

1. Add your user to the role. Rememeber to replace the user id below with the one from above.

       INSERT INTO AspNetUserRoles(UserId, RoleId)
       VALUES('71584f37-4165-4cd6-90af-0762ea996b6e', (SELECT Id FROM AspNetRoles WHERE Name = 'Admin'));

1. You may need to log out and back in for the change to take effect. You should see an Admin link in the navbar if you were successful.

# Using the API
It sometimes can be easier during development to directly hit the API instead of using the website, especially for automation tasks like database setup or cleanup.

## Authentication
The site supports Cookie authentication, but when hitting the API wtihout a browser, this can be hard to use. However, the site is its own OpenID Connect Server so can produce its own authentication token.

1. Download a program that can help you make raw HTTP requests, such as [Fiddler](http://www.telerik.com/fiddler).
1. Retrieve an auth token using the following request:

       POST http://localhost:5000/api/connect/token
       Content-Type: application/x-www-form-urlencoded
       
       grant_type=password&username={username}&password={password}

1. The response should be a Json object with the `access_token`, `expires_in`, and `token_type` fields.
1. Make the API request you want, including an Authorization header with the token. Example:

       GET http://localhost:5000/api/uploads
       Authorization: {token_type} {access_token}
