import { Component, OnInit } from "@angular/core";
import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { HttpErrorHandlerService } from "../../services/httpErrorHandlerService/httpErrorHandlerService";
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
        private http: HttpClient,
        private httpErrorHandlerService: HttpErrorHandlerService,
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
                headers = headers.set("Content-Type", "application/json");
                let body: IRecomputeRequest = {
                    uploadIds,
                    priority: this.recomputePriority,
                };
                return this.http
                    .post("/api/admin/recompute", body, { headers })
                    .toPromise();
            })
            .then(() => {
                this.isRecomputeLoading = false;
                this.refreshQueueData();
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("AdminComponent.recompute.error", err);
                let errors = this.httpErrorHandlerService.getValidationErrors(err);
                this.recomputeError = errors.join(";");
            });
    }

    public clearQueue(): void {
        this.clearQueueError = null;
        this.isClearQueueLoading = true;

        this.authenticationService.getAuthHeaders()
            .then(headers => {
                headers = headers.set("Content-Type", "application/json");
                let body: IClearQueueRequest = {
                    priority: this.clearQueuePriority,
                };
                return this.http
                    .post("/api/admin/clearqueue", body, { headers })
                    .toPromise();
            })
            .then(() => {
                this.isClearQueueLoading = false;
                this.refreshQueueData();
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("AdminComponent.clearQueue.error", err);
                let errors = this.httpErrorHandlerService.getValidationErrors(err);
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
                return this.http.get<number[]>("/api/admin/staleuploads", { headers })
                    .toPromise();
            })
            .then(response => {
                this.isStaleUploadsLoading = false;
                this.staleUploadIds = response;
                this.totalStaleUploads = this.staleUploadIds.length;
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("AdminComponent.fetchStaleUploads.error", err);
                let errors = this.httpErrorHandlerService.getValidationErrors(err);
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
                return this.http.get<IUploadQueueStats[]>("/api/admin/queues", { headers })
                    .toPromise();
            })
            .then(response => {
                this.isLoadingQueues = false;
                this.queues = response;
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
