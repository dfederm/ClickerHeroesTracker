# Getting started
## Prereqs
1. **Clone the Repo** - There should be a "Clone" button to the right on this page.
1. Install [.NET Core 2.1.0](https://github.com/dotnet/cli/blob/master/README.md#installers-and-binaries). Currently there is no official release for this version, so you'll need to download a nightly build. Try to match the version listed in [global.json](global.json)
1. If you want to use **Visual Studio**, install [Visual Studio 2017 Preview (15.3)](https://www.visualstudio.com/vs/preview/). The Community version is free.
1. You may also build from the **command line** and use an editor like [Visual Studio Code](https://code.visualstudio.com/).
1. (Optional, but recommended) If you want to use **SQL** for the local database:
    1. [SQL LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-2016-express-localdb#install-localdb) should work just fine.
    1. Add a new empty database to your local instance, eg. "ClickerHeroesTrackerData"
    1. Update your user secrets with the database connection string and change the Database Kind to "SqlServer". To get to your user secrets, in Visual Studio you can right-click on the Website project and select "Manage User Secrets", or you can manually find them (or create the file if it doesn't exist) at `%APPDATA%\Microsoft\UserSecrets\aspnet-ClickerHeroesTrackerWebsite-20161025101322\secrets.json`
    1. The required schema structure will be set up automatically when starting the application for the first time.
1. (Optional, SQL LocalDB above is recommended instead) If you want to use **Sqlite** for the local database, install [SqliteBrowser](http://sqlitebrowser.org/) to easily query Sqlite during local development. Note that at this point, SqlLite doesn't work very well and may be ripped out soon.
1. (Optional) Install [Azure Storage Emulator](https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409) to locally mock a storage account.
    - By default, in memory stores are used.
    - To use the Azure Storage Emulator set `Storage:ConnectionString` to `"UseDevelopmentStorage=true;"` by uncommenting it out in `appsettings.json`.

## Building
Today there is a main website and a new Angular app. The main website serves as both the API and the MVC application, but we are transitioning to the Angular app for the UI.

Because of the temporary duplication, you'll need to build both in the short term.

### Web Client
The Angular app is known as the webclient, rooted at the `WebClient` folder. Visual Studio Code is recommended to work with this project.

First run `npm install` initially to download the packages.

Use one of the following commands depending on what you're trying to do:
* `npm run clean` - Cleans the `dist` dir
* `npm run build` - Builds the project for production and puts it in the `dist` dir
* `npm run watch` - Builds the project and watches for file changes for incremental builds as you go. This is recommended during normal development.
* `npm run test` - Runs all unit tests. Note that building first is not required as we compile on the fly.
* `npm run test-watch` - Runs tests in a browser which you can refresh to re-run and debug as needed.

Other useful tools:
* `build` produces a `/logs/stats/report.html` which visualizes the bundles. The raw `stats.json` file can also be used to feed into other visualization tools. The bundles are as follows:
    * `app` is the main app code
    * `data` is a bundle of json files. This is in a separate bundle since the data shoudl not change often.
    * `vendor` is most the 3rd party libraries
    * `polyfill` is the polyfill scripts
* `test` produces a code coverage report under `/logs/coverage/html/index.html`. This can be used to drill into code coverage. A cobertura report is also produced for the VSTS build.

### Website
The Website is both the API and the MVC application. Visual Studio is recommended to work with this project.

First run `bower install` and `npm install` initially to download the frontend packages. Then simply building the solution in Visual Studio should be sufficient.

The gulp commands to use are:
* `clean` - Cleans the generated frontend files
* `build` - Builds the frontend assets. This should automatically run when building the solution.
* `watch` - Incrementally builds the frontend assets. This should automatically start when opening the solution.
* `copy` - Copies the Web Client's `dist` dir to the `wwwroot` so the Website can serve the Web Client.

## Starting the service
After building both the Web Client and Website, start the Website as you normally would in Visual Studio. For the best experience, use the "web" profile, but IIS Express should also work just fine.

In steady state, you should have the relevant `watch` commands running in both the Web Client and Website when working in the UI.

## Create an Admin user
To make a user a site admin:
1. Create a user via registration normally, or use an existing one.
1. Get the user id from the database. In this example, suppose it's `71584f37-4165-4cd6-90af-0762ea996b6e`
1. Ensure the Admin role exists in the `AspNetRoles` table. If it does not, add it with:

       INSERT INTO AspNetRoles(Id, ConcurrencyStamp, Name, NormalizedName)
       VALUES(NEWID(), NEWID(), 'Admin', 'ADMIN');

*NOTE: for SqlLite, you will have to manually create guids to replace unsupported NEWID()*

1. Add your user to the role. Rememeber to replace the user id below with the one from above.

       INSERT INTO AspNetUserRoles(UserId, RoleId)
       VALUES('71584f37-4165-4cd6-90af-0762ea996b6e', (SELECT Id FROM AspNetRoles WHERE Name = 'Admin'));

1. You may need to log out and back in for the change to take effect. You should see an Admin link in the navbar if you were successful.

## Using the API
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
