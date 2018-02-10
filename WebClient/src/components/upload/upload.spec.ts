import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { ActivatedRoute, Router, Params } from "@angular/router";
import { FormsModule } from "@angular/forms";
import { NO_ERRORS_SCHEMA, DebugElement, ChangeDetectorRef } from "@angular/core";
import { By } from "@angular/platform-browser";
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import { DatePipe, PercentPipe } from "@angular/common";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { Decimal } from "decimal.js";

import { UploadComponent } from "./upload";
import { ExponentialPipe } from "../../pipes/exponentialPipe";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { UploadService, IUpload } from "../../services/uploadService/uploadService";
import { SettingsService } from "../../services/settingsService/settingsService";

describe("UploadComponent", () => {
    let component: UploadComponent;
    let fixture: ComponentFixture<UploadComponent>;
    let routeParams: BehaviorSubject<Params>;
    let appInsights: AppInsightsService;
    let userInfoSubject: BehaviorSubject<IUserInfo>;

    let uploadServiceGetResolve: (upload: IUpload) => Promise<void>;
    let uploadServiceGetReject: () => Promise<void>;

    let uploadServiceDeleteResolve: () => Promise<void>;
    let uploadServiceDeleteReject: () => Promise<void>;

    const settings = SettingsService.defaultSettings;

    let settingsSubject = new BehaviorSubject(settings);

    beforeEach(async(() => {
        userInfoSubject = new BehaviorSubject({ isLoggedIn: false });
        let authenticationService = { userInfo: () => userInfoSubject };
        let uploadService = {
            get: (): Promise<IUpload> => new Promise<IUpload>((resolve, reject) => {
                uploadServiceGetResolve = (upload) => {
                    resolve(upload);
                    return fixture.whenStable().then(() => {
                        fixture.detectChanges();
                    });
                };
                uploadServiceGetReject = () => {
                    reject();
                    return fixture.whenStable().then(() => {
                        fixture.detectChanges();
                    });
                };
            }),
            delete: (): Promise<void> => new Promise<void>((resolve, reject) => {
                uploadServiceDeleteResolve = () => {
                    resolve();
                    return fixture.whenStable().then(() => {
                        fixture.detectChanges();
                    });
                };
                uploadServiceDeleteReject = () => {
                    reject();
                    return fixture.whenStable().then(() => {
                        fixture.detectChanges();
                    });
                };
            }),
        };
        let router = { navigate: (): void => void 0 };
        routeParams = new BehaviorSubject({ id: 123 });
        let route = { params: routeParams };
        let settingsService = { settings: () => settingsSubject };
        let changeDetectorRef = { markForCheck: (): void => void 0 };
        appInsights = jasmine.createSpyObj("appInsights", ["trackEvent", "startTrackEvent", "stopTrackEvent"]);
        let modalService = { open: (): void => void 0 };
        TestBed.configureTestingModule(
            {
                imports: [FormsModule],
                declarations:
                    [
                        UploadComponent,
                        ExponentialPipe,
                    ],
                providers:
                    [
                        { provide: AuthenticationService, useValue: authenticationService },
                        { provide: ActivatedRoute, useValue: route },
                        { provide: Router, useValue: router },
                        { provide: UploadService, useValue: uploadService },
                        { provide: SettingsService, useValue: settingsService },
                        { provide: ChangeDetectorRef, useValue: changeDetectorRef },
                        { provide: AppInsightsService, useValue: appInsights },
                        { provide: NgbModal, useValue: modalService },
                        DatePipe,
                        PercentPipe,
                        ExponentialPipe,
                    ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(UploadComponent);
                component = fixture.componentInstance;

                // Initial bindings
                fixture.detectChanges();
            });
    }));

    describe("Error message", () => {
        it("should show an error when the upload service fails", async(() => {
            uploadServiceGetReject()
                .then(() => {
                    fixture.detectChanges();

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).not.toBeNull("Error message not found");
                });
        }));

        it("should not show an error when the upload service succeeds", async(() => {
            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");
                });
        }));
    });

    describe("Upload summary data", () => {
        it("should initially display placeholder data", async(() => {
            let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
            expect(errorMessage).toBeNull("Error message found");

            let summaryData = fixture.debugElement.query(By.css(".col-md-6 ul"));
            expect(summaryData).not.toBeNull();

            let items = summaryData.queryAll(By.css("li"));
            expect(items.length).toEqual(3);

            let userNameItem = items[0];
            let userNameValueElement = userNameItem.query(By.css("span"));
            expect(userNameValueElement.nativeElement.classList.contains("text-muted")).toEqual(true);
            expect(getNormalizedTextContent(userNameValueElement)).toEqual("(Anonymous)");

            let uploadTimeItem = items[1];
            let uploadTimeValueElement = uploadTimeItem.query(By.css("span"));
            expect(getNormalizedTextContent(uploadTimeValueElement)).toEqual("");

            let playStyleItem = items[2];
            let playStyleValueElement = playStyleItem.query(By.css("span"));
            expect(getNormalizedTextContent(playStyleValueElement)).toEqual("");
        }));

        it("should display anonymous data", async(() => {
            let datePipe = TestBed.get(DatePipe) as DatePipe;

            let upload = getUpload();
            delete upload.user;
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    let summaryData = fixture.debugElement.query(By.css(".col-md-6 ul"));
                    expect(summaryData).not.toBeNull();

                    let items = summaryData.queryAll(By.css("li"));
                    expect(items.length).toEqual(3);

                    let userNameItem = items[0];
                    let userNameValueElement = userNameItem.query(By.css("span"));
                    expect(userNameValueElement.nativeElement.classList.contains("text-muted")).toEqual(true);
                    expect(getNormalizedTextContent(userNameValueElement)).toEqual("(Anonymous)");

                    let uploadTimeItem = items[1];
                    let uploadTimeValueElement = uploadTimeItem.query(By.css("span"));
                    expect(getNormalizedTextContent(uploadTimeValueElement)).toEqual(datePipe.transform(upload.timeSubmitted, "short"));

                    let playStyleItem = items[2];
                    let playStyleValueElement = playStyleItem.query(By.css("span"));
                    expect(getNormalizedTextContent(playStyleValueElement)).toEqual("Hybrid");
                });
        }));

        it("should display public data", async(() => {
            let datePipe = TestBed.get(DatePipe) as DatePipe;

            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    let summaryData = fixture.debugElement.query(By.css(".col-md-6 ul"));
                    expect(summaryData).not.toBeNull();

                    let items = summaryData.queryAll(By.css("li"));
                    expect(items.length).toEqual(3);

                    let userNameItem = items[0];
                    let userNameValueElement = userNameItem.query(By.css("a"));
                    expect(userNameValueElement.properties.routerLink).toEqual(`/users/${upload.user.name}`);
                    expect(userNameValueElement.nativeElement.classList.contains("text-muted")).toEqual(false);
                    expect(getNormalizedTextContent(userNameValueElement)).toEqual(upload.user.name);

                    let uploadTimeItem = items[1];
                    let uploadTimeValueElement = uploadTimeItem.query(By.css("span"));
                    expect(getNormalizedTextContent(uploadTimeValueElement)).toEqual(datePipe.transform(upload.timeSubmitted, "short"));

                    let playStyleItem = items[2];
                    let playStyleValueElement = playStyleItem.query(By.css("span"));
                    expect(getNormalizedTextContent(playStyleValueElement)).toEqual("Hybrid");
                });
        }));
    });

    describe("View Save Data button", () => {
        it("should display", async(() => {
            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    let buttons = fixture.debugElement.queryAll(By.css(".col-md-6.pull-right button"));
                    let found = false;
                    for (let i = 0; i < buttons.length; i++) {
                        if (getNormalizedTextContent(buttons[i]) === "View Save Data") {
                            found = true;
                        }
                    }

                    expect(found).toEqual(true, "Could not find the 'View Save Data' button");
                });
        }));

        it("should show the modal when clicked", async(() => {
            let modalService = TestBed.get(NgbModal) as NgbModal;
            spyOn(modalService, "open").and.returnValue({ result: Promise.resolve() });

            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    let buttons = fixture.debugElement.queryAll(By.css(".col-md-6.pull-right button"));
                    let viewSaveDataButton: DebugElement;
                    for (let i = 0; i < buttons.length; i++) {
                        if (getNormalizedTextContent(buttons[i]) === "View Save Data") {
                            viewSaveDataButton = buttons[i];
                        }
                    }

                    expect(viewSaveDataButton).toBeDefined("Could not find the 'View Save Data' button");

                    viewSaveDataButton.nativeElement.click();
                    expect(modalService.open).toHaveBeenCalled();
                });
        }));
    });

    describe("Delete button", () => {
        it("should display when it's the user's upload", async(() => {
            let upload = getUpload();
            userInfoSubject.next({
                isLoggedIn: true,
                id: "someUserId",
                username: upload.user.name,
                email: "someEmail",
            });
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    let buttons = fixture.debugElement.queryAll(By.css(".col-md-6.pull-right button"));
                    let deleteButton: DebugElement;
                    for (let i = 0; i < buttons.length; i++) {
                        if (getNormalizedTextContent(buttons[i]) === "Delete") {
                            deleteButton = buttons[i];
                        }
                    }

                    expect(deleteButton).toBeDefined("Could not find the 'Delete' button");
                });
        }));

        it("should show the modal when clicked", async(() => {
            let modalService = TestBed.get(NgbModal) as NgbModal;
            spyOn(modalService, "open").and.returnValue({ result: Promise.resolve() });

            let upload = getUpload();
            userInfoSubject.next({
                isLoggedIn: true,
                id: "someUserId",
                username: upload.user.name,
                email: "someEmail",
            });
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    let buttons = fixture.debugElement.queryAll(By.css(".col-md-6.pull-right button"));
                    let deleteButton: DebugElement;
                    for (let i = 0; i < buttons.length; i++) {
                        if (getNormalizedTextContent(buttons[i]) === "Delete") {
                            deleteButton = buttons[i];
                        }
                    }

                    expect(deleteButton).toBeDefined("Could not find the 'Delete' button");

                    deleteButton.nativeElement.click();
                    expect(modalService.open).toHaveBeenCalled();
                });
        }));

        it("should delete the upload when confirmed", async(() => {
            let router = TestBed.get(Router) as Router;
            spyOn(router, "navigate").and.returnValue(Promise.resolve());

            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    let closeModal = jasmine.createSpy("closeModal");
                    component.deleteUpload(closeModal);

                    uploadServiceDeleteResolve()
                        .then(() => fixture.whenStable()) // Not sure why the navigation promise needs yet another wait for stability
                        .then(() => {
                            fixture.detectChanges();

                            expect(router.navigate).toHaveBeenCalledWith([`/users/${upload.user.name}`]);
                            expect(closeModal).toHaveBeenCalled();

                            errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                            expect(errorMessage).toBeNull("Error message found");
                        });
                });
        }));

        it("should show an error when the upload service fails to delete the upload", async(() => {
            let router = TestBed.get(Router) as Router;
            spyOn(router, "navigate");

            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    let closeModal = jasmine.createSpy("closeModal");
                    component.deleteUpload(closeModal);

                    uploadServiceDeleteReject()
                        .then(() => fixture.whenStable()) // Not sure why the navigation promise needs yet another wait for stability
                        .then(() => {
                            fixture.detectChanges();

                            expect(router.navigate).not.toHaveBeenCalled();
                            expect(closeModal).toHaveBeenCalled();

                            errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                            expect(errorMessage).not.toBeNull("Error message not found");
                        });
                });
        }));

        it("should not display when it's not the user's upload", async(() => {
            let upload = getUpload();
            userInfoSubject.next({
                isLoggedIn: true,
                id: "someOtherUserId",
                username: "someOtherUsername",
                email: "someOtherEmail",
            });
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    let buttons = fixture.debugElement.queryAll(By.css(".col-md-6.pull-right button"));
                    let deleteButton: DebugElement;
                    for (let i = 0; i < buttons.length; i++) {
                        if (getNormalizedTextContent(buttons[i]) === "Delete") {
                            deleteButton = buttons[i];
                        }
                    }

                    expect(deleteButton).toBeUndefined("Unexpectedly found the 'Delete' button");
                });
        }));
    });

    describe("Miscellaneous Stats", () => {
        it("should display data", async(() => {
            let exponentialPipe = TestBed.get(ExponentialPipe) as ExponentialPipe;
            let percentPipe = TestBed.get(PercentPipe) as PercentPipe;

            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let expectedValues: { name: string, stat: string, value: string, type: "exponential" | "percent" }[] =
                        [
                            { name: "Hero Souls Spent", stat: "heroSoulsSpent", value: "1.7754273760949743393e+501", type: "exponential" },
                            { name: "Hero Souls Sacrificed", stat: "heroSoulsSacrificed", value: "4.451222095586916e+5129", type: "exponential" },
                            { name: "Ancient Souls Earned", stat: "totalAncientSouls", value: "25648", type: "exponential" },
                            { name: "Transcendent Power", stat: "transcendentPower", value: "0.24989526487039584", type: "percent" },
                            { name: "Titan Damage", stat: "titanDamage", value: "4.4512220955869015e+5129", type: "exponential" },
                            { name: "Highest Zone", stat: "highestZoneThisTranscension", value: "25621", type: "exponential" },
                            { name: "Highest Zone (Lifetime)", stat: "highestZoneLifetime", value: "264669", type: "exponential" },
                            { name: "Ascensions", stat: "ascensionsThisTranscension", value: "3", type: "exponential" },
                            { name: "Ascensions (Lifetime)", stat: "ascensionsLifetime", value: "3370", type: "exponential" },
                            { name: "Rubies", stat: "rubies", value: "107", type: "exponential" },
                            { name: "Autoclickers", stat: "autoclickers", value: "10", type: "exponential" },
                        ];

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    let table = fixture.debugElement.query(By.css("table"));
                    expect(table).not.toBeNull();

                    let rows = table.queryAll(By.css("tbody tr"));
                    expect(rows.length).toEqual(expectedValues.length);

                    for (let i = 0; i < rows.length; i++) {
                        let cells = rows[i].children;
                        let expected = expectedValues[i];

                        let expectedName = expected.name;
                        expect(getNormalizedTextContent(cells[0])).toEqual(expectedName);

                        let expectedValue = expected.value;
                        let expectedFormattedValue: string;
                        switch (expected.type) {
                            case "exponential":
                                {
                                    expectedFormattedValue = exponentialPipe.transform(new Decimal(expectedValue));
                                    break;
                                }
                            case "percent":
                                {
                                    expectedFormattedValue = percentPipe.transform(expectedValue, "1.1-3");
                                    break;
                                }
                            default:
                                {
                                    fail();
                                }
                        }

                        expect(getNormalizedTextContent(cells[1])).toEqual(expectedFormattedValue);
                    }
                });
        }));
    });

    function getUpload(): IUpload {
        return {
            id: 1234,
            user: {
                name: "Omnes",
                clanName: "The Clickvengers",
            },
            timeSubmitted: "2017-06-17T16:40:22",
            content: "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",
            playStyle: "hybrid",
            isScrubbed: false,
        };
    }

    function getNormalizedTextContent(element: DebugElement): string {
        let nativeElement: HTMLElement = element.nativeElement;
        return nativeElement.textContent.trim().replace(/\s\s+/g, " ");
    }
});
