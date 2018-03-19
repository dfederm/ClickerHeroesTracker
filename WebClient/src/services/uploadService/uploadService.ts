import { Injectable } from "@angular/core";
import { HttpClient, HttpParams, HttpErrorResponse } from "@angular/common/http";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";
import "rxjs/add/operator/toPromise";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { IUser } from "../../models";
import * as Cache from "lru-cache";
import { AppInsightsService } from "@markpieszak/ng-application-insights";

export interface IUpload {
    id: number;

    timeSubmitted: string;

    playStyle: string;

    user: IUser;

    content: string;

    isScrubbed: boolean;
}

@Injectable()
export class UploadService {
    private readonly cache = new Cache<number, IUpload>(10);

    private userInfo: IUserInfo;

    constructor(
        private readonly authenticationService: AuthenticationService,
        private readonly http: HttpClient,
        private readonly httpErrorHandlerService: HttpErrorHandlerService,
        private readonly appInsights: AppInsightsService,
    ) {
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => {
                // Reset the cache on user change
                if (userInfo.username !== (this.userInfo && this.userInfo.username)) {
                    this.cache.reset();
                }

                this.userInfo = userInfo;
            });
    }

    public get(id: number): Promise<IUpload> {
        let cachedUpload = this.cache.get(id);
        if (cachedUpload) {
            this.appInsights.trackEvent("UploadService.get.cacheHit");
            return Promise.resolve(cachedUpload);
        }

        this.appInsights.trackEvent("UploadService.get.cacheMiss");
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http
                    .get<IUpload>(`/api/uploads/${id}`, { headers })
                    .toPromise();
            })
            .then(upload => {
                this.cache.set(id, upload);
                return upload;
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UploadService.get.error", err);
                return Promise.reject(err);
            });
    }

    public create(encodedSaveData: string, addToProgress: boolean, playStyle: string): Promise<number> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                headers = headers.set("Content-Type", "application/x-www-form-urlencoded");
                let params = new HttpParams()
                    .set("encodedSaveData", encodedSaveData)
                    .set("addToProgress", (addToProgress && this.userInfo.isLoggedIn).toString())
                    .set("playStyle", playStyle);

                // Angular doesn't encode '+' correctly. See: https://github.com/angular/angular/issues/11058
                let body = params.toString().replace(/\+/gi, "%2B");

                return this.http
                    .post<number>("/api/uploads", body, { headers })
                    .toPromise();
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UploadService.create.error", err);
                return Promise.reject(err);
            });
    }

    public delete(id: number): Promise<void> {
        this.cache.del(id);
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http
                    .delete(`/api/uploads/${id}`, { headers, responseType: "text" })
                    .toPromise();
            })
            .then(() => void 0)
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UploadService.delete.error", err);
                return Promise.reject(err);
            });
    }
}
