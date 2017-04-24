import { Component, OnInit, Input } from "@angular/core";
import { Http, Response } from "@angular/http";

interface IChangelogSectionViewModel
{
    title?: string;
    entries: string[];
}

@Component({
    selector: "changelog",
    templateUrl: "./js/components/changelog/changelog.html",
})
export class ChangelogComponent implements OnInit
{
    public sections: IChangelogSectionViewModel[];
    public errorMessage: string;

    @Input()
    public isFull: boolean;

    constructor(private http: Http) {}

    public ngOnInit(): void
    {
        this.http
            .get("/api/news")
            .subscribe(
                response => this.handleData(response.json() as ISiteNewsEntryListResponse),
                error => this.handleError(error));
    }

    private handleData(response: ISiteNewsEntryListResponse): void
    {
        if (!response || !response.entries)
        {
            this.errorMessage = "There was a problem getting the site news";
            return;
        }

        let entries = response.entries;

        const maxEntries = 3;
        let numEntries = 0;

        // Put the dates in an array so we can enumerate backwards
        const dates: string[] = [];
        for (let dateStr in entries)
        {
            dates.push(dateStr);
        }

        this.sections = [];
        let currentSection: IChangelogSectionViewModel = null;
        for (let i = dates.length - 1; i >= 0; i--)
        {
            let dateStr = dates[i];

            // The date comes back as a UTC time at midnight of the date. We need to adjust for the user's local timezone offset or the date may move back by a day.
            let dateUtc = new Date(dateStr);
            let date = new Date(dateUtc.getUTCFullYear(), dateUtc.getUTCMonth(), dateUtc.getUTCDate()).toLocaleDateString();

            if (this.isFull || !currentSection)
            {
                if (currentSection)
                {
                    this.sections.push(currentSection);
                }

                currentSection =
                {
                    title: this.isFull ? date : null,
                    entries: [],
                };
            }

            let messages = entries[dateStr];
            for (let j = 0; j < messages.length; j++)
            {
                currentSection.entries.push(messages[j]);
                numEntries++;
                if (!this.isFull && numEntries === maxEntries)
                {
                    break;
                }
            }

            if (!this.isFull && numEntries === maxEntries)
            {
                break;
            }
        }

        if (currentSection)
        {
            this.sections.push(currentSection);
        }
    }

    private handleError(error: Response | { message?: string }): void
    {
        if (error instanceof Response)
        {
            let body = error.json() || "";
            let err = body.error || JSON.stringify(body);
            this.errorMessage = `${error.status} - ${error.statusText || ""} ${err}`;
        }
        else
        {
            this.errorMessage = error.message ? error.message : error.toString();
        }
    }
}
