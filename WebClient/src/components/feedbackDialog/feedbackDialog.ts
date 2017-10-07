import { Component, OnInit } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { FeedbackService } from "../../services/feedbackService/feedbackService";

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
        private authenticationService: AuthenticationService,
        private feedbackService: FeedbackService,
        public activeModal: NgbActiveModal,
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

        this.feedbackService.send(this.comments, email)
            .then(() => {
                this.activeModal.close();
            })
            .catch(() => {
                this.errorMessage = "Something went wrong. Please try again";
            });
    }
}
