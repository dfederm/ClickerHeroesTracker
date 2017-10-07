import { Injectable } from "@angular/core";
import { Http, RequestOptions } from "@angular/http";

import "rxjs/add/operator/toPromise";

import { AuthenticationService } from "../../services/authenticationService/authenticationService";

@Injectable()
export class FeedbackService {
    constructor(
        private authenticationService: AuthenticationService,
        private http: Http,
    ) { }

    public send(comments: string, email: string): Promise<void> {
        let headers = this.authenticationService.getAuthHeaders();
        headers.append("Content-Type", "application/x-www-form-urlencoded");
        let options = new RequestOptions({ headers });
        let params = new URLSearchParams();
        params.append("comments", comments);
        params.append("email", email);
        return this.http
            .post("/api/feedback", params.toString(), options)
            .toPromise()
            .then(() => void 0)
            .catch(error => {
                let errorMessage = error.message || error.toString();
                appInsights.trackEvent("FeedbackService.send.error", { message: errorMessage });
                return Promise.reject(error);
            });
    }
}
