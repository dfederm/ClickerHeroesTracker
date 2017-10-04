import { Component, OnInit } from "@angular/core";
import { SettingsService } from "../../services/settingsService/settingsService";

@Component({
  selector: "app",
  templateUrl: "./app.html",
})
export class AppComponent implements OnInit {
  constructor(private settingsService: SettingsService) { }

  public ngOnInit(): void {
    this.settingsService
      .settings()
      .subscribe(settings => {
        let cssUrl: string;
        switch (settings.theme) {
          case "dark": {
            cssUrl = "https://cdnjs.cloudflare.com/ajax/libs/bootswatch/4.0.0-alpha.6/slate/bootstrap.min.css";
            break;
          }
          case "light":
          default: {
            cssUrl = "https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/4.0.0-alpha.6/css/bootstrap.min.css";
            break;
          }
        }

        // This needs to be in the <head> so it's not part of this component's template.
        document.getElementById("bootstrapStylesheet").setAttribute("href", cssUrl);
      });
  }
}
