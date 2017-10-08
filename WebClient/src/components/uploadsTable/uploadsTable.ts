import { Component, OnInit, Input } from "@angular/core";

import { UploadService, IUploadSummaryListResponse } from "../../services/uploadService/uploadService";

interface IUploadViewModel {
    id: number;
    uploadTime: Date;
}

@Component({
    selector: "uploadsTable",
    templateUrl: "./uploadsTable.html",
})
export class UploadsTableComponent implements OnInit {
    public uploads: IUploadViewModel[];
    public isError: boolean;
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

    constructor(private uploadService: UploadService) { }

    public ngOnInit(): void {
        this.populateUploads();
    }

    private populateUploads(): void {
        // TODO #175 - Pass the user name
        this.uploadService
            .getUploads(this.page, this.count)
            .then(response => this.handleData(response))
            .catch(() => this.isError = true);
    }

    private handleData(response: IUploadSummaryListResponse): void {
        if (!response || !response.uploads) {
            this.isError = true;
            return;
        }

        this.uploads = [];

        let uploads = response.uploads;
        for (let upload of uploads) {
            this.uploads.push({
                id: upload.id,
                uploadTime: new Date(upload.timeSubmitted),
            });
        }

        if (response.pagination) {
            this.totalUploads = response.pagination.count;
        }
    }
}
