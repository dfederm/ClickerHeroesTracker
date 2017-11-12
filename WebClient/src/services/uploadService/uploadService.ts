import { Injectable } from "@angular/core";
import { HttpClient, HttpParams, HttpErrorResponse } from "@angular/common/http";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";

import "rxjs/add/operator/toPromise";

import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { IUpload } from "../../models";

@Injectable()
export class UploadService {
    private userInfo: IUserInfo;

    constructor(
        private authenticationService: AuthenticationService,
        private http: HttpClient,
        private httpErrorHandlerService: HttpErrorHandlerService,
    ) {
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => this.userInfo = userInfo);
    }

    public get(id: number): Promise<IUpload> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http
                    .get<IUpload>(`/api/uploads/${id}`, { headers })
                    .toPromise();
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
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http
                    .delete(`/api/uploads/${id}`, { headers })
                    .toPromise();
            })
            .then(() => void 0)
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UploadService.delete.error", err);
                return Promise.reject(err);
            });
    }
}
