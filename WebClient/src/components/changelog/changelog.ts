import { Component, OnInit, Input } from "@angular/core";

import { NewsService, ISiteNewsEntryListResponse } from "../../services/newsService/newsService";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";

interface IChangelogSectionViewModel {
    date?: Date;
    // Using ngFor and ngModel together requires a sub-object.
    entries: { message: string }[];
    editable: boolean;
    isNew?: boolean;
}

@Component({
    selector: "changelog",
    templateUrl: "./changelog.html",
})
export class ChangelogComponent implements OnInit {
    public sections: IChangelogSectionViewModel[];
    public isError: boolean;
    public isLoading: boolean;

    public canEdit: boolean;

    @Input()
    public isFull: boolean;

    constructor(
        private newsService: NewsService,
        private authenticationService: AuthenticationService,
    ) { }

    public ngOnInit(): void {
        this.refreshNews();

        if (this.isFull) {
            this.authenticationService
                .userInfo()
                .subscribe(userInfo => this.canEdit = userInfo.isAdmin);
        }
    }

    public addSection(): void {
        this.sections.unshift({
            date: new Date(),
            entries: [{ message: "" }],
            editable: true,
            isNew: true,
        });
    }

    public addMessage(viewModel: IChangelogSectionViewModel): void {
        viewModel.entries.push({ message: "" });
    }

    public save(viewModel: IChangelogSectionViewModel): void {
        let date = (typeof viewModel.date === "string" ? new Date(viewModel.date) : viewModel.date).toISOString().substring(0, 10);
        let messages: string[] = [];
        for (let i = 0; i < viewModel.entries.length; i++) {
            let message = viewModel.entries[i].message.trim();
            if (message) {
                messages.push(viewModel.entries[i].message);
            }
        }

        this.isLoading = true;
        this.newsService
            .addNews({ date, messages })
            .then(() => this.refreshNews())
            .catch(() => this.isError = true);
    }

    public delete(viewModel: IChangelogSectionViewModel): void {
        let date = viewModel.date.toISOString().substring(0, 10);
        this.isLoading = true;
        this.newsService
            .deleteNews(date)
            .then(() => this.refreshNews())
            .catch(() => this.isError = true);
    }

    private refreshNews(): void {
        this.isLoading = true;
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

        this.isLoading = false;
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
                    editable: false,
                };
            }

            let messages = entries[dateStr];
            for (let j = 0; j < messages.length; j++) {
                currentSection.entries.push({ message: messages[j] });
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
