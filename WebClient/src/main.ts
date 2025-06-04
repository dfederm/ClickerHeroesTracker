import { enableProdMode, ErrorHandler, importProvidersFrom } from '@angular/core';

import { environment } from './environments/environment';
import { ErrorHandlerService } from './services/errorHandlerService/errorHandlerService';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { DeveloperHttpInterceptor } from './services/developerHttpInterceptor/developerHttpInterceptor';
import { BrowserModule, bootstrapApplication } from '@angular/platform-browser';
import { provideAnimations } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { provideRouter, Routes, UrlSegment, UrlMatchResult } from '@angular/router';
import { HomeComponent } from './components/home/home';
import { NewsComponent } from './components/news/news';
import { UploadComponent } from './components/upload/upload';
import { ClansComponent } from './components/clans/clans';
import { ClanComponent } from './components/clan/clan';
import { UserComponent } from './components/user/user';
import { UserUploadsComponent } from './components/userUploads/userUploads';
import { UserProgressComponent } from './components/userProgress/userProgress';
import { UserCompareComponent } from './components/userCompare/userCompare';
import { AdminComponent } from './components/admin/admin';
import { NotFoundComponent } from './components/notFound/notFound';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { ClipboardModule } from 'ngx-clipboard';
import { AdsenseModule } from 'ng2-adsense';
import { ValidateEqualModule } from 'ng-validate-equal';
import { NgxSpinnerModule } from 'ngx-spinner';
import { AppComponent } from './components/app/app';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

// Custom url matching for legacy calculation urls. Angular doesn't have great built-in rules for this.
// This is an exported function because Angular AOT is terrible and can't handle it otherwise.
function legacyCalculatorMatcher(segments: UrlSegment[]): UrlMatchResult | null {
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

const routes: Routes = [
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

if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, {
    providers: [
        importProvidersFrom(BrowserModule, FormsModule, NgbModule, ClipboardModule, AdsenseModule.forRoot(), ValidateEqualModule, NgxSpinnerModule),
        { provide: ErrorHandler, useClass: ErrorHandlerService },
        ...(environment.production ? [] : [
            { provide: HTTP_INTERCEPTORS, useClass: DeveloperHttpInterceptor, multi: true },
        ]),
        provideAnimations(),
        provideRouter(routes),
        provideHttpClient(withInterceptorsFromDi()),
        provideCharts(withDefaultRegisterables()),
    ]
}).catch(err => console.error(err));
