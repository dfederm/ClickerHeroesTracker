import { Component } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

import { LogInDialogComponent } from "../logInDialog/logInDialog";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { UserService } from "../../services/userService/userService";

@Component({
    selector: "registerDialog",
    templateUrl: "./registerDialog.html",
})
export class RegisterDialogComponent {
    public errors: string[];

    public isLoading: boolean;

    public username = "";

    public email = "";

    public password = "";

    public confirmPassword = "";

    public LogInDialogComponent = LogInDialogComponent;

    constructor(
        private readonly authenticationService: AuthenticationService,
        private readonly userService: UserService,
        public activeModal: NgbActiveModal,
    ) { }

    public register(): void {
        this.errors = null;
        this.isLoading = true;
        this.userService.create(this.username, this.email, this.password)
            .then(() => {
                return this.authenticationService.logInWithPassword(this.username, this.password)
                    .then(() => {
                        this.isLoading = false;
                        this.activeModal.close();
                    })
                    .catch(() => {
                        this.errors = ["Something went wrong. Your account was created but but we had trouble logging you in. Please try logging in with your new account."];
                    });
            })
            .catch((errors: string[]) => {
                this.errors = errors;
            });
    }
}
