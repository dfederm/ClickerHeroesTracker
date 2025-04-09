import { Component, OnInit } from "@angular/core";
import { UploadDialogComponent } from "../uploadDialog/uploadDialog";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { UploadsTableComponent } from "../uploadsTable/uploadsTable";
import { RouterLink } from "@angular/router";
import { ChangelogComponent } from "../changelog/changelog";
import { OpenDialogDirective } from "src/directives/openDialog/openDialog";

@Component({
    selector: "home",
    templateUrl: "./home.html",
    imports: [
        ChangelogComponent,
        OpenDialogDirective,
        RouterLink,
        UploadsTableComponent,
    ]
})
export class HomeComponent implements OnInit {
    public UploadDialogComponent = UploadDialogComponent;

    public userName: string;

    constructor(
        private readonly authenticationService: AuthenticationService,
    ) { }

    public ngOnInit(): void {
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => this.userName = userInfo.isLoggedIn ? userInfo.username : null);
    }
}
