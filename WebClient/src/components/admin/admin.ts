import { Component, OnInit } from "@angular/core";
import { Http, RequestOptions } from "@angular/http";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { UploadService } from "../../services/uploadService/uploadService";

import "rxjs/add/operator/toPromise";

export interface IUploadQueueStats {
    priority: string;
    numMessages: number;
}

export interface IRecomputeRequest {
    uploadIds: number[];
    priority: string;
}

export interface IClearQueueRequest {
    priority: string;
}

// This actually pretty generic, so if it's used elsewhere consider moving it somewhere more generic
export interface IValidationErrorResponse {
    [field: string]: string[];
}

@Component({
    selector: "admin",
    templateUrl: "./admin.html",
})
export class AdminComponent implements OnInit {
    public static numParallelDeletes = 10;

    public isLoadingQueues: boolean;

    public queues: IUploadQueueStats[];

    public recomputeError: string;

    public isRecomputeLoading: boolean;

    public recomputeUploadIds: string;

    public recomputePriority: string;

    public clearQueueError: string;

    public isClearQueueLoading: boolean;

    public clearQueuePriority: string;

    public staleUploadError: string;

    public isStaleUploadsLoading: boolean;

    public staleUploadIds: number[];

    public deletesInProgress: boolean;

    public deletedStaleUploads: number;

    public totalStaleUploads: number;

    constructor(
        private authenticationService: AuthenticationService,
        private http: Http,
        private appInsights: AppInsightsService,
        private uploadService: UploadService,
    ) { }

    public ngOnInit(): void {
        this.refreshQueueData()
            .catch(() => {
                this.recomputeError = this.clearQueueError = "Could not fetch queue data";
            });
    }

    public recompute(): void {
        this.recomputeError = null;

        let uploadIds: number[] = [];
        let uploadIdsRaw = this.recomputeUploadIds.split(",");
        for (let i = 0; i < uploadIdsRaw.length; i++) {
            let uploadId = parseInt(uploadIdsRaw[i]);
            if (isNaN(uploadId)) {
                this.recomputeError = `${uploadIdsRaw[i]} is not a number.`;
                return;
            }

            uploadIds.push(uploadId);
        }

        this.isRecomputeLoading = true;
        this.authenticationService.getAuthHeaders()
            .then(headers => {
                headers.append("Content-Type", "application/json");
                let options = new RequestOptions({ headers });
                let body: IRecomputeRequest = {
                    uploadIds,
                    priority: this.recomputePriority,
                };
                return this.http
                    .post("/api/admin/recompute", body, options)
                    .toPromise();
            })
            .then(() => {
                this.isRecomputeLoading = false;
                this.refreshQueueData();
            })
            .catch(error => {
                let errors: string[] = [];

                let validationErrorResponse: IValidationErrorResponse;
                try {
                    validationErrorResponse = error.json();
                } catch (error) {
                    // It must not have been json
                }

                if (validationErrorResponse) {
                    for (let field in validationErrorResponse) {
                        errors.push(...validationErrorResponse[field]);
                    }
                } else {
                    errors.push(error.message || error.toString());
                }

                this.appInsights.trackEvent("AdminComponent.recompute.error", { message: errors.join(";") });
                this.recomputeError = errors.join(";");
            });
    }

    public clearQueue(): void {
        this.clearQueueError = null;
        this.isClearQueueLoading = true;

        this.authenticationService.getAuthHeaders()
            .then(headers => {
                headers.append("Content-Type", "application/json");
                let options = new RequestOptions({ headers });
                let body: IClearQueueRequest = {
                    priority: this.clearQueuePriority,
                };
                return this.http
                    .post("/api/admin/clearqueue", body, options)
                    .toPromise();
            })
            .then(() => {
                this.isClearQueueLoading = false;
                this.refreshQueueData();
            })
            .catch(error => {
                let errors: string[] = [];

                let validationErrorResponse: IValidationErrorResponse;
                try {
                    validationErrorResponse = error.json();
                } catch (error) {
                    // It must not have been json
                }

                if (validationErrorResponse) {
                    for (let field in validationErrorResponse) {
                        errors.push(...validationErrorResponse[field]);
                    }
                } else {
                    errors.push(error.message || error.toString());
                }

                this.appInsights.trackEvent("AdminComponent.clearQueue.error", { message: errors.join(";") });
                this.clearQueueError = errors.join(";");
            });
    }

    public fetchStaleUploads(): void {
        this.staleUploadError = null;
        this.isStaleUploadsLoading = true;
        this.staleUploadIds = [];
        this.deletedStaleUploads = 0;
        this.totalStaleUploads = 0;

        this.authenticationService.getAuthHeaders()
            .then(headers => {
                let options = new RequestOptions({ headers });
                return this.http.get("/api/admin/staleuploads", options)
                    .toPromise();
            })
            .then(response => {
                this.isStaleUploadsLoading = false;
                this.staleUploadIds = response.json();
                this.totalStaleUploads = this.staleUploadIds.length;
            })
            .catch(error => {
                let errors: string[] = [];

                let validationErrorResponse: IValidationErrorResponse;
                try {
                    validationErrorResponse = error.json();
                } catch (error) {
                    // It must not have been json
                }

                if (validationErrorResponse) {
                    for (let field in validationErrorResponse) {
                        errors.push(...validationErrorResponse[field]);
                    }
                } else {
                    errors.push(error.message || error.toString());
                }

                this.appInsights.trackEvent("AdminComponent.fetchStaleUploads.error", { message: errors.join(";") });
                this.staleUploadError = errors.join(";");
            });
    }

    public deleteStaleUploads(): void {
        this.deletesInProgress = true;
        for (let i = 0; i < AdminComponent.numParallelDeletes; i++) {
            this.deleteNextUpload();
        }
    }

    public cancelDeleteStaleUploads(): void {
        this.deletesInProgress = false;
    }

    private refreshQueueData(): Promise<void> {
        this.isLoadingQueues = true;
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                let options = new RequestOptions({ headers });
                return this.http.get("/api/admin/queues", options)
                    .toPromise();
            })
            .then(response => {
                this.isLoadingQueues = false;
                this.queues = response.json();
                if (this.queues.length > 0) {
                    this.recomputePriority = this.queues[0].priority;
                    this.clearQueuePriority = this.queues[0].priority;
                }
            });
    }

    private deleteNextUpload(): void {
        // Cancellation
        if (!this.deletesInProgress) {
            return;
        }

        if (!this.staleUploadIds || this.staleUploadIds.length === 0) {
            this.deletesInProgress = false;
            return;
        }

        let uploadId = this.staleUploadIds.shift();
        this.uploadService.delete(uploadId)
            .catch(() => void 0) // Swallow errors
            .then(() => {
                this.deletedStaleUploads++;
                this.deleteNextUpload();
            });
    }
}
