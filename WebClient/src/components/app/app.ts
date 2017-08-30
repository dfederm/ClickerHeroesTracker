import { Component, OnInit } from "@angular/core";

@Component({
  selector: "app",
  templateUrl: "./app.html",
})
export class AppComponent implements OnInit
{
  public ngOnInit(): void
  {
      if (typeof (Storage) !== "undefined") {
        let currentTheme = localStorage.SiteThemeType;
        let cssUrl = "https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/4.0.0-alpha.6/css/bootstrap.min.css";

        if (currentTheme && currentTheme.toLowerCase() === "dark") {
            cssUrl = "https://cdnjs.cloudflare.com/ajax/libs/bootswatch/4.0.0-alpha.6/slate/bootstrap.min.css";
        }

        document.getElementById("bootstrapStylesheet").setAttribute("href", cssUrl);
    }
  }
}
