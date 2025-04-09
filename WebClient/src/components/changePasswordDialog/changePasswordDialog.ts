import { Component, OnInit } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { UserService, IUserLogins } from "../../services/userService/userService";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";
import { ExternalLoginsComponent } from "../externalLogins/externalLogins";
import { FormsModule } from "@angular/forms";
import { ValidateEqualModule } from "ng-validate-equal";

@Component({
    selector: "changePasswordDialog",
    templateUrl: "./changePasswordDialog.html",
    imports: [
        ExternalLoginsComponent,
        FormsModule,
        NgxSpinnerModule,
        ValidateEqualModule,
    ]
})
export class ChangePasswordDialogComponent implements OnInit {
    public errors: string[];

    public logins: IUserLogins;

    public currentPassword = "";

    public newPassword = "";

    public confirmNewPassword = "";

    private userName: string;

    constructor(
        private readonly authenticationService: AuthenticationService,
        private readonly userService: UserService,
        public activeModal: NgbActiveModal,
        private readonly spinnerService: NgxSpinnerService,
    ) { }

    public ngOnInit(): void {
        this.spinnerService.show("changePassword");
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => {
                this.userName = userInfo.username;

                this.userService
                    .getLogins(this.userName)
                    .then(logins => {
                        this.logins = logins;
                    })
                    .catch(() => {
                        this.errors = ["There was an unexpected error. Please try again in a bit."];
                    })
                    .finally(() => {
                        this.spinnerService.hide("changePassword");
                    });
            });
    }

    public submit(): void {
        this.errors = null;
        this.spinnerService.show("changePassword");
        let passwordPromise = this.logins.hasPassword
            ? this.userService.changePassword(this.userName, this.currentPassword, this.newPassword)
            : this.userService.setPassword(this.userName, this.newPassword);
        passwordPromise
            .then(() => {
                this.activeModal.close();
            })
            .catch((errors: string[]) => {
                this.errors = errors;
            })
            .finally(() => {
                this.spinnerService.hide("changePassword");
            });
    }
}
