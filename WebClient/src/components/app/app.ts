import { Component, OnInit } from "@angular/core";
import { LoggingService } from "../../services/loggingService/loggingService";
import { NgxSpinnerService } from "ngx-spinner";

import { SettingsService, IUserSettings } from "../../services/settingsService/settingsService";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";

@Component({
  selector: "app",
  templateUrl: "./app.html",
})
export class AppComponent implements OnInit {
  public static defaultTheme = "light";

  // TODO: Consider getting all themes from https://bootswatch.com/api/5.json
  public static themeCssUrls: { [theme: string]: string } = {
    light: "https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/css/bootstrap.min.css",
    dark: "https://cdn.jsdelivr.net/npm/bootswatch@5.1.3/dist/slate/bootstrap.min.css",
  };

  public isLoading: boolean;

  private hasSettings: boolean;

  private hasUserInfo: boolean;

  constructor(
    private readonly settingsService: SettingsService,
    private readonly authenticationService: AuthenticationService,
    private readonly loggingService: LoggingService,
    private readonly spinnerService: NgxSpinnerService,
  ) { }

  public ngOnInit(): void {
    this.spinnerService.show();
    this.isLoading = true;

    // Get the auth headers simply because it forces a wait on fetching the initial auth tokens.
    this.authenticationService
      .getAuthHeaders()
      .catch(() => void 0) // Swallow errors
      .then(() => {
        this.authenticationService
          .userInfo()
          .subscribe(userInfo => this.handleUserInfo(userInfo));

        this.settingsService
          .settings()
          .subscribe(settings => this.handleSettings(settings));
      });
  }

  private handleSettings(settings: IUserSettings): void {
    let cssUrl = AppComponent.themeCssUrls[settings.theme] || AppComponent.themeCssUrls[AppComponent.defaultTheme];

    // This needs to be in the <head> so it's not part of this component's template.
    document.getElementById("bootstrapStylesheet").setAttribute("href", cssUrl);

    this.hasSettings = true;
    this.checkIfDoneLoading();
  }

  private handleUserInfo(userInfo: IUserInfo): void {
    if (userInfo.isLoggedIn) {
      this.loggingService.setAuthenticatedUserContext(userInfo.username);
    } else {
      this.loggingService.clearAuthenticatedUserContext();
    }

    this.hasUserInfo = true;
    this.checkIfDoneLoading();
  }

  private checkIfDoneLoading(): void {
    if (this.hasSettings && this.hasUserInfo) {
      this.spinnerService.hide();
      this.isLoading = false;
    }
  }
}
