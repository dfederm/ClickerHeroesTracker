import { Component } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { Http, Response, RequestOptions, Headers } from "@angular/http";
import { Router } from "@angular/router";

import { LogInDialogComponent } from "../logInDialog/logInDialog";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";

@Component({
    selector: "upload",
    templateUrl: "./js/components/uploadDialog/uploadDialog.html",
})
export class UploadDialogComponent
{
    public errorMessage: string;

    public isLoggedIn: boolean;

    public playStyles: string[] = ["Idle", "Hybrid", "Active"];

    public encodedSaveData: string = "";

    // TODO: set based on user settings
    public playStyle: string = "Idle";

    public addToProgress: boolean = true;

    public LogInDialogComponent = LogInDialogComponent;

    constructor(
        private authenticationService: AuthenticationService,
        public activeModal: NgbActiveModal,
        private http: Http,
        private router: Router,
    ) { }

    public ngOnInit(): void
    {
        this.authenticationService
            .isLoggedIn()
            .subscribe(isLoggedIn => this.isLoggedIn = isLoggedIn);
    }

    public upload(): void
    {
        if (!this.encodedSaveData)
        {
            this.errorMessage = "Save data is required";
            return;
        }

        let headersPromise = this.isLoggedIn
            ? this.authenticationService.getAuthHeaders()
            : Promise.resolve(new Headers());
        headersPromise
            .then(headers =>
            {
                headers.append("Content-Type", "application/x-www-form-urlencoded");
                let options = new RequestOptions({ headers: headers });
                let params = new URLSearchParams();
                params.append("encodedSaveData", this.encodedSaveData);
                params.append("addToProgress", (this.addToProgress && this.isLoggedIn).toString());
                params.append("playStyle", this.playStyle);
                return this.http
                    .post("/api/uploads", params.toString(), options)
                    .toPromise();
            })
            .then(result =>
            {
                const uploadId = parseInt(result.text());
                if (uploadId)
                {
                    this.router.navigate(["/calculator/view", uploadId]);
                }
            })
            .catch((error: Response) =>
            {
                appInsights.trackEvent("UploadDialog.upload.error", { message: error.toString() });
                this.errorMessage = error.status >= 400 && error.status < 500
                    ? "The uploaded save was not valid"
                    : "An unknown error occurred";
            });
    }
}
