import { Component } from "@angular/core";
import { ChangelogComponent } from "../changelog/changelog";

@Component({
    selector: "news",
    templateUrl: "./news.html",
    imports: [
        ChangelogComponent,
    ]
})
export class NewsComponent { }
