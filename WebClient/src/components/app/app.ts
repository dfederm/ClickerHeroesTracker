import { Component, OnInit } from "@angular/core";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
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
    light: "https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-beta.2/css/bootstrap.min.css",
    dark: "https://bootswatch.com/slate/bootstrap.min.css",
  };

  public isLoading: boolean;

  private hasSettings: boolean;

  private hasUserInfo: boolean;

  constructor(
    private settingsService: SettingsService,
    private authenticationService: AuthenticationService,
    private appInsights: AppInsightsService,
  ) { }

  public ngOnInit(): void {
    this.isLoading = true;

    this.settingsService
      .settings()
      .subscribe(settings => this.handleSettings(settings));

    this.authenticationService
      .userInfo()
      .subscribe(userInfo => this.handleUserInfo(userInfo));
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
