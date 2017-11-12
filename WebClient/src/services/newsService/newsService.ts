import { Injectable } from "@angular/core";
import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";

import "rxjs/add/operator/toPromise";

export interface ISiteNewsEntryListResponse {
    entries: { [date: string]: string[] };
}

@Injectable()
export class NewsService {
    constructor(
        private http: HttpClient,
        private httpErrorHandlerService: HttpErrorHandlerService,
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
}
