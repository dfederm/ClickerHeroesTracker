import { platformBrowserDynamic } from "@angular/platform-browser-dynamic";
import { enableProdMode } from "@angular/core";

import { AppModule } from "./app.module";

if (process.env.ENV === "prod")
{
    enableProdMode();
}

if (typeof (Storage) !== "undefined") {
    let currentTheme = localStorage.SiteThemeType;
    let cssUrl = "https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/4.0.0-alpha.6/css/bootstrap.min.css";

    if (currentTheme && currentTheme.toLowerCase() === "dark") {
        cssUrl = "https://cdnjs.cloudflare.com/ajax/libs/bootswatch/4.0.0-alpha.6/slate/bootstrap.min.css";
    }

    document.getElementById("bootstrapStylesheet").setAttribute("href", cssUrl);
}

platformBrowserDynamic().bootstrapModule(AppModule);
