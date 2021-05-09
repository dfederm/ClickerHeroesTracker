import { Component, OnInit } from "@angular/core";
import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { HttpErrorHandlerService } from "../../services/httpErrorHandlerService/httpErrorHandlerService";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { UploadService } from "../../services/uploadService/uploadService";
import { IBlockClanRequest } from "../../models";

export interface IPruneInvalidAuthTokensRequest {
    batchSize: number;
}

@Component({
    selector: "admin",
    templateUrl: "./admin.html",
})
export class AdminComponent implements OnInit {
    public static numParallelDeletes = 10;

    public static pruneInvalidAuthTokenBatchSize = 1000;

    public staleUploadError: string | null = null;

    public isStaleUploadsLoading = false;

    public staleUploadIds: number[] = [];

    public deletesInProgress = false;

    public deletedStaleUploads = 0;

    public totalStaleUploads = 0;

    public invalidAuthTokensError: string | null = null;

    public isInvalidAuthTokensLoading = false;

    public prunedInvalidAuthTokens = 0;

    public totalInvalidAuthTokens = 0;

    public pruningInProgress = false;

    public blockedClansError: string | null = null;

    public isLoadingBlockedClans = false;

    public blockedClans: string[] = [];

    public unblockClanName: string | undefined;

    public isUnblockClansLoading = false;

    constructor(
        private readonly authenticationService: AuthenticationService,
        private readonly http: HttpClient,
        private readonly httpErrorHandlerService: HttpErrorHandlerService,
        private readonly uploadService: UploadService,
    ) { }

    public ngOnInit(): void {
        this.refreshBlockedClans()
            .catch(() => {
                this.blockedClansError = "Could not fetch blocked clans";
            });
    }

    public unblockClan(): void {
        const unblockClanName = this.unblockClanName;
        if (!unblockClanName) {
            this.blockedClansError = "Invalid clan to unblock";
            return;
        }

        this.blockedClansError = null;
        this.isUnblockClansLoading = true;

        this.authenticationService.getAuthHeaders()
            .then(headers => {
                headers = headers.set("Content-Type", "application/json");
                const body: IBlockClanRequest = {
                    clanName: unblockClanName,
                    isBlocked: false,
                };
                return this.http
                    .post("/api/admin/blockclan", body, { headers })
                    .toPromise();
            })
            .then(() => {
                this.isUnblockClansLoading = false;
                this.refreshBlockedClans();
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("AdminComponent.unblockClan.error", err);
                const errors = this.httpErrorHandlerService.getValidationErrors(err);
                this.blockedClansError = errors.join(";");
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
                const errors = this.httpErrorHandlerService.getValidationErrors(err);
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

    public fetchInvalidAuthTokens(): void {
        this.invalidAuthTokensError = null;
        this.isInvalidAuthTokensLoading = true;
        this.prunedInvalidAuthTokens = 0;
        this.totalInvalidAuthTokens = 0;

        this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http.get<number>("/api/admin/countinvalidauthtokens", { headers })
                    .toPromise();
            })
            .then(response => {
                this.isInvalidAuthTokensLoading = false;
                this.totalInvalidAuthTokens = response;
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("AdminComponent.fetchInvalidAuthTokens.error", err);
                this.invalidAuthTokensError = "Something went wrong";
            });
    }

    public pruneInvalidAuthTokens(): void {
        this.invalidAuthTokensError = null;
        this.pruningInProgress = true;

        this.authenticationService.getAuthHeaders()
            .then(headers => {
                const body: IPruneInvalidAuthTokensRequest = { batchSize: AdminComponent.pruneInvalidAuthTokenBatchSize };
                return this.http.post("/api/admin/pruneinvalidauthtokens", body, { headers })
                    .toPromise();
            })
            .then(() => {
                this.prunedInvalidAuthTokens += AdminComponent.pruneInvalidAuthTokenBatchSize;
                if (this.prunedInvalidAuthTokens < this.totalInvalidAuthTokens) {
                    if (this.pruningInProgress) {
                        // Keep going
                        this.pruneInvalidAuthTokens();
                    }
                } else {
                    // Finished
                    this.prunedInvalidAuthTokens = this.totalInvalidAuthTokens;
                    this.pruningInProgress = false;
                }
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("AdminComponent.pruneInvalidAuthTokens.error", err);
                this.invalidAuthTokensError = "Something went wrong";
            });
    }

    public cancelPruneInvalidAuthTokens(): void {
        this.pruningInProgress = false;
    }

    private refreshBlockedClans(): Promise<void> {
        this.isLoadingBlockedClans = true;
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http.get<string[]>("/api/admin/blockedclans", { headers })
                    .toPromise();
            })
            .then(response => {
                this.isLoadingBlockedClans = false;
                this.blockedClans = response;
                if (this.blockedClans.length > 0) {
                    this.unblockClanName = this.blockedClans[0];
                }
            });
    }

    private deleteNextUpload(): void {
        // Cancellation
        if (!this.deletesInProgress) {
            return;
        }

        const uploadId = this.staleUploadIds.shift();
        if (!uploadId) {
            this.deletesInProgress = false;
            return;
        }

        this.uploadService.delete(uploadId)
            .catch(() => void 0) // Swallow errors
            .then(() => {
                this.deletedStaleUploads++;
                this.deleteNextUpload();
            });
    }
}
