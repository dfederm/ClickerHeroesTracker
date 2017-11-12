import { Injectable } from "@angular/core";
import { HttpClient, HttpParams, HttpErrorResponse } from "@angular/common/http";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";
import { AuthenticationService } from "../authenticationService/authenticationService";

import "rxjs/add/operator/toPromise";

@Injectable()
export class FeedbackService {
    constructor(
        private authenticationService: AuthenticationService,
        private http: HttpClient,
        private httpErrorHandlerService: HttpErrorHandlerService,
    ) { }

    public send(comments: string, email: string): Promise<void> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                headers = headers.set("Content-Type", "application/x-www-form-urlencoded");
                let params = new HttpParams()
                    .set("comments", comments)
                    .set("email", email);
                return this.http
                    .post("/api/feedback", params.toString(), { headers, responseType: "text" })
                    .toPromise();
            })
            .then(() => void 0)
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("FeedbackService.send.error", err);
                return Promise.reject(err);
            });
    }
}
