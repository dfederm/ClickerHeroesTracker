import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { DatePipe } from "@angular/common";
import { ChangeDetectorRef, Component, Input, Pipe, PipeTransform } from "@angular/core";
import { Decimal } from "decimal.js";

import { UploadsTableComponent } from "./uploadsTable";
import { UserService, IUploadSummaryListResponse, IUploadSummary } from "../../services/userService/userService";
import { SettingsService } from "../../services/settingsService/settingsService";
import { BehaviorSubject } from "rxjs";
import { ExponentialPipe } from "src/pipes/exponentialPipe";
import { provideRouter, RouterLink } from "@angular/router";
import { NgbPagination } from "@ng-bootstrap/ng-bootstrap";
import { NgxSpinnerModule } from "ngx-spinner";

describe("UploadsTableComponent", () => {
    let component: UploadsTableComponent;
    let fixture: ComponentFixture<UploadsTableComponent>;

    const settings = SettingsService.defaultSettings;
    let settingsSubject = new BehaviorSubject(settings);

    let uploads: IUploadSummary[] = [];
    for (let i = 0; i < 8; i++) {
        uploads.push({
            id: i,
            timeSubmitted: `2017-02-0${i + 1}T00:00:00`,
            saveTime: `2017-01-0${i + 1}T00:00:00`,
            ascensionNumber: i,
            zone: 100 * i,
            souls: `1e${100 * i}`,
        });
    }

    @Component({ selector: "ngx-spinner", template: "", standalone: true })
    class MockNgxSpinnerComponent {
        @Input()
        public fullScreen: boolean;
    }

    const exponentialPipeTransform = (value: string | Decimal | number) => "exponentialPipe(" + value + ")";

    @Pipe({ name: 'exponential', standalone: true })
    class MockExponentialPipe implements PipeTransform {
        public transform = exponentialPipeTransform;
    }

    @Component({ selector: "ngb-pagination", template: "", standalone: true })
    class MockNgbPaginationComponent {
        @Input()
        public collectionSize: number;

        @Input()
        public page: number;

        @Input()
        public pageSize: number;

        @Input()
        public maxSize: number;

        @Input()
        public rotate: boolean;

        @Input()
        public ellipses: boolean;

        @Input()
        public boundaryLinks: boolean;
    }
    
    beforeEach(async () => {
        let userService = {
            getUploads(userName: string, page: number, count: number): Promise<IUploadSummaryListResponse> {
                expect(userName).toEqual(component.userName);
                let uploadsResponse: IUploadSummaryListResponse = {
                    pagination: {
                        count: uploads.length,
                        next: "someNext",
                        previous: "somePrevious",
                    },
                    uploads: uploads.slice((page - 1) * count, page * count),
                };
                return Promise.resolve(uploadsResponse);
            },
        };
        let settingsService = { settings: () => settingsSubject };
        let changeDetectorRef = { markForCheck: (): void => void 0 };

        await TestBed.configureTestingModule(
            {
                imports: [
                    UploadsTableComponent,
                ],
                providers: [
                    provideRouter([]),
                    { provide: UserService, useValue: userService },
                    { provide: SettingsService, useValue: settingsService },
                    { provide: ChangeDetectorRef, useValue: changeDetectorRef },
                    DatePipe,
                ],
            })
            .compileComponents();
        TestBed.overrideComponent(UploadsTableComponent, {
            remove: { imports: [NgxSpinnerModule, ExponentialPipe, NgbPagination] },
            add: { imports: [MockNgxSpinnerComponent, MockExponentialPipe, MockNgbPaginationComponent] },
        });

        fixture = TestBed.createComponent(UploadsTableComponent);
        component = fixture.componentInstance;
    });

    it("should display a table without pagination when paginate=false", async () => {
        component.userName = "someUserName";
        component.count = 3;
        component.paginate = false;
        await verifyTable(1);
    });

    it("should display a table with pagination when paginate=true", async () => {
        component.userName = "someUserName";
        component.count = 3;
        component.paginate = true;
        await verifyTable(1);
    });

    it("should update the table when the page updates", async () => {
        component.userName = "someUserName";
        component.count = 3;
        component.paginate = true;
        await verifyTable(1);

        component.page = 2;
        await verifyTable(2);
    });

    it("should update the table when the userName updates", async () => {
        component.userName = "someUserName";
        component.count = 3;
        component.paginate = true;
        await verifyTable(1);

        component.userName = "someOtherUserName";
        return await verifyTable(1);
    });

    it("should display an error when the upload service errors", async () => {
        let userService = TestBed.inject(UserService);
        spyOn(userService, "getUploads").and.returnValue(Promise.reject("someReason"));

        fixture.detectChanges();
        await fixture.whenStable();
        fixture.detectChanges();

        let error = fixture.debugElement.query(By.css("p"));
        expect(error.nativeElement.textContent.trim()).toEqual("There was a problem getting your uploads");
    });

    it("should display an error when the upload service returns an invalid response", async () => {
        let userService = TestBed.inject(UserService);
        spyOn(userService, "getUploads").and.returnValue(Promise.resolve({} as any));

        fixture.detectChanges();
        await fixture.whenStable();
        fixture.detectChanges();

        let error = fixture.debugElement.query(By.css("p"));
        expect(error.nativeElement.textContent.trim()).toEqual("There was a problem getting your uploads");
    });

    async function verifyTable(page: number): Promise<void> {
        let datePipe = TestBed.inject(DatePipe);

        fixture.detectChanges();
        await fixture.whenStable();
        fixture.detectChanges();

        let rows = fixture.debugElement.query(By.css("tbody")).children;
        expect(rows.length).toEqual(component.count);

        for (let i = 0; i < rows.length; i++) {
            let expectedUpload = uploads[((page - 1) * component.count) + i];

            let cells = rows[i].children;
            expect(cells.length).toEqual(5);

            let ascensionCell = cells[0];
            expect(ascensionCell.nativeElement.textContent.trim()).toEqual(exponentialPipeTransform(expectedUpload.ascensionNumber));

            let zoneCell = cells[1];
            expect(zoneCell.nativeElement.textContent.trim()).toEqual(exponentialPipeTransform(expectedUpload.zone));

            let soulsCell = cells[2];
            expect(soulsCell.nativeElement.textContent.trim()).toEqual(exponentialPipeTransform(new Decimal(expectedUpload.souls)));

            let dateCell = cells[3];
            expect(dateCell.properties.title).toEqual("Uploaded " + datePipe.transform(expectedUpload.timeSubmitted, "short"));
            expect(dateCell.nativeElement.textContent.trim()).toEqual(datePipe.transform(expectedUpload.saveTime, "short"));

            let viewCell = cells[4];
            let link = viewCell.query(By.css("a"));
            let routerLink = link.injector.get(RouterLink) as RouterLink;
            expect(routerLink.href).toEqual(`/uploads/${expectedUpload.id}`);
        }

        let pagination = fixture.debugElement.query(By.css("ngb-pagination"))?.componentInstance as MockNgbPaginationComponent;
        if (component.paginate) {
            expect(pagination).not.toBeNull();
            expect(pagination.collectionSize).toEqual(uploads.length);
            expect(pagination.page).toEqual(page);
            expect(pagination.pageSize).toEqual(component.count);
            expect(pagination.maxSize).toEqual(5);
            expect(pagination.rotate).toEqual(true);
            expect(pagination.ellipses).toEqual(false);
            expect(pagination.boundaryLinks).toEqual(true);
        } else {
            expect(pagination).toBeUndefined();
        }
    }
});
