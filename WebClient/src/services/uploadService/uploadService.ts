import { Injectable } from "@angular/core";
import { HttpClient, HttpParams, HttpErrorResponse } from "@angular/common/http";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { IUser } from "../../models";
import { LRUCache } from "lru-cache";
import { LoggingService } from "../../services/loggingService/loggingService";
import { UserService } from "../userService/userService";

export interface IUpload {
    id: number;

    timeSubmitted: string;

    playStyle: string;

    user: IUser;

    content: string;

    isScrubbed: boolean;
}

@Injectable({
    providedIn: "root",
})
export class UploadService {
    private readonly cache = new LRUCache<number, IUpload>({ max: 10 });

    private user: IUser;

    private cacheOnCreate = true;

    constructor(
        private readonly authenticationService: AuthenticationService,
        private readonly http: HttpClient,
        private readonly httpErrorHandlerService: HttpErrorHandlerService,
        private readonly loggingService: LoggingService,
        private readonly userService: UserService,
    ) {
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => {
                if (userInfo.username !== (this.user && this.user.name)) {
                    this.cache.clear();

                    this.user = null;
                    if (userInfo.isLoggedIn) {
                        // Make sure we don't cache anything on creation until we have the data we need
                        this.cacheOnCreate = false;
                        this.userService.getUser(userInfo.username)
                            .then(user => {
                                this.user = user;
                                this.cacheOnCreate = true;
                            })
                            .catch(() => {
                                // Swallow. We just won't be able to cache created uploads
                            });
                    } else {
                        this.cacheOnCreate = true;
                    }
                }
            });
    }

    public get(id: number): Promise<IUpload> {
        let cachedUpload = this.cache.get(id);
        if (cachedUpload) {
            this.loggingService.logEvent("UploadService.get.cacheHit");
            return Promise.resolve(cachedUpload);
        }

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
                    .set("addToProgress", addToProgress.toString())
                    .set("playStyle", playStyle);

                // Angular doesn't encode '+' correctly. See: https://github.com/angular/angular/issues/11058
                let body = params.toString().replace(/\+/gi, "%2B");

                return this.http
                    .post<number>("/api/uploads", body, { headers })
                    .toPromise();
            })
            .then(uploadId => {
                if (this.cacheOnCreate) {
                    let upload: IUpload = {
                        id: uploadId,
                        isScrubbed: false, // Provided by the user, so definitely not scrubbed, even if the user isn't logged in
                        content: encodedSaveData,
                        playStyle,
                        timeSubmitted: (new Date()).toISOString(), // Won't be 100% correct, but pretty close
                        user: addToProgress ? this.user : null,
                    };
                    this.cache.set(uploadId, upload);
                }

                return uploadId;
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("UploadService.create.error", err);
                return Promise.reject(err);
            });
    }

    public delete(id: number): Promise<void> {
        this.cache.delete(id);
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
