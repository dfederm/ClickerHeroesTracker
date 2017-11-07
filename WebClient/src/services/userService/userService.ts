import { Injectable } from "@angular/core";
import { Http, RequestOptions, Headers, URLSearchParams } from "@angular/http";
import { AppInsightsService } from "@markpieszak/ng-application-insights";

import "rxjs/add/operator/toPromise";

import { AuthenticationService } from "../../services/authenticationService/authenticationService";
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

// This actually pretty generic, so if it's used elsewhere consider moving it somewhere more generic
export interface IValidationErrorResponse {
    [field: string]: string[];
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
        private authenticationService: AuthenticationService,
        private http: Http,
        private appInsights: AppInsightsService,
    ) { }

    public create(userName: string, email: string, password: string): Promise<void> {
        let headers = new Headers();
        headers.append("Content-Type", "application/json");
        let options = new RequestOptions({ headers });

        let body: ICreateUserRequest = {
            userName,
            email,
            password,
        };

        return this.http
            .post("/api/users", body, options)
            .toPromise()
            .then(() => void 0)
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

                this.appInsights.trackEvent("UserService.create.error", { message: errors.join(";") });
                return Promise.reject(errors);
            });
    }

    public getUploads(userName: string, page: number, count: number): Promise<IUploadSummaryListResponse> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                let options = new RequestOptions({ headers });
                return this.http
                    .get(`/api/users/${userName}/uploads?page=${page}&count=${count}`, options)
                    .toPromise();
            })
            .then(response => response.json() as IUploadSummaryListResponse)
            .catch(error => {
                let errorMessage = error.message || error.toString();
                this.appInsights.trackEvent("UploadService.getUploads.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }

    public getProgress(userName: string, start: Date, end: Date): Promise<IProgressData> {
        let params = new URLSearchParams();
        params.append("start", start.toISOString());
        params.append("end", end.toISOString());

        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                let options = new RequestOptions({ headers });
                return this.http
                    .get(`/api/users/${userName}/progress?${params.toString()}`, options)
                    .toPromise();
            })
            .then(response => response.json() as IProgressData)
            .catch(error => {
                let errorMessage = error.message || error.toString();
                this.appInsights.trackEvent("UserService.getProgress.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }

    public getFollows(userName: string): Promise<IFollowsData> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                let options = new RequestOptions({ headers });
                return this.http
                    .get(`/api/users/${userName}/follows`, options)
                    .toPromise();
            })
            .then(response => response.json() as IFollowsData)
            .catch(error => {
                let errorMessage = error.message || error.toString();
                this.appInsights.trackEvent("UserService.getFollows.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }

    public getLogins(userName: string): Promise<IUserLogins> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                let options = new RequestOptions({ headers });
                return this.http
                    .get(`/api/users/${userName}/logins`, options)
                    .toPromise();
            })
            .then(response => response.json() as IUserLogins)
            .catch(error => {
                let errorMessage = error.message || error.toString();
                this.appInsights.trackEvent("UserService.getLogins.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }

    public removeLogin(userName: string, externalLogin: IExternalLogin): Promise<void> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                let options = new RequestOptions({ headers, body: externalLogin });
                return this.http
                    .delete(`/api/users/${userName}/logins`, options)
                    .toPromise();
            })
            .then(() => void 0)
            .catch(error => {
                let errorMessage = error.message || error.toString();
                this.appInsights.trackEvent("UserService.removeLogin.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }

    public setPassword(userName: string, newPassword: string): Promise<void> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                headers.append("Content-Type", "application/json");
                let options = new RequestOptions({ headers });
                let body: ISetPasswordRequest = { newPassword };
                return this.http
                    .post(`/api/users/${userName}/setpassword`, body, options)
                    .toPromise();
            })
            .then(() => void 0)
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

                this.appInsights.trackEvent("UserService.changePassword.error", { message: errors.join(";") });
                return Promise.reject(errors);
            });
    }

    public changePassword(userName: string, currentPassword: string, newPassword: string): Promise<void> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                headers.append("Content-Type", "application/json");
                let options = new RequestOptions({ headers });
                let body: IChangePasswordRequest = { currentPassword, newPassword };
                return this.http
                    .post(`/api/users/${userName}/changepassword`, body, options)
                    .toPromise();
            })
            .then(() => void 0)
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

                this.appInsights.trackEvent("UserService.changePassword.error", { message: errors.join(";") });
                return Promise.reject(errors);
            });
    }

    public resetPassword(email: string): Promise<void> {
        let headers = new Headers();
        headers.append("Content-Type", "application/json");
        let options = new RequestOptions({ headers });
        let body: IResetPasswordRequest = { email };
        return this.http
            .post("/api/users/resetpassword", body, options)
            .toPromise()
            .then(() => void 0)
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

                this.appInsights.trackEvent("UserService.resetPassword.error", { message: errors.join(";") });
                return Promise.reject(errors);
            });
    }

    public resetPasswordConfirmation(email: string, password: string, code: string): Promise<void> {
        let headers = new Headers();
        headers.append("Content-Type", "application/json");
        let options = new RequestOptions({ headers });
        let body: IResetPasswordConfirmationRequest = {
            email,
            password,
            code,
        };
        return this.http
            .post("/api/users/resetpasswordconfirmation", body, options)
            .toPromise()
            .then(() => void 0)
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

                this.appInsights.trackEvent("UserService.resetPasswordConfirmation.error", { message: errors.join(";") });
                return Promise.reject(errors);
            });
    }
}
