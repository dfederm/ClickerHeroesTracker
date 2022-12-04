import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { FormsModule } from "@angular/forms";
import { RouterModule, Routes, UrlSegment, UrlMatchResult } from "@angular/router";
import { HttpClientModule } from "@angular/common/http";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { ClipboardModule } from "ngx-clipboard";
import { AdsenseModule } from "ng2-adsense";
import { ApplicationInsightsModule, AppInsightsService } from "@markpieszak/ng-application-insights";
import { NgChartsModule } from "ng2-charts";
import { ValidateEqualModule } from "ng-validate-equal";
import { JwBootstrapSwitchNg2Module } from "jw-bootstrap-switch-ng2";
import { NgxSpinnerModule } from "ngx-spinner";
import { SocialLoginModule, SocialAuthServiceConfig, GoogleLoginProvider, MicrosoftLoginProvider, FacebookLoginProvider } from '@abacritt/angularx-social-login';

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
import { TimeAgoPipe } from "./pipes/timeAgoPipe";

import { environment } from "./environments/environment";
import { HTTP_INTERCEPTORS } from "@angular/common/http";
import { DeveloperHttpInterceptor } from "./services/developerHttpInterceptor/developerHttpInterceptor";

// Custom url matching for legacy calculation urls. Angular doesn't have great built-in rules for this.
// This is an exported function because Angular AOT is terrible and can't handle it otherwise.
export function legacyCalculatorMatcher(segments: UrlSegment[]): UrlMatchResult | null {
  if (segments.length !== 2) {
    return null;
  }

  // Matches urls like /Calculator/View?uploadId=195791 and /calculator/view?uploadId=377358
  if ((segments[0].path === "Calculator" && segments[1].path === "View")
    || (segments[0].path === "calculator" && segments[1].path === "view")) {
    // Shenanigans. Angular doesn't seem to give url matchers a way to get at the query params
    let query = window.location.search;
    if (query.startsWith("?uploadId=")) {
      let uploadId = query.substring(10);
      return {
        consumed: segments,
        posParams: {
          id: new UrlSegment(uploadId, {}),
        },
      };
    }
  }

  return null;
}

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

    // Legacy route redirection
    { path: "beta", redirectTo: "/" },
    { path: "Home/New", redirectTo: "/news" },
    { path: "Upload", redirectTo: "/" }, // TODO: find a way to open the upload dialog
    { path: "Manage", redirectTo: "/" }, // TODO: find a way to open the settings dialog
    { path: "Account/Login", redirectTo: "/" }, // TODO: find a way to open the log in dialog
    { path: "Account/ForgotPassword", redirectTo: "/" }, // TODO: find a way to open the reset password dialog
    { path: "Account/Register", redirectTo: "/" }, // TODO: find a way to open the register dialog
    { matcher: legacyCalculatorMatcher, redirectTo: "uploads/:id" },

    // Catch-all
    { path: "**", component: NotFoundComponent },
  ];

@NgModule({
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    RouterModule.forRoot(routes),
    HttpClientModule,
    NgbModule,
    ClipboardModule,
    AdsenseModule.forRoot(),
    // Make sure this matches the API settings as well. Is there a better way to do this?
    ApplicationInsightsModule.forRoot({ instrumentationKey: "99fba640-790d-484f-83c4-3c97450d8698" }),
    NgChartsModule,
    ValidateEqualModule,
    JwBootstrapSwitchNg2Module,
    NgxSpinnerModule,
    SocialLoginModule,
  ],
  declarations: [
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
    TimeAgoPipe,
  ],
  providers: [
    AppInsightsService,
    ...(environment.production ? [] : [
      { provide: HTTP_INTERCEPTORS, useClass: DeveloperHttpInterceptor, multi: true },
    ]),
    {
      provide: 'SocialAuthServiceConfig',
      useValue: {
        autoLogin: false,
        providers: [
          {
            id: GoogleLoginProvider.PROVIDER_ID,
            provider: new GoogleLoginProvider(
              '371697338749-cbgs417cd45vgktq0kmjanbn3lh2lbl6.apps.googleusercontent.com',
              {
                oneTapEnabled: false,
              }
            )
          },
          {
            id: FacebookLoginProvider.PROVIDER_ID,
            provider: new FacebookLoginProvider('246885142330300')
          },
          {
            id: MicrosoftLoginProvider.PROVIDER_ID,
            provider: new MicrosoftLoginProvider('4ecf3d26-e844-4855-9158-b8f6c0121b50', {
              scopes: ["openid", "email"],
              redirect_uri: 'http://localhost:4200',
              logout_redirect_uri: 'http://localhost:4200/logout'
            }),
          }
        ],
        onError: (err) => {
          console.error(err);
        }
      } as SocialAuthServiceConfig,
    }],
  bootstrap: [AppComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class AppModule { }
