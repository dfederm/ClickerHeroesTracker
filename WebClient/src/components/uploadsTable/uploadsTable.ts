import { Component, OnInit, Input } from "@angular/core";
import { UserService, IUploadSummaryListResponse } from "../../services/userService/userService";
import { Decimal } from "decimal.js";

interface IUploadViewModel {
    id: number;
    uploadTime: Date;
    ascensionNumber: number;
    zone: number;
    souls: Decimal;
}

@Component({
    selector: "uploadsTable",
    templateUrl: "./uploadsTable.html",
})
export class UploadsTableComponent implements OnInit {
    public uploads: IUploadViewModel[];
    public isError: boolean;
    public isLoading: boolean;
    public totalUploads: number;

    @Input()
    public count: number;

    @Input()
    public paginate: boolean;

    private _userName: string;
    private _page = 1;

    public get page(): number {
        return this._page;
    }

    public set page(page: number) {
        this._page = page;
        this.populateUploads();
    }

    public get userName(): string {
        return this._userName;
    }

    @Input()
    public set userName(userName: string) {
        this._userName = userName;
        this.populateUploads();
    }

    constructor(private readonly userService: UserService) { }

    public ngOnInit(): void {
        this.populateUploads();
    }

    private populateUploads(): void {
        this.isLoading = true;
        this.userService
            .getUploads(this.userName, this.page, this.count)
            .then(response => this.handleData(response))
            .catch(() => this.isError = true);
    }

    private handleData(response: IUploadSummaryListResponse): void {
        if (!response || !response.uploads) {
            this.isError = true;
            return;
        }

        this.isLoading = false;
        this.uploads = [];

        let uploads = response.uploads;
        for (let upload of uploads) {
            this.uploads.push({
                id: upload.id,
                uploadTime: new Date(upload.timeSubmitted),
                ascensionNumber: upload.ascensionNumber,
                zone: upload.zone,
                souls: new Decimal(upload.souls),
            });
        }

        if (response.pagination) {
            this.totalUploads = response.pagination.count;
        }
    }
}
