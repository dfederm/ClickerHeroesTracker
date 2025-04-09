import { Component } from "@angular/core";
import { NgbActiveModal, NgbModal } from "@ng-bootstrap/ng-bootstrap";

import { LogInDialogComponent } from "../logInDialog/logInDialog";
import { UserService } from "../../services/userService/userService";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";
import { FormsModule } from "@angular/forms";
import { ValidateEqualModule } from "ng-validate-equal";

@Component({
    selector: "resetPasswordDialog",
    templateUrl: "./resetPasswordDialog.html",
    imports: [
      FormsModule,
      NgxSpinnerModule,
      ValidateEqualModule,
    ],
    standalone: true,
})
export class ResetPasswordDialogComponent {
    public errors: string[];

    public email = "";

    public code = "";

    public password = "";

    public confirmPassword = "";

    public codeSent = false;

    constructor(
        private readonly userService: UserService,
        private readonly modalService: NgbModal,
        public activeModal: NgbActiveModal,
        private readonly spinnerService: NgxSpinnerService,
    ) { }

    public sendCode(): void {
        this.errors = null;
        this.spinnerService.show("resetPasswordDialog");
        this.userService.resetPassword(this.email)
            .then(() => {
                this.codeSent = true;
            })
            .catch((errors: string[]) => {
                this.errors = errors;
            })
            .finally(() => {
                this.spinnerService.hide("resetPasswordDialog");
            });
    }

    public resetPassword(): void {
        this.errors = null;
        this.spinnerService.show("resetPasswordDialog");
        this.userService.resetPasswordConfirmation(this.email, this.password, this.code)
            .then(() => {
                this.activeModal.close();
                this.modalService.open(LogInDialogComponent);
            })
            .catch((errors: string[]) => {
                this.errors = errors;
            })
            .finally(() => {
                this.spinnerService.hide("resetPasswordDialog");
            });
    }
}
