import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { DatePipe } from "@angular/common";
import { NO_ERRORS_SCHEMA } from "@angular/core";

import { UploadsTableComponent } from "./uploadsTable";
import { UserService, IUploadSummaryListResponse } from "../../services/userService/userService";

describe("UploadsTableComponent", () => {
    let component: UploadsTableComponent;
    let fixture: ComponentFixture<UploadsTableComponent>;

    let uploads =
        [
            { id: 0, timeSubmitted: "2017-01-01T00:00:00", playStyle: "idle" },
            { id: 1, timeSubmitted: "2017-01-02T00:00:00", playStyle: "idle" },
            { id: 2, timeSubmitted: "2017-01-03T00:00:00", playStyle: "idle" },
            { id: 3, timeSubmitted: "2017-01-04T00:00:00", playStyle: "idle" },
            { id: 4, timeSubmitted: "2017-01-05T00:00:00", playStyle: "idle" },
            { id: 5, timeSubmitted: "2017-01-06T00:00:00", playStyle: "idle" },
            { id: 6, timeSubmitted: "2017-01-07T00:00:00", playStyle: "idle" },
            { id: 7, timeSubmitted: "2017-01-08T00:00:00", playStyle: "idle" },
        ];

    beforeEach(async(() => {
        let userService = {
            getUploads(userName: string, page: number, count: number): Promise<IUploadSummaryListResponse> {
                expect(userName).toEqual(component.userName);
                let uploadsResponse: IUploadSummaryListResponse = {
                    pagination:
                        {
                            count: uploads.length,
                            next: "someNext",
                            previous: "somePrevious",
                        },
                    uploads: uploads.slice((page - 1) * count, page * count),
                };
                return Promise.resolve(uploadsResponse);
            },
        };

        TestBed.configureTestingModule(
            {
                declarations: [UploadsTableComponent],
                providers:
                    [
                        { provide: UserService, useValue: userService },
                        DatePipe,
                    ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(UploadsTableComponent);
                component = fixture.componentInstance;
            });
    }));

    it("should display a table without pagination when paginate=false", async(() => {
        component.userName = "someUserName";
        component.count = 3;
        component.paginate = false;

        verifyTable(1);
    }));

    it("should display a table with pagination when paginate=true", async(() => {
        component.userName = "someUserName";
        component.count = 3;
        component.paginate = true;

        verifyTable(1);
    }));

    it("should update the table when the page updates", async(() => {
        component.userName = "someUserName";
        component.count = 3;
        component.paginate = true;

        verifyTable(1)
            .then(() => {
                component.page = 2;
                verifyTable(2);
            });
    }));

    it("should update the table when the userName updates", async(() => {
        component.userName = "someUserName";
        component.count = 3;
        component.paginate = true;

        verifyTable(1)
            .then(() => {
                component.userName = "someOtherUserName";
                verifyTable(1);
            });
    }));

    it("should display an error when the upload service errors", async(() => {
        let userService = TestBed.get(UserService);
        spyOn(userService, "getUploads").and.returnValue(Promise.reject("someReason"));

        fixture.detectChanges();
        fixture.whenStable().then(() => {
            fixture.detectChanges();

            let error = fixture.debugElement.query(By.css("p"));
            expect(error.nativeElement.textContent.trim()).toEqual("There was a problem getting your uploads");
        });
    }));

    it("should display an error when the upload service returns an invalid response", async(() => {
        let userService = TestBed.get(UserService);
        spyOn(userService, "getUploads").and.returnValue(Promise.resolve({}));

        fixture.detectChanges();
        fixture.whenStable().then(() => {
            fixture.detectChanges();

            let error = fixture.debugElement.query(By.css("p"));
            expect(error.nativeElement.textContent.trim()).toEqual("There was a problem getting your uploads");
        });
    }));

    function verifyTable(page: number): Promise<void> {
        let datePipe = TestBed.get(DatePipe) as DatePipe;

        fixture.detectChanges();
        return fixture.whenStable().then(() => {
            fixture.detectChanges();

            let rows = fixture.debugElement.query(By.css("tbody")).children;
            expect(rows.length).toEqual(component.count);

            for (let i = 0; i < rows.length; i++) {
                let expectedUpload = uploads[((page - 1) * component.count) + i];

                let cells = rows[i].children;
                expect(cells.length).toEqual(2);

                let dateCell = cells[0];
                expect(dateCell.nativeElement.textContent.trim()).toEqual(datePipe.transform(expectedUpload.timeSubmitted, "short"));

                let viewCell = cells[1];
                let link = viewCell.query(By.css("a"));
                expect(link.properties.routerLink).toEqual(`/uploads/${expectedUpload.id}`);
            }

            let pagination = fixture.debugElement.query(By.css("ngb-pagination"));
            if (component.paginate) {
                expect(pagination).not.toBeNull();
                expect(pagination.properties.collectionSize).toEqual(uploads.length);
                expect(pagination.properties.page).toEqual(page);
                expect(pagination.properties.pageSize).toEqual(component.count);
                expect(pagination.properties.maxSize).toEqual(5);
                expect(pagination.properties.rotate).toEqual(true);
                expect(pagination.properties.ellipses).toEqual(false);
                expect(pagination.properties.boundaryLinks).toEqual(true);
            } else {
                expect(pagination).toBeNull();
            }
        });
    }
});
