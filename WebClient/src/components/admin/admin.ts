import { Component, OnInit } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { NgxSpinnerService } from "ngx-spinner";
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

    public staleUploadError: string;

    public staleUploadIds: number[];

    public deletesInProgress: boolean;

    public deletedStaleUploads: number;

    public totalStaleUploads: number;

    public invalidAuthTokensError: string;

    public prunedInvalidAuthTokens: number;

    public totalInvalidAuthTokens: number;

    public pruningInProgress: boolean;

    public blockedClansError: string;

    public blockedClans: string[];

    public unblockClanName: string;

    constructor(
        private readonly authenticationService: AuthenticationService,
        private readonly http: HttpClient,
        private readonly httpErrorHandlerService: HttpErrorHandlerService,
        private readonly spinnerService: NgxSpinnerService,
        private readonly uploadService: UploadService,
    ) { }

    public async ngOnInit(): Promise<void> {
        try {
            await this.refreshBlockedClans()
        } catch (error) {
            this.blockedClansError = "Could not fetch blocked clans";            
        }
    }

    public async unblockClan(): Promise<void> {
        this.blockedClansError = null;

        this.spinnerService.show("blockedClans");
        try {
            let headers = await this.authenticationService.getAuthHeaders();
            headers = headers.set("Content-Type", "application/json");
    
            let body: IBlockClanRequest = {
                clanName: this.unblockClanName,
                isBlocked: false,
            };
    
            await this.http
                .post("/api/admin/blockclan", body, { headers })
                .toPromise();
     
            await this.refreshBlockedClans();
        } catch (err) {
            this.httpErrorHandlerService.logError("AdminComponent.unblockClan.error", err);
            let errors = this.httpErrorHandlerService.getValidationErrors(err);
            this.blockedClansError = errors.join(";");        
        } finally {
            this.spinnerService.hide("blockedClans");
        }
    }

    public async fetchStaleUploads(): Promise<void> {
        this.staleUploadError = null;
        this.staleUploadIds = [];
        this.deletedStaleUploads = 0;
        this.totalStaleUploads = 0;

        this.spinnerService.show("staleUploads");
        try {
            let headers = await this.authenticationService.getAuthHeaders();
            let response = await this.http.get<number[]>("/api/admin/staleuploads", { headers }).toPromise();
    
            this.staleUploadIds = response;
            this.totalStaleUploads = this.staleUploadIds.length;
        } catch (err) {
            this.httpErrorHandlerService.logError("AdminComponent.fetchStaleUploads.error", err);
            let errors = this.httpErrorHandlerService.getValidationErrors(err);
            this.staleUploadError = errors.join(";");
        } finally {
            this.spinnerService.hide("staleUploads");
        }

    }

    public async deleteStaleUploads(): Promise<void> {
        this.deletesInProgress = true;
        for (let i = 0; i < AdminComponent.numParallelDeletes; i++) {
            await this.deleteNextUpload();
        }
    }

    public cancelDeleteStaleUploads(): void {
        this.deletesInProgress = false;
    }

    public async fetchInvalidAuthTokens(): Promise<void> {
        this.invalidAuthTokensError = null;
        this.prunedInvalidAuthTokens = 0;
        this.totalInvalidAuthTokens = 0;

        this.spinnerService.show("invalidAuthTokens");
        try {
            const headers = await this.authenticationService.getAuthHeaders();
            const response = await this.http.get<number>("/api/admin/countinvalidauthtokens", { headers }).toPromise();
            this.totalInvalidAuthTokens = response;
        } catch (err) {
            this.httpErrorHandlerService.logError("AdminComponent.fetchInvalidAuthTokens.error", err);
            this.invalidAuthTokensError = "Something went wrong";        
        } finally {
            this.spinnerService.hide("invalidAuthTokens");
        }
    }

    public async pruneInvalidAuthTokens(): Promise<void> {
        this.invalidAuthTokensError = null;
        this.pruningInProgress = true;

        try {
            let headers = await this.authenticationService.getAuthHeaders();
            let body: IPruneInvalidAuthTokensRequest = { batchSize: AdminComponent.pruneInvalidAuthTokenBatchSize };
            await this.http.post("/api/admin/pruneinvalidauthtokens", body, { headers }).toPromise();

            this.prunedInvalidAuthTokens += AdminComponent.pruneInvalidAuthTokenBatchSize;
            if (this.prunedInvalidAuthTokens < this.totalInvalidAuthTokens) {
                if (this.pruningInProgress) {
                    // Keep going
                    await this.pruneInvalidAuthTokens();
                }
            } else {
                // Finished
                this.prunedInvalidAuthTokens = this.totalInvalidAuthTokens;
                this.pruningInProgress = false;
            }
        } catch (err) {
            this.httpErrorHandlerService.logError("AdminComponent.pruneInvalidAuthTokens.error", err);
            this.invalidAuthTokensError = "Something went wrong";
        }
    }

    public cancelPruneInvalidAuthTokens(): void {
        this.pruningInProgress = false;
    }

    private async refreshBlockedClans(): Promise<void> {
        this.spinnerService.show("blockedClans");
        try {
            let headers = await this.authenticationService.getAuthHeaders();
            let response = await this.http.get<string[]>("/api/admin/blockedclans", { headers }).toPromise();
    
            this.blockedClans = response;
            if (this.blockedClans.length > 0) {
                this.unblockClanName = this.blockedClans[0];
            }    
        } finally {
            this.spinnerService.hide("blockedClans");
        }
    }

    private async deleteNextUpload(): Promise<void> {
        // Cancellation
        if (!this.deletesInProgress) {
            return;
        }

        if (!this.staleUploadIds || this.staleUploadIds.length === 0) {
            this.deletesInProgress = false;
            return;
        }

        let uploadId = this.staleUploadIds.shift();

        try {
            await this.uploadService.delete(uploadId);            
        } catch {
            // Swallow errors
        }

        this.deletedStaleUploads++;
        await this.deleteNextUpload();
    }
}
