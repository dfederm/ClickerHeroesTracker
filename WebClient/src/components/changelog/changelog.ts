import { Component, OnInit, Input } from "@angular/core";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";

import { NewsService, ISiteNewsEntryListResponse } from "../../services/newsService/newsService";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { FormsModule } from "@angular/forms";
import { DatePipe } from "@angular/common";

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
    imports: [
        DatePipe,
        FormsModule,
        NgxSpinnerModule,
    ],
    standalone: true,
})
export class ChangelogComponent implements OnInit {
    public sections: IChangelogSectionViewModel[];
    public isError: boolean;

    public canEdit: boolean;

    @Input()
    public showDates: boolean;

    @Input()
    public maxEntries?: number;

    constructor(
        private readonly newsService: NewsService,
        private readonly authenticationService: AuthenticationService,
        private readonly spinnerService: NgxSpinnerService,
    ) { }

    public ngOnInit(): void {
        this.refreshNews();

        // Editing is only possible when showing dates
        if (this.showDates) {
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

        this.spinnerService.show("changelog");
        this.newsService
            .addNews({ date, messages })
            .then(() => this.refreshNews())
            .catch(() => this.isError = true)
            .finally(() => this.spinnerService.hide("changelog"));
    }

    public delete(viewModel: IChangelogSectionViewModel): void {
        let date = viewModel.date.toISOString().substring(0, 10);
        this.spinnerService.show("changelog");
        this.newsService
            .deleteNews(date)
            .then(() => this.refreshNews())
            .catch(() => this.isError = true)
            .finally(() => this.spinnerService.hide("changelog"));
    }

    private refreshNews(): void {
        this.spinnerService.show("changelog");
        this.newsService
            .getNews()
            .then(response => this.handleData(response))
            .catch(() => this.isError = true)
            .finally(() => this.spinnerService.hide("changelog"));
    }

    private handleData(response: ISiteNewsEntryListResponse): void {
        if (!response || !response.entries) {
            this.isError = true;
            return;
        }

        let entries = response.entries;

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

            if (this.showDates || !currentSection) {
                if (currentSection) {
                    this.sections.push(currentSection);
                }

                currentSection = {
                    date: this.showDates ? date : null,
                    entries: [],
                    editable: false,
                };
            }

            let messages = entries[dateStr];
            for (let j = 0; j < messages.length; j++) {
                currentSection.entries.push({ message: messages[j] });
                numEntries++;
                if (this.isFull(numEntries)) {
                    break;
                }
            }

            if (this.isFull(numEntries)) {
                break;
            }
        }

        if (currentSection) {
            this.sections.push(currentSection);
        }
    }

    private isFull(numEntries: number): boolean {
        return this.maxEntries && numEntries >= this.maxEntries;
    }
}
