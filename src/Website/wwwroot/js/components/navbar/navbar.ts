import { Component, OnInit } from "@angular/core";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";

import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { LogInDialogComponent } from "../logInDialog/logInDialog";

declare global
{
    // tslint:disable-next-line:interface-name - We don't own this interface name, just extending it
    interface Window
    {
        adsbygoogle: object[];
    }
}

@Component({
    selector: "navbar",
    templateUrl: "./js/components/navbar/navbar.html",
})
export class NavbarComponent implements OnInit
{
    public isCollapsed: boolean = true;

    public isLoggedIn: boolean;

    constructor(
        private authenticationService: AuthenticationService,
        private modalService: NgbModal,
    ) { }

    public ngOnInit(): void
    {
        this.authenticationService
            .isLoggedIn()
            .subscribe(isLoggedIn => this.isLoggedIn = isLoggedIn);
    }

    public openLogInDialog(): void
    {
        this.modalService.open(LogInDialogComponent);
    }

    public logOut(): void
    {
        this.authenticationService.logOut();
    }
}
