import { NgModule } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";
import { HttpClientModule } from "@angular/common/http";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { ClipboardModule } from "ngx-clipboard";
import { TimeAgoPipeModule } from "time-ago-pipe";
import { AdsenseModule } from "ng2-adsense";
import { ApplicationInsightsModule, AppInsightsService } from "@markpieszak/ng-application-insights";
import { ChartsModule } from "ng2-charts";
import { CompareValidatorModule } from "angular-compare-validator";
import { JWBootstrapSwitchModule } from "jw-bootstrap-switch-ng2";
import { LoadingModule } from "ngx-loading";

import { AppComponent } from "./components/app/app";
import { HomeComponent } from "./components/home/home";
import { NewsComponent } from "./components/news/news";
import { ChangelogComponent } from "./components/changelog/changelog";
import { AdComponent } from "./components/ad/ad";
import { NavbarComponent } from "./components/navbar/navbar";
import { LogInDialogComponent } from "./components/logInDialog/logInDialog";
import { UploadDialogComponent } from "./components/uploadDialog/uploadDialog";
import { UserComponent } from "./components/user/user";
import { UploadsTableComponent } from "./components/uploadsTable/uploadsTable";
import { UserUploadsComponent } from "./components/userUploads/userUploads";
import { UploadComponent } from "./components/upload/upload";
import { ClansComponent } from "./components/clans/clans";
import { UserProgressComponent } from "./components/userProgress/userProgress";
import { UserCompareComponent } from "./components/userCompare/userCompare";
import { BannerComponent } from "./components/banner/banner";
import { RegisterDialogComponent } from "./components/registerDialog/registerDialog";
import { ExternalLoginsComponent } from "./components/externalLogins/externalLogins";
import { FeedbackDialogComponent } from "./components/feedbackDialog/feedbackDialog";
import { ResetPasswordDialogComponent } from "./components/resetPasswordDialog/resetPasswordDialog";
import { SettingsDialogComponent } from "./components/settingsDialog/settingsDialog";
import { ChangePasswordDialogComponent } from "./components/changePasswordDialog/changePasswordDialog";
import { AdminComponent } from "./components/admin/admin";
import { NotFoundComponent } from "./components/notFound/notFound";
import { AncientSuggestionsComponent } from "./components/ancientSuggestions/ancientSuggestions";
import { OutsiderSuggestionsComponent } from "./components/outsiderSuggestions/outsiderSuggestions";
import { AscensionZoneComponent } from "./components/ascensionZone/ascensionZone";
import { ClanComponent } from "./components/clan/clan";

import { OpenDialogDirective } from "./directives/openDialog/openDialog";

import { ExponentialPipe } from "./pipes/exponentialPipe";

import { NewsService } from "./services/newsService/newsService";
import { AuthenticationService } from "./services/authenticationService/authenticationService";
import { UploadService } from "./services/uploadService/uploadService";
import { ClanService } from "./services/clanService/clanService";
import { UserService } from "./services/userService/userService";
import { VersionService } from "./services/versionService/versionService";
import { SettingsService } from "./services/settingsService/settingsService";
import { FeedbackService } from "./services/feedbackService/feedbackService";
import { HttpErrorHandlerService } from "./services/httpErrorHandlerService/httpErrorHandlerService";

const routes: Routes =
  [
    { path: "", pathMatch: "full", component: HomeComponent },
    { path: "news", component: NewsComponent },
    { path: "uploads/:id", component: UploadComponent },
    { path: "clans", component: ClansComponent },
    { path: "clans/:clanName", component: ClanComponent },
    { path: "users/:userName", component: UserComponent },
    { path: "users/:userName/uploads", component: UserUploadsComponent },
    { path: "users/:userName/progress", component: UserProgressComponent },
    { path: "users/:userName/compare/:compareUserName", component: UserCompareComponent },
    { path: "admin", component: AdminComponent },
    { path: "**", component: NotFoundComponent },
  ];

@NgModule({
  imports:
    [
      BrowserModule,
      FormsModule,
      RouterModule.forRoot(routes),
      HttpClientModule,
      NgbModule.forRoot(),
      ClipboardModule,
      AdsenseModule.forRoot(),
      // Make sure this matches the API settings as well. Is there a better way to do this?
      ApplicationInsightsModule.forRoot({ instrumentationKey: "99fba640-790d-484f-83c4-3c97450d8698" }),
      ChartsModule,
      CompareValidatorModule,
      JWBootstrapSwitchModule,
      LoadingModule,
      TimeAgoPipeModule,
    ],
  declarations:
    [
      AppComponent,
      HomeComponent,
      NewsComponent,
      ChangelogComponent,
      AdComponent,
      NavbarComponent,
      LogInDialogComponent,
      UploadDialogComponent,
      OpenDialogDirective,
      UserComponent,
      UploadsTableComponent,
      UserUploadsComponent,
      UploadComponent,
      ExponentialPipe,
      ClansComponent,
      UserProgressComponent,
      UserCompareComponent,
      BannerComponent,
      RegisterDialogComponent,
      ExternalLoginsComponent,
      FeedbackDialogComponent,
      ResetPasswordDialogComponent,
      SettingsDialogComponent,
      ChangePasswordDialogComponent,
      AdminComponent,
      NotFoundComponent,
      AncientSuggestionsComponent,
      OutsiderSuggestionsComponent,
      AscensionZoneComponent,
      ClanComponent,
    ],
  entryComponents:
    [
      LogInDialogComponent,
      UploadDialogComponent,
      RegisterDialogComponent,
      FeedbackDialogComponent,
      ResetPasswordDialogComponent,
      SettingsDialogComponent,
      ChangePasswordDialogComponent,
    ],
  providers:
    [
      NewsService,
      AuthenticationService,
      UploadService,
      ClanService,
      AppInsightsService,
      UserService,
      VersionService,
      SettingsService,
      FeedbackService,
      HttpErrorHandlerService,
    ],
  bootstrap: [AppComponent],
})
export class AppModule { }
