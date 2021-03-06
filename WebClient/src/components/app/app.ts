import { Component, OnInit } from "@angular/core";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { NgxSpinnerService } from "ngx-spinner";

import { SettingsService, IUserSettings } from "../../services/settingsService/settingsService";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";

@Component({
  selector: "app",
  templateUrl: "./app.html",
})
export class AppComponent implements OnInit {
  public static defaultTheme = "light";

  // TODO: Consider getting all themes from https://bootswatch.com/api/4.json
  public static themeCssUrls: { [theme: string]: string } = {
    light: "https://maxcdn.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.min.css",
    dark: "https://maxcdn.bootstrapcdn.com/bootswatch/4.1.3/slate/bootstrap.min.css",
  };

  public isLoading: boolean;

  private hasSettings: boolean;

  private hasUserInfo: boolean;

  constructor(
    private readonly settingsService: SettingsService,
    private readonly authenticationService: AuthenticationService,
    private readonly appInsights: AppInsightsService,
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
      this.appInsights.setAuthenticatedUserContext(userInfo.username);
    } else {
      this.appInsights.clearAuthenticatedUserContext();
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
