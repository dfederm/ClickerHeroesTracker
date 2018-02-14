// tslint:disable:no-require-imports
// tslint:disable:no-var-requires

if (process.env.ENV === "prod") {
    require("@angular/core").enableProdMode();
    let platformBrowser = require("@angular/platform-browser").platformBrowser;
    let appModuleFactory = require("./app.module.ngfactory").AppModuleNgFactory;
    platformBrowser().bootstrapModuleFactory(appModuleFactory);
} else {
    let platformBrowserDynamic = require("@angular/platform-browser-dynamic").platformBrowserDynamic;
    let appModule = require("./app.module").AppModule;
    platformBrowserDynamic().bootstrapModule(appModule);
}
