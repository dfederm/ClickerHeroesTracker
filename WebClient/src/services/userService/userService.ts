import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders, HttpParams, HttpErrorResponse } from "@angular/common/http";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";

import "rxjs/add/operator/toPromise";

import { AuthenticationService } from "../authenticationService/authenticationService";
import { IPaginationMetadata, IUpload } from "../../models";

export interface ICreateUserRequest {
    userName: string;

    email: string;

    password: string;
}

export interface IUploadSummaryListResponse {
    pagination: IPaginationMetadata;

    uploads: IUpload[];
}

export interface ISetPasswordRequest {
    newPassword: string;
}

export interface IChangePasswordRequest {
    currentPassword: string;

    newPassword: string;
}

export interface IResetPasswordRequest {
    email: string;
}

export interface IResetPasswordConfirmationRequest {
    email: string;
    password: string;
    code: string;
}

export interface IProgressData {
    titanDamageData: { [date: string]: string };

    soulsSpentData: { [date: string]: string };

    heroSoulsSacrificedData: { [date: string]: string };

    totalAncientSoulsData: { [date: string]: string };

    transcendentPowerData: { [date: string]: string };

    rubiesData: { [date: string]: string };

    highestZoneThisTranscensionData: { [date: string]: string };

    highestZoneLifetimeData: { [date: string]: string };

    ascensionsThisTranscensionData: { [date: string]: string };

    ascensionsLifetimeData: { [date: string]: string };

    ancientLevelData: { [ancient: string]: { [date: string]: string } };

    outsiderLevelData: { [outsider: string]: { [date: string]: string } };
}

export interface IFollowsData {
    follows: string[];
}

export interface IAddFollowRequest {
    followUserName: string;
}

export interface IUserLogins {
    hasPassword: boolean;
    externalLogins: IExternalLogin[];
}

export interface IExternalLogin {
    providerName: string;
    externalUserId: string;
}

@Injectable()
export class UserService {
    constructor(
        private readonly authenticationService: AuthenticationService,
        private readonly http: HttpClient,
        private readonly httpErrorHandlerService: HttpErrorHandlerService,
    ) { }

    public create(userName: string, email: string, password: string): Promise<void> {
        let headers = new HttpHeaders();
        headers = headers.set("Content-Type", "application/json");

        let body: ICreateUserRequest = {
            userName,
            email,
            password,
        };

        return this.http
            .post("/api/users", body, { headers, responseType: "text" })
            .toPromise()
            .then(() => void 0)
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UserService.create.error", err);
                let errors = this.httpErrorHandlerService.getValidationErrors(err);
                return Promise.reject(errors);
            });
    }

    public getUploads(userName: string, page: number, count: number): Promise<IUploadSummaryListResponse> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http
                    .get<IUploadSummaryListResponse>(`/api/users/${userName}/uploads?page=${page}&count=${count}`, { headers })
                    .toPromise();
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UserService.getUploads.error", err);
                return Promise.reject(err);
            });
    }

    public getProgress(userName: string, start: Date, end: Date): Promise<IProgressData> {
        let params = new HttpParams()
            .set("start", start.toISOString())
            .set("end", end.toISOString());

        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http
                    .get<IProgressData>(`/api/users/${userName}/progress?${params.toString()}`, { headers })
                    .toPromise();
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UserService.getProgress.error", err);
                return Promise.reject(err);
            });
    }

    public getFollows(userName: string): Promise<IFollowsData> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http
                    .get<IFollowsData>(`/api/users/${userName}/follows`, { headers })
                    .toPromise();
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UserService.getFollows.error", err);
                return Promise.reject(err);
            });
    }

    public addFollow(userName: string, followUserName: string): Promise<void> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                let body: IAddFollowRequest = { followUserName };
                return this.http
                    .post(`/api/users/${userName}/follows`, body, { headers, responseType: "text" })
                    .toPromise();
            })
            .then(() => void 0)
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UserService.addFollow.error", err);
                return Promise.reject(err);
            });
    }

    public removeFollow(userName: string, followUserName: string): Promise<void> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http
                    .delete(`/api/users/${userName}/follows/${followUserName}`, { headers, responseType: "text" })
                    .toPromise();
            })
            .then(() => void 0)
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UserService.removeFollow.error", err);
                return Promise.reject(err);
            });
    }

    public getLogins(userName: string): Promise<IUserLogins> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http
                    .get<IUserLogins>(`/api/users/${userName}/logins`, { headers })
                    .toPromise();
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UserService.getLogins.error", err);
                return Promise.reject(err);
            });
    }

    public removeLogin(userName: string, externalLogin: IExternalLogin): Promise<void> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http
                    .request("delete", `/api/users/${userName}/logins`, { headers, body: externalLogin, responseType: "text" })
                    .toPromise();
            })
            .then(() => void 0)
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UserService.removeLogin.error", err);
                return Promise.reject(err);
            });
    }

    public setPassword(userName: string, newPassword: string): Promise<void> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                headers = headers.set("Content-Type", "application/json");
                let body: ISetPasswordRequest = { newPassword };
                return this.http
                    .post(`/api/users/${userName}/setpassword`, body, { headers, responseType: "text" })
                    .toPromise();
            })
            .then(() => void 0)
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UserService.setPassword.error", err);
                let errors = this.httpErrorHandlerService.getValidationErrors(err);
                return Promise.reject(errors);
            });
    }

    public changePassword(userName: string, currentPassword: string, newPassword: string): Promise<void> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                headers = headers.set("Content-Type", "application/json");
                let body: IChangePasswordRequest = { currentPassword, newPassword };
                return this.http
                    .post(`/api/users/${userName}/changepassword`, body, { headers, responseType: "text" })
                    .toPromise();
            })
            .then(() => void 0)
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UserService.changePassword.error", err);
                let errors = this.httpErrorHandlerService.getValidationErrors(err);
                return Promise.reject(errors);
            });
    }

    public resetPassword(email: string): Promise<void> {
        let headers = new HttpHeaders();
        headers = headers.set("Content-Type", "application/json");
        let body: IResetPasswordRequest = { email };
        return this.http
            .post("/api/users/resetpassword", body, { headers, responseType: "text" })
            .toPromise()
            .then(() => void 0)
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UserService.resetPassword.error", err);
                let errors = this.httpErrorHandlerService.getValidationErrors(err);
                return Promise.reject(errors);
            });
    }

    public resetPasswordConfirmation(email: string, password: string, code: string): Promise<void> {
        let headers = new HttpHeaders();
        headers = headers.set("Content-Type", "application/json");
        let body: IResetPasswordConfirmationRequest = {
            email,
            password,
            code,
        };
        return this.http
            .post("/api/users/resetpasswordconfirmation", body, { headers, responseType: "text" })
            .toPromise()
            .then(() => void 0)
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UserService.resetPasswordConfirmation.error", err);
                let errors = this.httpErrorHandlerService.getValidationErrors(err);
                return Promise.reject(errors);
            });
    }
}
