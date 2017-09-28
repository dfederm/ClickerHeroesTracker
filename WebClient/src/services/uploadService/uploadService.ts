import { Injectable } from "@angular/core";
import { Http, RequestOptions } from "@angular/http";

import "rxjs/add/operator/toPromise";

import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { IPaginationMetadata } from "../pagination";

export interface IUpload {
    id: number;

    timeSubmitted: string;

    playStyle: string;

    user?: IUser;

    uploadContent?: string;

    stats?: { [key: string]: string };
}

export interface IUploadSummaryListResponse {
    pagination: IPaginationMetadata;

    uploads: IUpload[];
}

export interface IUser {
    id: string;

    name: string;
}

@Injectable()
export class UploadService {
    private userInfo: IUserInfo;

    constructor(
        private authenticationService: AuthenticationService,
        private http: Http,
    ) {
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => this.userInfo = userInfo);
    }

    public getUploads(page: number, count: number): Promise<IUploadSummaryListResponse> {
        let headers = this.authenticationService.getAuthHeaders();
        let options = new RequestOptions({ headers });
        return this.http
            .get("/api/uploads?page=" + page + "&count=" + count, options)
            .toPromise()
            .then(response => response.json() as IUploadSummaryListResponse)
            .catch(error => {
                let errorMessage = error.message || error.toString();
                appInsights.trackEvent("UploadService.getUploads.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
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
                appInsights.trackEvent("UploadService.get.error", { message: errorMessage });
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
                appInsights.trackEvent("UploadService.create.error", { message: errorMessage });
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
                appInsights.trackEvent("UploadService.delete.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }
}
