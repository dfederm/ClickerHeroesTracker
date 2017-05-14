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

import { OpenDialogDirective } from "./directives/openDialog/openDialog";

import { NewsService } from "./services/newsService/newsService";
import { AuthenticationService } from "./services/authenticationService/authenticationService";

const routes: Routes = [
  { path: "", redirectTo: "beta", pathMatch: "full" },
  { path: "news",  component: NewsComponent },
  // Remove these once the beta is over
  { path: "Home/Beta", redirectTo: "beta" },
  { path: "beta", component: HomeComponent },
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
  ],
  bootstrap: [ AppComponent ],
})
export class AppModule { }
