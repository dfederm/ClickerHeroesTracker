import { NgModule } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";
import { HttpModule } from "@angular/http";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { ClipboardModule } from "ngx-clipboard/dist";
import { TimeAgoPipe } from "time-ago-pipe";
import { AdsenseModule } from "ng2-adsense";
import { ApplicationInsightsModule, AppInsightsService } from "@markpieszak/ng-application-insights";
import { ChartsModule } from "ng2-charts";
import { CompareValidatorModule } from "angular-compare-validator";

import { AppComponent } from "./components/app/app";
import { HomeComponent } from "./components/home/home";
import { NewsComponent } from "./components/news/news";
import { ChangelogComponent } from "./components/changelog/changelog";
import { AdComponent } from "./components/ad/ad";
import { NavbarComponent } from "./components/navbar/navbar";
import { LogInDialogComponent } from "./components/logInDialog/logInDialog";
import { UploadDialogComponent } from "./components/uploadDialog/uploadDialog";
import { DashboardComponent } from "./components/dashboard/dashboard";
import { UploadsTableComponent } from "./components/uploadsTable/uploadsTable";
import { UploadsComponent } from "./components/uploads/uploads";
import { UploadComponent } from "./components/upload/upload";
import { ClansComponent } from "./components/clans/clans";
import { UserProgressComponent } from "./components/userProgress/userProgress";
import { UserCompareComponent } from "./components/userCompare/userCompare";
import { BannerComponent } from "./components/banner/banner";
import { RegisterDialogComponent } from "./components/registerDialog/registerDialog";
import { ExternalLoginsComponent } from "./components/externalLogins/externalLogins";

import { OpenDialogDirective } from "./directives/openDialog/openDialog";

import { ExponentialPipe } from "./pipes/exponentialPipe";

import { NewsService } from "./services/newsService/newsService";
import { AuthenticationService } from "./services/authenticationService/authenticationService";
import { UploadService } from "./services/uploadService/uploadService";
import { ClanService } from "./services/clanService/clanService";
import { UserService } from "./services/userService/userService";
import { VersionService } from "./services/versionService/versionService";
import { SettingsService } from "./services/settingsService/settingsService";

const routes: Routes =
  [
    { path: "", pathMatch: "full", component: HomeComponent },
    { path: "news", component: NewsComponent },
    { path: "dashboard", component: DashboardComponent },
    { path: "uploads", component: UploadsComponent },
    { path: "upload/:id", component: UploadComponent },
    { path: "clans", component: ClansComponent },
    { path: "users/:userName/progress", component: UserProgressComponent },
    { path: "users/:userName/compare/:compareUserName", component: UserCompareComponent },
  ];

@NgModule({
  imports:
  [
    BrowserModule,
    FormsModule,
    RouterModule.forRoot(routes),
    HttpModule,
    NgbModule.forRoot(),
    ClipboardModule,
    AdsenseModule.forRoot(),
    // Make sure this matches the API settings as well. Is there a better way to do this?
    ApplicationInsightsModule.forRoot({ instrumentationKey: "99fba640-790d-484f-83c4-3c97450d8698" }),
    ChartsModule,
    CompareValidatorModule,
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
    DashboardComponent,
    UploadsTableComponent,
    UploadsComponent,
    UploadComponent,
    ExponentialPipe,
    ClansComponent,
    TimeAgoPipe,
    UserProgressComponent,
    UserCompareComponent,
    BannerComponent,
    RegisterDialogComponent,
    ExternalLoginsComponent,
  ],
  entryComponents:
  [
    LogInDialogComponent,
    UploadDialogComponent,
    RegisterDialogComponent,
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
  ],
  bootstrap: [AppComponent],
})
export class AppModule { }
