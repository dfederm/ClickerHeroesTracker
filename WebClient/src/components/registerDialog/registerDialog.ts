import { Component } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

import { LogInDialogComponent } from "../logInDialog/logInDialog";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { UserService } from "../../services/userService/userService";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";
import { ExternalLoginsComponent } from "../externalLogins/externalLogins";
import { OpenDialogDirective } from "src/directives/openDialog/openDialog";
import { FormsModule } from "@angular/forms";
import { ValidateEqualModule } from "ng-validate-equal";

@Component({
    selector: "registerDialog",
    templateUrl: "./registerDialog.html",
    imports: [
        ExternalLoginsComponent,
        FormsModule,
        NgxSpinnerModule,
        OpenDialogDirective,
        ValidateEqualModule,
    ]
})
export class RegisterDialogComponent {
    public errors: string[];

    public username = "";

    public email = "";

    public password = "";

    public confirmPassword = "";

    public LogInDialogComponent = LogInDialogComponent;

    constructor(
        private readonly authenticationService: AuthenticationService,
        private readonly userService: UserService,
        public activeModal: NgbActiveModal,
        private readonly spinnerService: NgxSpinnerService,
    ) { }

    public register(): void {
        this.errors = null;
        this.spinnerService.show("registerDialog");
        this.userService.create(this.username, this.email, this.password)
            .then(() => {
                return this.authenticationService.logInWithPassword(this.username, this.password)
                    .then(() => {
                        this.activeModal.close();
                    })
                    .catch(() => {
                        this.errors = ["Something went wrong. Your account was created but but we had trouble logging you in. Please try logging in with your new account."];
                        return Promise.resolve();
                    });
            })
            .catch((errors: string[]) => {
                this.errors = errors;
            })
            .finally(() => {
                this.spinnerService.hide("registerDialog");
            });
    }
}
