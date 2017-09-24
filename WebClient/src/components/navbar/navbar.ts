import { Component, OnInit } from "@angular/core";

import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { RegisterDialogComponent } from "../registerDialog/registerDialog";
import { LogInDialogComponent } from "../logInDialog/logInDialog";
import { UploadDialogComponent } from "../uploadDialog/uploadDialog";

// tslint:disable-next-line:no-namespace
declare global {
    // tslint:disable-next-line:interface-name - We don't own this interface name, just extending it
    interface Window {
        adsbygoogle: object[];
    }
}

@Component({
    selector: "navbar",
    templateUrl: "./navbar.html",
})
export class NavbarComponent implements OnInit {
    public isCollapsed = true;

    public isLoggedIn: boolean;

    public RegisterDialogComponent = RegisterDialogComponent;
    public LogInDialogComponent = LogInDialogComponent;
    public UploadDialogComponent = UploadDialogComponent;

    constructor(
        private authenticationService: AuthenticationService,
    ) { }

    public ngOnInit(): void {
        this.authenticationService
            .isLoggedIn()
            .subscribe(isLoggedIn => this.isLoggedIn = isLoggedIn);
    }

    public logOut(): void {
        this.authenticationService.logOut();
    }
}
