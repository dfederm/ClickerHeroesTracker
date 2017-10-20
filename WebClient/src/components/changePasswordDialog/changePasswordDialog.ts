import { Component, OnInit } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { UserService } from "../../services/userService/userService";

@Component({
    selector: "changePasswordDialog",
    templateUrl: "./changePasswordDialog.html",
})
export class ChangePasswordDialogComponent implements OnInit {
    public errors: string[];

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
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => this.userName = userInfo.username);
    }

    public changePassword(): void {
        this.errors = null;
        this.userService.changePassword(this.userName, this.currentPassword, this.newPassword)
            .then(() => {
                this.activeModal.close();
            })
            .catch((errors: string[]) => {
                this.errors = errors;
            });
    }
}
