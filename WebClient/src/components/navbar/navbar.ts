import { Component, OnInit } from "@angular/core";
import { Router, NavigationEnd } from "@angular/router";

import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { RegisterDialogComponent } from "../registerDialog/registerDialog";
import { LogInDialogComponent } from "../logInDialog/logInDialog";
import { UploadDialogComponent } from "../uploadDialog/uploadDialog";
import { FeedbackDialogComponent } from "../feedbackDialog/feedbackDialog";
import { SettingsDialogComponent } from "../settingsDialog/settingsDialog";

@Component({
    selector: "navbar",
    templateUrl: "./navbar.html",
})
export class NavbarComponent implements OnInit {
    public isCollapsed = true;

    public userInfo: IUserInfo;

    public RegisterDialogComponent = RegisterDialogComponent;
    public LogInDialogComponent = LogInDialogComponent;
    public UploadDialogComponent = UploadDialogComponent;
    public FeedbackDialogComponent = FeedbackDialogComponent;
    public SettingsDialogComponent = SettingsDialogComponent;

    constructor(
        private authenticationService: AuthenticationService,
        private router: Router,
    ) { }

    public ngOnInit(): void {
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => this.userInfo = userInfo);

        this.router.events.subscribe((event) => {
            if (event instanceof NavigationEnd) {
                this.isCollapsed = true;
            }
        });
    }

    public logOut($event: MouseEvent): void {
        $event.preventDefault();
        this.authenticationService.logOut();
    }
}
