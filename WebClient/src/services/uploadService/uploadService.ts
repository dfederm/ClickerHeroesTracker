import { Injectable } from "@angular/core";
import { Http, RequestOptions, URLSearchParams } from "@angular/http";
import { AppInsightsService } from "@markpieszak/ng-application-insights";

import "rxjs/add/operator/toPromise";

import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { IUpload } from "../../models";

@Injectable()
export class UploadService {
    private userInfo: IUserInfo;

    constructor(
        private authenticationService: AuthenticationService,
        private http: Http,
        private appInsights: AppInsightsService,
    ) {
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => this.userInfo = userInfo);
    }

    public get(id: number): Promise<IUpload> {
        let headers = this.authenticationService.getAuthHeaders();
        let options = new RequestOptions({ headers });
        return this.http
            .get(`/api/uploads/${id}`, options)
            .toPromise()
            .then(response => response.json() as IUpload)
            .catch(error => {
                let errorMessage = error.message || error.toString();
                this.appInsights.trackEvent("UploadService.get.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }

    public create(encodedSaveData: string, addToProgress: boolean, playStyle: string): Promise<number> {
        let headers = this.authenticationService.getAuthHeaders();
        headers.append("Content-Type", "application/x-www-form-urlencoded");
        let options = new RequestOptions({ headers });
        let params = new URLSearchParams();
        params.append("encodedSaveData", encodedSaveData);
        params.append("addToProgress", (addToProgress && this.userInfo.isLoggedIn).toString());
        params.append("playStyle", playStyle);
        return this.http
            .post("/api/uploads", params.toString(), options)
            .toPromise()
            .then(result => parseInt(result.text()))
            .catch(error => {
                let errorMessage = error.message || error.toString();
                this.appInsights.trackEvent("UploadService.create.error", { message: errorMessage });
                return Promise.reject(error);
            });
    }

    public delete(id: number): Promise<void> {
        let headers = this.authenticationService.getAuthHeaders();
        let options = new RequestOptions({ headers });
        return this.http
            .delete(`/api/uploads/${id}`, options)
            .toPromise()
            .then(() => void 0)
            .catch(error => {
                let errorMessage = error.message || error.toString();
                this.appInsights.trackEvent("UploadService.delete.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }
}
