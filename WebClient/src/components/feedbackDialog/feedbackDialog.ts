import { Component, OnInit } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { FeedbackService } from "../../services/feedbackService/feedbackService";
import { NgxSpinnerService } from "ngx-spinner";

@Component({
    selector: "feedback",
    templateUrl: "./feedbackDialog.html",
})
export class FeedbackDialogComponent implements OnInit {
    public errorMessage: string;

    public userInfo: IUserInfo;

    public comments = "";

    public email = "";

    constructor(
        private readonly authenticationService: AuthenticationService,
        private readonly feedbackService: FeedbackService,
        public activeModal: NgbActiveModal,
        private readonly spinnerService: NgxSpinnerService,
    ) { }

    public ngOnInit(): void {
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => this.userInfo = userInfo);
    }

    public submit(): void {
        let email = this.userInfo.isLoggedIn
            ? this.userInfo.email
            : this.email;

        this.spinnerService.show("feedbackDialog");
        this.feedbackService.send(this.comments, email)
            .then(() => {
                this.activeModal.close();
            })
            .catch(() => {
                this.errorMessage = "Something went wrong. Please try again";
            })
            .finally(() => {
                this.spinnerService.hide("feedbackDialog");
            });
    }
}
