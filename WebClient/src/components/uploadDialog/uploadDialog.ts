import { Component, OnInit } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { Response } from "@angular/http";
import { Router } from "@angular/router";

import { LogInDialogComponent } from "../logInDialog/logInDialog";
import { RegisterDialogComponent } from "../registerDialog/registerDialog";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { UploadService } from "../../services/uploadService/uploadService";
import { SettingsService, PlayStyle } from "../../services/settingsService/settingsService";

@Component({
    selector: "upload",
    templateUrl: "./uploadDialog.html",
})
export class UploadDialogComponent implements OnInit {
    public errorMessage: string;

    public userInfo: IUserInfo;

    public playStyles: PlayStyle[] = ["idle", "hybrid", "active"];

    public encodedSaveData = "";

    public playStyle: PlayStyle = "idle";

    public addToProgress = true;

    public LogInDialogComponent = LogInDialogComponent;

    public RegisterDialogComponent = RegisterDialogComponent;

    constructor(
        private authenticationService: AuthenticationService,
        private uploadService: UploadService,
        public activeModal: NgbActiveModal,
        private router: Router,
        private settingsService: SettingsService,
    ) { }

    public ngOnInit(): void {
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => this.userInfo = userInfo);

        this.settingsService
            .settings()
            .subscribe(settings => this.playStyle = settings.playStyle);
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
