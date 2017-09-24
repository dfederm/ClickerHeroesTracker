import { Injectable } from "@angular/core";
import { Http, RequestOptions, Headers } from "@angular/http";

import "rxjs/add/operator/toPromise";

import { AuthenticationService } from "../../services/authenticationService/authenticationService";

export interface ICreateUserRequest {
    userName: string;

    email: string;

    password: string;
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

@Injectable()
export class UserService {
    private isLoggedIn: boolean;

    constructor(
        private authenticationService: AuthenticationService,
        private http: Http,
    ) {
        this.authenticationService
            .isLoggedIn()
            .subscribe(isLoggedIn => this.isLoggedIn = isLoggedIn);
    }

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

                appInsights.trackEvent("UserService.create.error", { message: errors.join(";") });
                return Promise.reject(errors);
            });
    }

    public getProgress(userName: string, start: Date, end: Date): Promise<IProgressData> {
        let headersPromise = this.isLoggedIn
            ? this.authenticationService.getAuthHeaders()
            : Promise.resolve(new Headers());
        return headersPromise
            .then(headers => {
                let params = new URLSearchParams();
                params.append("start", start.toISOString());
                params.append("end", end.toISOString());

                let options = new RequestOptions({ headers });
                return this.http
                    .get(`/api/users/${userName}/progress?${params.toString()}`, options)
                    .toPromise();
            })
            .then(response => response.json() as IProgressData)
            .catch(error => {
                let errorMessage = error.message || error.toString();
                appInsights.trackEvent("UserService.getProgress.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }

    public getFollows(userName: string): Promise<IFollowsData> {
        let headersPromise = this.isLoggedIn
            ? this.authenticationService.getAuthHeaders()
            : Promise.resolve(new Headers());
        return headersPromise
            .then(headers => {
                let options = new RequestOptions({ headers });
                return this.http
                    .get(`/api/users/${userName}/follows`, options)
                    .toPromise();
            })
            .then(response => response.json() as IFollowsData)
            .catch(error => {
                let errorMessage = error.message || error.toString();
                appInsights.trackEvent("UserService.getFollows.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }
}
