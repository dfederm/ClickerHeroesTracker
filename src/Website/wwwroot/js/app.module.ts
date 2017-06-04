import { NgModule } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";
import { HttpModule } from "@angular/http";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";

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

import { OpenDialogDirective } from "./directives/openDialog/openDialog";

import { NewsService } from "./services/newsService/newsService";
import { AuthenticationService } from "./services/authenticationService/authenticationService";
import { UploadService } from "./services/uploadService/uploadService";

const routes: Routes =
[
  { path: "", pathMatch: "full", component: HomeComponent },
  { path: "news",  component: NewsComponent },
  { path: "dashboard",  component: DashboardComponent },
  { path: "uploads",  component: UploadsComponent },
];

@NgModule({
  imports:
  [
    BrowserModule,
    FormsModule,
    RouterModule.forRoot(routes),
    HttpModule,
    NgbModule.forRoot(),
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
  ],
  entryComponents:
  [
    LogInDialogComponent,
    UploadDialogComponent,
  ],
  providers:
  [
    NewsService,
    AuthenticationService,
    UploadService,
  ],
  bootstrap: [ AppComponent ],
})
export class AppModule { }
