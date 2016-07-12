# Getting started #
## Prereqs ##
1. **Clone the Repo** - There should be a "Clone" button to the right on this page.
2. If you want to use **Visual Studio**
    1. Install [Visual Studio Community Edition](https://www.visualstudio.com/products/free-developer-offers-vs) for free
    2. If you already had it installed, ensure you have [Visual Studio Update 3](https://go.microsoft.com/fwlink/?LinkId=691129) installed.
    3. Install [.NET Core 1.0 for Visual Studio](https://go.microsoft.com/fwlink/?LinkId=817245)
    4. Skip the next step.
3. If you'd like to build from the **command line** and use an editor like [Visual Studio Code](https://code.visualstudio.com/), install the [.NET Core SDK for Windows](https://go.microsoft.com/fwlink/?LinkID=809122).
4. (Optional) If you want to use **Sqlite** for the local database, install [SqliteBrowser](http://sqlitebrowser.org/) to easily query Sqlite during local development.
5. (Optional) If you want to use **SQL** for the local database
	1. Add a new empty database to your local SQL server, eg. "ClickerHeroesTrackerData"
	2. Update `appsettings.json` with the database connection string and change the Database Kind to "SqlServer".
	3. The required schema structure will be set up automatically when starting the application for the first time.
5. (Optional) Install [Azure Storage Emulator](https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409) to locally mock a storage account.
