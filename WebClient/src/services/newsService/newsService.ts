import { Injectable } from "@angular/core";
import { Http } from "@angular/http";

import "rxjs/add/operator/toPromise";

export interface ISiteNewsEntryListResponse
{
    entries: { [date: string]: string[] };
}

@Injectable()
export class NewsService
{
    constructor(private http: Http) { }

    public getNews(): Promise<ISiteNewsEntryListResponse>
    {
        return this.http
            .get("/api/news")
            .toPromise()
            .then(response => response.json() as ISiteNewsEntryListResponse)
            .catch(error =>
            {
                let errorMessage = error.message || error.toString();
                appInsights.trackEvent("NewsService.getNews.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }
}
