// tslint:disable:no-require-imports
// tslint:disable:no-var-requires

// Polyfills
import "core-js/es/reflect";

// Angular needs zone.js
import "zone.js/dist/zone";

if (process.env.NODE_ENV === "production") {
    require("@angular/core").enableProdMode();
    let platformBrowser = require("@angular/platform-browser").platformBrowser;
    let appModuleFactory = require("./app.module.ngfactory").AppModuleNgFactory;
    platformBrowser().bootstrapModuleFactory(appModuleFactory);
} else {
    // Better dubuggability
    Error.stackTraceLimit = Infinity;
    require("zone.js/dist/long-stack-trace-zone");

    let platformBrowserDynamic = require("@angular/platform-browser-dynamic").platformBrowserDynamic;
    let appModule = require("./app.module").AppModule;
    platformBrowserDynamic().bootstrapModule(appModule);
}
