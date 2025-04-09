import { Component, OnInit } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { HttpErrorResponse } from "@angular/common/http";
import { Router } from "@angular/router";

import { LogInDialogComponent } from "../logInDialog/logInDialog";
import { RegisterDialogComponent } from "../registerDialog/registerDialog";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { UploadService } from "../../services/uploadService/uploadService";
import { SettingsService, PlayStyle } from "../../services/settingsService/settingsService";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";
import { OpenDialogDirective } from "src/directives/openDialog/openDialog";
import { TitleCasePipe } from "@angular/common";
import { FormsModule } from "@angular/forms";

@Component({
    selector: "upload",
    templateUrl: "./uploadDialog.html",
    imports: [
      FormsModule,
      NgxSpinnerModule,
      OpenDialogDirective,
      TitleCasePipe,
    ],
    standalone: true,
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
        private readonly authenticationService: AuthenticationService,
        private readonly uploadService: UploadService,
        public activeModal: NgbActiveModal,
        private readonly router: Router,
        private readonly settingsService: SettingsService,
        private readonly spinnerService: NgxSpinnerService,
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

        this.spinnerService.show("uploadDialog");
        this.uploadService.create(this.encodedSaveData, this.addToProgress, this.playStyle)
            .then(uploadId => {
                return this.router.navigate(["/uploads", uploadId]);
            })
            .then(() => {
                this.activeModal.close();
            })
            .catch((error: HttpErrorResponse) => {
                this.errorMessage = error.status >= 400 && error.status < 500
                    ? "The uploaded save was not valid"
                    : "An unknown error occurred";
            })
            .finally(() => {
                this.spinnerService.hide("uploadDialog");
            });
    }

    public uploadFile(event: Event): void {
        let fileInput = event.target as HTMLInputElement;
        let fileList = fileInput.files;
        if (fileList.length > 0) {
            let reader = new FileReader();
            let file: File = fileList[0];
            reader.onload = () => this.encodedSaveData = reader.result as string;
            reader.readAsText(file);
        }
    }
}
