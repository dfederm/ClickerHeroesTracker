import { Component, OnInit } from "@angular/core";

import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { RegisterDialogComponent } from "../registerDialog/registerDialog";
import { LogInDialogComponent } from "../logInDialog/logInDialog";
import { UploadDialogComponent } from "../uploadDialog/uploadDialog";

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

    constructor(
        private authenticationService: AuthenticationService,
    ) { }

    public ngOnInit(): void {
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => this.userInfo = userInfo);
    }

    public logOut(): void {
        this.authenticationService.logOut();
    }
}
