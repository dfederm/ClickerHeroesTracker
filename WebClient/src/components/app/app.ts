import { Component, OnInit } from "@angular/core";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { UserAgentApplication } from "msal";

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
  ) { }

  public ngOnInit(): void {
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

    // This is weird but Microsoft login redirects back to the site and expects it to create the object before it closes the popup.
    // See https://github.com/AzureAD/microsoft-authentication-library-for-js/issues/174
    if (location.hash.includes("id_token")) {
      // tslint:disable-next-line:no-unused-expression - Constructing this object has side-effects
      new UserAgentApplication("4ecf3d26-e844-4855-9158-b8f6c0121b50", null, null);
    }
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
      this.isLoading = false;
    }
  }
}
