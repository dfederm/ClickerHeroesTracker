import { Component } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { RegisterDialogComponent } from "../registerDialog/registerDialog";
import { ResetPasswordDialogComponent } from "../resetPasswordDialog/resetPasswordDialog";

@Component({
    selector: "logInDialog",
    templateUrl: "./logInDialog.html",
})
export class LogInDialogComponent {
    public error: string;

    public username = "";

    public password = "";

    public RegisterDialogComponent = RegisterDialogComponent;

    public ResetPasswordDialogComponent = ResetPasswordDialogComponent;

    constructor(
        private authenticationService: AuthenticationService,
        public activeModal: NgbActiveModal,
    ) { }

    public logIn(): void {
        this.error = null;
        this.authenticationService.logInWithPassword(this.username, this.password)
            .then(() => {
                this.activeModal.close();
            })
            .catch(() => {
                this.error = "Incorrect username or password";
            });
    }
}
