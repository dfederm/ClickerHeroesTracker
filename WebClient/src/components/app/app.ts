import { Component, OnInit } from "@angular/core";

@Component({
  selector: "app",
  templateUrl: "./app.html",
})
export class AppComponent implements OnInit {
  public ngOnInit(): void {
    let cssUrl = "https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/4.0.0-alpha.6/css/bootstrap.min.css";
    if (localStorage) {
      let currentTheme: string = localStorage.SiteThemeType;
      if (currentTheme && currentTheme.toLowerCase() === "dark") {
        cssUrl = "https://cdnjs.cloudflare.com/ajax/libs/bootswatch/4.0.0-alpha.6/slate/bootstrap.min.css";
      }
    }

    // This needs to be in the <head> so it's not part of this component's template.
    document.getElementById("bootstrapStylesheet").setAttribute("href", cssUrl);
  }
}
