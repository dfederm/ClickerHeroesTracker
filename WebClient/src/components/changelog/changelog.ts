import { Component, OnInit, Input } from "@angular/core";

import { NewsService, ISiteNewsEntryListResponse } from "../../services/newsService/newsService";

interface IChangelogSectionViewModel {
    date?: Date;
    entries: string[];
}

@Component({
    selector: "changelog",
    templateUrl: "./changelog.html",
})
export class ChangelogComponent implements OnInit {
    public sections: IChangelogSectionViewModel[];
    public isError: boolean;

    @Input()
    public isFull: boolean;

    constructor(private newsService: NewsService) { }

    public ngOnInit(): void {
        this.newsService
            .getNews()
            .then(response => this.handleData(response))
            .catch(() => this.isError = true);
    }

    private handleData(response: ISiteNewsEntryListResponse): void {
        if (!response || !response.entries) {
            this.isError = true;
            return;
        }

        let entries = response.entries;

        const maxEntries = 3;
        let numEntries = 0;

        // Put the dates in an array so we can enumerate backwards
        const dates: string[] = [];
        for (let dateStr in entries) {
            dates.push(dateStr);
        }

        this.sections = [];
        let currentSection: IChangelogSectionViewModel = null;
        for (let i = dates.length - 1; i >= 0; i--) {
            let dateStr = dates[i];

            // The date comes back as a UTC time at midnight of the date. We need to adjust for the user's local timezone offset or the date may move back by a day.
            let dateUtc = new Date(dateStr);
            let date = new Date(dateUtc.getUTCFullYear(), dateUtc.getUTCMonth(), dateUtc.getUTCDate());

            if (this.isFull || !currentSection) {
                if (currentSection) {
                    this.sections.push(currentSection);
                }

                currentSection = {
                    date: this.isFull ? date : null,
                    entries: [],
                };
            }

            let messages = entries[dateStr];
            for (let j = 0; j < messages.length; j++) {
                currentSection.entries.push(messages[j]);
                numEntries++;
                if (!this.isFull && numEntries === maxEntries) {
                    break;
                }
            }

            if (!this.isFull && numEntries === maxEntries) {
                break;
            }
        }

        if (currentSection) {
            this.sections.push(currentSection);
        }
    }
}
