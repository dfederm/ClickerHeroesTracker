import { Component, OnInit } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { UserService, IUserLogins } from "../../services/userService/userService";

@Component({
    selector: "changePasswordDialog",
    templateUrl: "./changePasswordDialog.html",
})
export class ChangePasswordDialogComponent implements OnInit {
    public errors: string[];

    public isLoading: boolean;

    public logins: IUserLogins;

    public currentPassword = "";

    public newPassword = "";

    public confirmNewPassword = "";

    private userName: string;

    constructor(
        private authenticationService: AuthenticationService,
        private userService: UserService,
        public activeModal: NgbActiveModal,
    ) { }

    public ngOnInit(): void {
        this.isLoading = true;
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => {
                this.userName = userInfo.username;

                this.userService
                    .getLogins(this.userName)
                    .then(logins => {
                        this.isLoading = false;
                        this.logins = logins;
                    })
                    .catch(() => {
                        this.errors = ["There was an unexpected error. Please try again in a bit."];
                    });
            });
    }

    public submit(): void {
        this.errors = null;
        this.isLoading = true;
        let passwordPromise = this.logins.hasPassword
            ? this.userService.changePassword(this.userName, this.currentPassword, this.newPassword)
            : this.userService.setPassword(this.userName, this.newPassword);
        passwordPromise
            .then(() => {
                this.isLoading = false;
                this.activeModal.close();
            })
            .catch((errors: string[]) => {
                this.errors = errors;
            });
    }
}
