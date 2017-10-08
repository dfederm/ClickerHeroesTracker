import { Component } from "@angular/core";
import { NgbActiveModal, NgbModal } from "@ng-bootstrap/ng-bootstrap";

import { LogInDialogComponent } from "../logInDialog/logInDialog";
import { UserService } from "../../services/userService/userService";

@Component({
    selector: "resetPasswordDialog",
    templateUrl: "./resetPasswordDialog.html",
})
export class ResetPasswordDialogComponent {
    public errors: string[];

    public email = "";

    public code = "";

    public password = "";

    public confirmPassword = "";

    public codeSent = false;

    constructor(
        private userService: UserService,
        private modalService: NgbModal,
        public activeModal: NgbActiveModal,
    ) { }

    public sendCode(): void {
        this.errors = null;
        this.userService.resetPassword(this.email)
            .then(() => {
                this.codeSent = true;
            })
            .catch((errors: string[]) => {
                this.errors = errors;
            });
    }

    public resetPassword(): void {
        this.errors = null;
        this.userService.resetPasswordConfirmation(this.email, this.password, this.code)
            .then(() => {
                this.activeModal.close();
                this.modalService.open(LogInDialogComponent);
            })
            .catch((errors: string[]) => {
                this.errors = errors;
            });
    }
}
