import { Component } from "@angular/core";
import { ChangelogComponent } from "../changelog/changelog";

@Component({
  selector: "news",
  templateUrl: "./news.html",
  imports: [
    ChangelogComponent,
  ],
  standalone: true,
})
export class NewsComponent { }
