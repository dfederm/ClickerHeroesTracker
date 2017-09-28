import { Component, OnInit } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { Response } from "@angular/http";
import { Router } from "@angular/router";

import { LogInDialogComponent } from "../logInDialog/logInDialog";
import { RegisterDialogComponent } from "../registerDialog/registerDialog";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { UploadService } from "../../services/uploadService/uploadService";

@Component({
    selector: "upload",
    templateUrl: "./uploadDialog.html",
})
export class UploadDialogComponent implements OnInit {
    public errorMessage: string;

    public userInfo: IUserInfo;

    public playStyles: string[] = ["Idle", "Hybrid", "Active"];

    public encodedSaveData = "";

    // TODO: set based on user settings
    public playStyle = "Idle";

    public addToProgress = true;

    public LogInDialogComponent = LogInDialogComponent;

    public RegisterDialogComponent = RegisterDialogComponent;

    constructor(
        private authenticationService: AuthenticationService,
        private uploadService: UploadService,
        public activeModal: NgbActiveModal,
        private router: Router,
    ) { }

    public ngOnInit(): void {
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => this.userInfo = userInfo);
    }

    public upload(): void {
        if (!this.encodedSaveData) {
            this.errorMessage = "Save data is required";
            return;
        }

        this.uploadService.create(this.encodedSaveData, this.addToProgress, this.playStyle)
            .then(uploadId => {
                return this.router.navigate(["/upload", uploadId]);
            })
            .then(() => {
                this.activeModal.close();
            })
            .catch((error: Response) => {
                this.errorMessage = error.status >= 400 && error.status < 500
                    ? "The uploaded save was not valid"
                    : "An unknown error occurred";
            });
    }
}
