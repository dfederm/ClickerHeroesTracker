import { Component } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

import { AuthenticationService } from "../../services/authenticationService/authenticationService";

@Component({
    selector: "logInDialog",
    templateUrl: "./logInDialog.html",
})
export class LogInDialogComponent {
    public isError = false;

    public username = "";

    public password = "";

    constructor(
        private authenticationService: AuthenticationService,
        public activeModal: NgbActiveModal,
    ) { }

    public logIn(): void {
        this.authenticationService.logIn(this.username, this.password)
            .then(() => {
                this.activeModal.close();
            })
            .catch(() => {
                this.isError = true;
            });
    }
}
