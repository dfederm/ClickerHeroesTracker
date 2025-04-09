import { Component, OnInit, Input } from "@angular/core";
import { UserService, IUploadSummaryListResponse } from "../../services/userService/userService";
import { Decimal } from "decimal.js";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";
import { NgbPagination } from "@ng-bootstrap/ng-bootstrap";
import { RouterLink } from "@angular/router";
import { DatePipe } from "@angular/common";
import { ExponentialPipe } from "src/pipes/exponentialPipe";

interface IUploadViewModel {
    id: number;
    saveTime: Date;
    uploadTime: Date;
    ascensionNumber: number;
    zone: number;
    souls: Decimal;
}

@Component({
    selector: "uploadsTable",
    templateUrl: "./uploadsTable.html",
    imports: [
        DatePipe,
        ExponentialPipe,
        NgbPagination,
        NgxSpinnerModule,
        RouterLink,
    ]
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
    private _isInitialized = false;

    public get page(): number {
        return this._page;
    }

    public set page(page: number) {
        this._page = page;
        if (this._isInitialized) {
            this.populateUploads();
        }
    }

    public get userName(): string {
        return this._userName;
    }

    @Input()
    public set userName(userName: string) {
        this._userName = userName;
        if (this._isInitialized) {
            this.populateUploads();
        }
    }

    constructor(
        private readonly userService: UserService,
        private readonly spinnerService: NgxSpinnerService,
    ) { }

    public ngOnInit(): void {
        this.populateUploads();
        this._isInitialized = true;
    }

    private populateUploads(): void {
        this.isError = false;
        this.spinnerService.show("uploadsTable");
        this.userService
            .getUploads(this.userName, this.page, this.count)
            .then(response => this.handleData(response))
            .catch(() => this.isError = true)
            .finally(() => {
                this.spinnerService.hide("uploadsTable");
            });
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
                saveTime: new Date(upload.saveTime),
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
