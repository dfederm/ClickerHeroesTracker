import { NgModule } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";
import { HttpModule } from "@angular/http";

import { AppComponent } from "./components/app/app";
import { HomeComponent } from "./components/home/home";
import { NewsComponent } from "./components/news/news";
import { ChangelogComponent } from "./components/changelog/changelog";

const routes: Routes = [
  { path: "", component: HomeComponent, pathMatch: "full" },
  { path: "news",  component: NewsComponent },
  // Remove these once the beta is over
  { path: "Home/Beta", redirectTo: "beta" },
  { path: "beta", component: HomeComponent },
];

@NgModule({
  imports: [
    BrowserModule,
    FormsModule,
    RouterModule.forRoot(routes),
    HttpModule,
  ],
  declarations: [
    AppComponent,
    HomeComponent,
    ChangelogComponent,
    NewsComponent,
  ],
  providers: [ ],
  bootstrap: [ AppComponent ],
})
export class AppModule { }
