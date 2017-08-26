import { platformBrowserDynamic } from "@angular/platform-browser-dynamic";
import { enableProdMode } from "@angular/core";

import { AppModule } from "./app.module";

if (process.env.ENV === "prod")
{
    enableProdMode();
}

platformBrowserDynamic().bootstrapModule(AppModule);
