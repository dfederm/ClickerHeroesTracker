import { Injectable } from "@angular/core";
import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";
import { AuthenticationService } from "../authenticationService/authenticationService";

export interface ISiteNewsEntryListResponse {
    entries: { [date: string]: string[] };
}

export interface ISiteNewsEntry {
    date: string;
    messages: string[];
}

@Injectable({
    providedIn: "root",
})
export class NewsService {
    constructor(
        private readonly authenticationService: AuthenticationService,
        private readonly http: HttpClient,
        private readonly httpErrorHandlerService: HttpErrorHandlerService,
    ) { }

    public getNews(): Promise<ISiteNewsEntryListResponse> {
        return this.http
            .get<ISiteNewsEntryListResponse>("/api/news")
            .toPromise()
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("NewsService.getNews.error", err);
                return Promise.reject(err);
            });
    }

    public addNews(newsEntry: ISiteNewsEntry): Promise<void> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http
                    .post("/api/news", newsEntry, { headers, responseType: "text" })
                    .toPromise();
            })
            .then(() => void 0)
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("NewsService.addNews.error", err);
                return Promise.reject(err);
            });
    }

    public deleteNews(date: string): Promise<void> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http
                    .delete(`/api/news/${date}`, { headers, responseType: "text" })
                    .toPromise();
            })
            .then(() => void 0)
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("NewsService.deleteNews.error", err);
                return Promise.reject(err);
            });
    }
}
