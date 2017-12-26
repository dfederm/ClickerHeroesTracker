import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { ActivatedRoute, Router, Params } from "@angular/router";
import { FormsModule } from "@angular/forms";
import { NO_ERRORS_SCHEMA, DebugElement, ChangeDetectorRef } from "@angular/core";
import { By } from "@angular/platform-browser";
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import { DatePipe, PercentPipe } from "@angular/common";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";

import { UploadComponent } from "./upload";
import { ExponentialPipe } from "../../pipes/exponentialPipe";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { UploadService } from "../../services/uploadService/uploadService";
import { SettingsService } from "../../services/settingsService/settingsService";
import { IUpload } from "../../models";

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
                id: upload.user.id,
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
                id: upload.user.id,
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

    describe("Suggestion Types", () => {
        it("should default to available souls including souls from ascension", async(() => {
            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    let suggestionTypes = fixture.debugElement.queryAll(By.css("input[type='radio']"));
                    expect(suggestionTypes.length).toEqual(2);

                    expect((suggestionTypes[0].nativeElement as HTMLInputElement).checked).toEqual(true);
                    expect((suggestionTypes[1].nativeElement as HTMLInputElement).checked).toEqual(false);

                    let useSoulsFromAscension = fixture.debugElement.query(By.css("input[name='useSoulsFromAscension']"));
                    expect(useSoulsFromAscension).not.toBeNull("Couldn't find the 'Use souls from ascension' checkbox");
                });
        }));

        it("should hide souls from ascension after selecting the Rules of Thumb", async(() => {
            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    let suggestionTypes = fixture.debugElement.queryAll(By.css("input[type='radio']"));
                    expect(suggestionTypes.length).toEqual(2);

                    suggestionTypes[1].nativeElement.click();
                    fixture.detectChanges();

                    let useSoulsFromAscension = fixture.debugElement.query(By.css("input[name='useSoulsFromAscension']"));
                    expect(useSoulsFromAscension).toBeNull("Unexpectedly found the 'Use souls from ascension' checkbox");
                });
        }));

        it("should re-show souls from ascension after selecting Available Souls", async(() => {
            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    let suggestionTypes = fixture.debugElement.queryAll(By.css("input[type='radio']"));
                    expect(suggestionTypes.length).toEqual(2);

                    suggestionTypes[1].nativeElement.click();
                    fixture.detectChanges();

                    suggestionTypes[0].nativeElement.click();
                    fixture.detectChanges();

                    let useSoulsFromAscension = fixture.debugElement.query(By.css("input[name='useSoulsFromAscension']"));
                    expect(useSoulsFromAscension).not.toBeNull("Couldn't find the 'Use souls from ascension' checkbox");
                });
        }));
    });

    describe("Ancient Levels", () => {
        it("should display data based on hybrid playstyle and Available Souls using souls from ascension", done => {
            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let expectedValues: { name: string, level: number, suggested?: number, hasEffectiveLevel?: boolean }[] =
                        [
                            { name: "Argaiv", level: 200, suggested: 589 },
                            { name: "Atman", level: 10, suggested: 14, hasEffectiveLevel: true },
                            { name: "Berserker", level: 0 },
                            { name: "Bhaal", level: 0, suggested: 295 },
                            { name: "Bubos", level: 10, suggested: 12 },
                            { name: "Chawedo", level: 0 },
                            { name: "Chronos", level: 10, suggested: 13 },
                            { name: "Dogcog", level: 10, suggested: 12 },
                            { name: "Dora", level: 10, suggested: 14, hasEffectiveLevel: true },
                            { name: "Energon", level: 0 },
                            { name: "Fortuna", level: 10, suggested: 12, hasEffectiveLevel: true },
                            { name: "Fragsworth", level: 200, suggested: 295, hasEffectiveLevel: true },
                            { name: "Hecatoncheir", level: 0 },
                            { name: "Juggernaut", level: 50, suggested: 95, hasEffectiveLevel: true },
                            { name: "Kleptos", level: 0 },
                            { name: "Kumawakamaru", level: 10, suggested: 12, hasEffectiveLevel: true },
                            { name: "Libertas", level: 200, suggested: 546 },
                            { name: "Mammon", level: 200, suggested: 546 },
                            { name: "Mimzee", level: 200, suggested: 546, hasEffectiveLevel: true },
                            { name: "Morgulis", level: 40000, suggested: 346921 },
                            { name: "Nogardnit", level: 50, suggested: 155 },
                            { name: "Pluto", level: 0, suggested: 546 },
                            { name: "Revolc", level: 10, hasEffectiveLevel: true },
                            { name: "Siyalatas", level: 200, suggested: 589 },
                            { name: "Sniperino", level: 0 },
                            { name: "Vaagur", level: 10 },
                        ];

                    verify(expectedValues);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should display data based on hybrid playstyle and Available Souls without using souls from ascension", done => {
            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    // Don't use souls from ascension
                    let useSoulsFromAscension = fixture.debugElement.query(By.css("input[name='useSoulsFromAscension']"));
                    useSoulsFromAscension.nativeElement.click();
                    fixture.detectChanges();

                    let expectedValues: { name: string, level: number, suggested?: number, hasEffectiveLevel?: boolean }[] =
                        [
                            { name: "Argaiv", level: 200, suggested: 200 },
                            { name: "Atman", level: 10, suggested: 10, hasEffectiveLevel: true },
                            { name: "Berserker", level: 0 },
                            { name: "Bhaal", level: 0, suggested: 25 },
                            { name: "Bubos", level: 10, suggested: 10 },
                            { name: "Chawedo", level: 0 },
                            { name: "Chronos", level: 10, suggested: 10 },
                            { name: "Dogcog", level: 10, suggested: 10 },
                            { name: "Dora", level: 10, suggested: 10, hasEffectiveLevel: true },
                            { name: "Energon", level: 0 },
                            { name: "Fortuna", level: 10, suggested: 10, hasEffectiveLevel: true },
                            { name: "Fragsworth", level: 200, suggested: 200, hasEffectiveLevel: true },
                            { name: "Hecatoncheir", level: 0 },
                            { name: "Juggernaut", level: 50, suggested: 50, hasEffectiveLevel: true },
                            { name: "Kleptos", level: 0 },
                            { name: "Kumawakamaru", level: 10, suggested: 10, hasEffectiveLevel: true },
                            { name: "Libertas", level: 200, suggested: 200 },
                            { name: "Mammon", level: 200, suggested: 200 },
                            { name: "Mimzee", level: 200, suggested: 200, hasEffectiveLevel: true },
                            { name: "Morgulis", level: 40000, suggested: 40000 },
                            { name: "Nogardnit", level: 50, suggested: 50 },
                            { name: "Pluto", level: 0, suggested: 47 },
                            { name: "Revolc", level: 10, hasEffectiveLevel: true },
                            { name: "Siyalatas", level: 200, suggested: 200 },
                            { name: "Sniperino", level: 0 },
                            { name: "Vaagur", level: 10 },
                        ];

                    verify(expectedValues);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should display data based on hybrid playstyle and the Rules of Thumb", done => {
            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    // Use rules of thumb
                    let suggestionTypes = fixture.debugElement.queryAll(By.css("input[type='radio']"));
                    suggestionTypes[1].nativeElement.click();
                    fixture.detectChanges();

                    let expectedValues: { name: string, level: number, suggested?: number, hasEffectiveLevel?: boolean, isPrimary?: boolean }[] =
                        [
                            { name: "Argaiv", level: 200, suggested: 200 },
                            { name: "Atman", level: 10, suggested: 11, hasEffectiveLevel: true },
                            { name: "Berserker", level: 0 },
                            { name: "Bhaal", level: 0, suggested: 100 },
                            { name: "Bubos", level: 10, suggested: 9 },
                            { name: "Chawedo", level: 0 },
                            { name: "Chronos", level: 10, suggested: 10 },
                            { name: "Dogcog", level: 10, suggested: 8 },
                            { name: "Dora", level: 10, suggested: 11, hasEffectiveLevel: true },
                            { name: "Energon", level: 0 },
                            { name: "Fortuna", level: 10, suggested: 9, hasEffectiveLevel: true },
                            { name: "Fragsworth", level: 200, suggested: 100, hasEffectiveLevel: true },
                            { name: "Hecatoncheir", level: 0 },
                            { name: "Juggernaut", level: 50, suggested: 40, hasEffectiveLevel: true },
                            { name: "Kleptos", level: 0 },
                            { name: "Kumawakamaru", level: 10, suggested: 9, hasEffectiveLevel: true },
                            { name: "Libertas", level: 200, suggested: 186 },
                            { name: "Mammon", level: 200, suggested: 186 },
                            { name: "Mimzee", level: 200, suggested: 186, hasEffectiveLevel: true },
                            { name: "Morgulis", level: 40000, suggested: 40000 },
                            { name: "Nogardnit", level: 50, suggested: 66 },
                            { name: "Pluto", level: 0, suggested: 186 },
                            { name: "Revolc", level: 10, hasEffectiveLevel: true },
                            { name: "Siyalatas", level: 200, isPrimary: true },
                            { name: "Sniperino", level: 0 },
                            { name: "Vaagur", level: 10 },
                        ];

                    verify(expectedValues);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should display data based on idle playstyle and Available Souls without using souls from ascension", done => {
            let upload = getUpload();
            upload.playStyle = "idle";
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    // Don't use souls from ascension
                    let useSoulsFromAscension = fixture.debugElement.query(By.css("input[name='useSoulsFromAscension']"));
                    useSoulsFromAscension.nativeElement.click();
                    fixture.detectChanges();

                    let expectedValues: { name: string, level: number, suggested?: number, hasEffectiveLevel?: boolean }[] =
                        [
                            { name: "Argaiv", level: 200, suggested: 200 },
                            { name: "Atman", level: 10, suggested: 10, hasEffectiveLevel: true },
                            { name: "Berserker", level: 0 },
                            { name: "Bhaal", level: 0 },
                            { name: "Bubos", level: 10, suggested: 10 },
                            { name: "Chawedo", level: 0 },
                            { name: "Chronos", level: 10, suggested: 10 },
                            { name: "Dogcog", level: 10, suggested: 10 },
                            { name: "Dora", level: 10, suggested: 10, hasEffectiveLevel: true },
                            { name: "Energon", level: 0 },
                            { name: "Fortuna", level: 10, suggested: 10, hasEffectiveLevel: true },
                            { name: "Fragsworth", level: 200, hasEffectiveLevel: true },
                            { name: "Hecatoncheir", level: 0 },
                            { name: "Juggernaut", level: 50, hasEffectiveLevel: true },
                            { name: "Kleptos", level: 0 },
                            { name: "Kumawakamaru", level: 10, suggested: 10, hasEffectiveLevel: true },
                            { name: "Libertas", level: 200, suggested: 200 },
                            { name: "Mammon", level: 200, suggested: 200 },
                            { name: "Mimzee", level: 200, suggested: 200, hasEffectiveLevel: true },
                            { name: "Morgulis", level: 40000, suggested: 40000 },
                            { name: "Nogardnit", level: 50, suggested: 54 },
                            { name: "Pluto", level: 0 },
                            { name: "Revolc", level: 10, hasEffectiveLevel: true },
                            { name: "Siyalatas", level: 200, suggested: 200 },
                            { name: "Sniperino", level: 0 },
                            { name: "Vaagur", level: 10 },
                        ];

                    verify(expectedValues);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should display data based on idle playstyle and the Rules of Thumb", done => {
            let upload = getUpload();
            upload.playStyle = "idle";
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    // Use rules of thumb
                    let suggestionTypes = fixture.debugElement.queryAll(By.css("input[type='radio']"));
                    suggestionTypes[1].nativeElement.click();
                    fixture.detectChanges();

                    let expectedValues: { name: string, level: number, suggested?: number, hasEffectiveLevel?: boolean, isPrimary?: boolean }[] =
                        [
                            { name: "Argaiv", level: 200, suggested: 200 },
                            { name: "Atman", level: 10, suggested: 11, hasEffectiveLevel: true },
                            { name: "Berserker", level: 0 },
                            { name: "Bhaal", level: 0 },
                            { name: "Bubos", level: 10, suggested: 9 },
                            { name: "Chawedo", level: 0 },
                            { name: "Chronos", level: 10, suggested: 10 },
                            { name: "Dogcog", level: 10, suggested: 8 },
                            { name: "Dora", level: 10, suggested: 11, hasEffectiveLevel: true },
                            { name: "Energon", level: 0 },
                            { name: "Fortuna", level: 10, suggested: 9, hasEffectiveLevel: true },
                            { name: "Fragsworth", level: 200, hasEffectiveLevel: true },
                            { name: "Hecatoncheir", level: 0 },
                            { name: "Juggernaut", level: 50, hasEffectiveLevel: true },
                            { name: "Kleptos", level: 0 },
                            { name: "Kumawakamaru", level: 10, suggested: 9, hasEffectiveLevel: true },
                            { name: "Libertas", level: 200, suggested: 186 },
                            { name: "Mammon", level: 200, suggested: 186 },
                            { name: "Mimzee", level: 200, suggested: 186, hasEffectiveLevel: true },
                            { name: "Morgulis", level: 40000, suggested: 40000 },
                            { name: "Nogardnit", level: 50, suggested: 66 },
                            { name: "Pluto", level: 0 },
                            { name: "Revolc", level: 10, hasEffectiveLevel: true },
                            { name: "Siyalatas", level: 200, isPrimary: true },
                            { name: "Sniperino", level: 0 },
                            { name: "Vaagur", level: 10 },
                        ];

                    verify(expectedValues);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should display data based on active playstyle and Available Souls without using souls from ascension", done => {
            let upload = getUpload();
            upload.playStyle = "active";
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    // Don't use souls from ascension
                    let useSoulsFromAscension = fixture.debugElement.query(By.css("input[name='useSoulsFromAscension']"));
                    useSoulsFromAscension.nativeElement.click();
                    fixture.detectChanges();

                    let expectedValues: { name: string, level: number, suggested?: number, hasEffectiveLevel?: boolean }[] =
                        [
                            { name: "Argaiv", level: 200, suggested: 200 },
                            { name: "Atman", level: 10, suggested: 10, hasEffectiveLevel: true },
                            { name: "Berserker", level: 0 },
                            { name: "Bhaal", level: 0, suggested: 39 },
                            { name: "Bubos", level: 10, suggested: 10 },
                            { name: "Chawedo", level: 0 },
                            { name: "Chronos", level: 10, suggested: 10 },
                            { name: "Dogcog", level: 10, suggested: 10 },
                            { name: "Dora", level: 10, suggested: 10, hasEffectiveLevel: true },
                            { name: "Energon", level: 0 },
                            { name: "Fortuna", level: 10, suggested: 10, hasEffectiveLevel: true },
                            { name: "Fragsworth", level: 200, suggested: 200, hasEffectiveLevel: true },
                            { name: "Hecatoncheir", level: 0 },
                            { name: "Juggernaut", level: 50, suggested: 50, hasEffectiveLevel: true },
                            { name: "Kleptos", level: 0 },
                            { name: "Kumawakamaru", level: 10, suggested: 10, hasEffectiveLevel: true },
                            { name: "Libertas", level: 200 },
                            { name: "Mammon", level: 200, suggested: 200 },
                            { name: "Mimzee", level: 200, suggested: 200, hasEffectiveLevel: true },
                            { name: "Morgulis", level: 40000, suggested: 40000 },
                            { name: "Nogardnit", level: 50 },
                            { name: "Pluto", level: 0, suggested: 37 },
                            { name: "Revolc", level: 10, hasEffectiveLevel: true },
                            { name: "Siyalatas", level: 200 },
                            { name: "Sniperino", level: 0 },
                            { name: "Vaagur", level: 10 },
                        ];

                    verify(expectedValues);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should display data based on active playstyle and the Rules of Thumb", done => {
            let upload = getUpload();
            upload.playStyle = "active";
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    // Use rules of thumb
                    let suggestionTypes = fixture.debugElement.queryAll(By.css("input[type='radio']"));
                    suggestionTypes[1].nativeElement.click();
                    fixture.detectChanges();

                    let expectedValues: { name: string, level: number, suggested?: number, hasEffectiveLevel?: boolean, isPrimary?: boolean }[] =
                        [
                            { name: "Argaiv", level: 200, suggested: 200 },
                            { name: "Atman", level: 10, suggested: 11, hasEffectiveLevel: true },
                            { name: "Berserker", level: 0 },
                            { name: "Bhaal", level: 0, suggested: 200 },
                            { name: "Bubos", level: 10, suggested: 9 },
                            { name: "Chawedo", level: 0 },
                            { name: "Chronos", level: 10, suggested: 10 },
                            { name: "Dogcog", level: 10, suggested: 8 },
                            { name: "Dora", level: 10, suggested: 11, hasEffectiveLevel: true },
                            { name: "Energon", level: 0 },
                            { name: "Fortuna", level: 10, suggested: 9, hasEffectiveLevel: true },
                            { name: "Fragsworth", level: 200, isPrimary: true, hasEffectiveLevel: true },
                            { name: "Hecatoncheir", level: 0 },
                            { name: "Juggernaut", level: 50, suggested: 70, hasEffectiveLevel: true },
                            { name: "Kleptos", level: 0 },
                            { name: "Kumawakamaru", level: 10, suggested: 9, hasEffectiveLevel: true },
                            { name: "Libertas", level: 200 },
                            { name: "Mammon", level: 200, suggested: 186 },
                            { name: "Mimzee", level: 200, suggested: 186, hasEffectiveLevel: true },
                            { name: "Morgulis", level: 40000, suggested: 40000 },
                            { name: "Nogardnit", level: 50 },
                            { name: "Pluto", level: 0, suggested: 186 },
                            { name: "Revolc", level: 10, hasEffectiveLevel: true },
                            { name: "Siyalatas", level: 200 },
                            { name: "Sniperino", level: 0 },
                            { name: "Vaagur", level: 10 },
                        ];

                    verify(expectedValues);
                })
                .then(done)
                .catch(done.fail);
        });

        function verify(expectedValues: { name: string, level: number, suggested?: number, hasEffectiveLevel?: boolean, isPrimary?: boolean }[]): void {
            let exponentialPipe = TestBed.get(ExponentialPipe) as ExponentialPipe;

            let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
            expect(errorMessage).toBeNull("Error message found");

            let tables = fixture.debugElement.queryAll(By.css("table"));
            expect(tables.length).toEqual(3);

            let rows = tables[0].queryAll(By.css("tbody tr"));
            expect(rows.length).toEqual(expectedValues.length);

            for (let i = 0; i < rows.length; i++) {
                let cells = rows[i].children;
                let expected = expectedValues[i];

                let expectedName = expected.name + ":";
                expect(getNormalizedTextContent(cells[0])).toEqual(expectedName, "Unexpected ancient name");

                let expectedCurrentLevel = exponentialPipe.transform(expected.level);
                if (expected.hasEffectiveLevel) {
                    expectedCurrentLevel += " (*)";
                }
                expect(getNormalizedTextContent(cells[1])).toEqual(expectedCurrentLevel, `Unexpected current level for ${expected.name}`);

                let expectedSuggestedLevel = expected.isPrimary
                    ? "N/A (*)"
                    : expected.suggested === undefined
                        ? "-"
                        : exponentialPipe.transform(expected.suggested);
                expect(getNormalizedTextContent(cells[2])).toEqual(expectedSuggestedLevel, `Unexpected suggested level for ${expected.name}`);

                let expectedDifference = expected.isPrimary || expected.suggested === undefined
                    ? "-"
                    : exponentialPipe.transform(expected.suggested - expected.level);
                expect(getNormalizedTextContent(cells[3])).toEqual(expectedDifference, `Unexpected difference in levels for ${expected.name}`);
            }
        }
    });

    describe("Outsider Levels", () => {
        it("should display data", async(() => {
            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let expectedValues: { name: string, level: number, suggested: number }[] =
                        [
                            { name: "Xyliqil", level: 0, suggested: 0 },
                            { name: "Chor'gorloth", level: 0, suggested: 10 },
                            { name: "Phandoryss", level: 0, suggested: 74 },
                            { name: "Ponyboy", level: 0, suggested: 19 },
                            { name: "Borb", level: 0, suggested: 5 },
                            { name: "Rhageist", level: 0, suggested: 6 },
                            { name: "K'Ariqua", level: 0, suggested: 6 },
                            { name: "Orphalas", level: 0, suggested: 6 },
                            { name: "Sen-Akhan", level: 0, suggested: 6 },
                        ];

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    let tables = fixture.debugElement.queryAll(By.css("table"));
                    expect(tables.length).toEqual(3);

                    let rows = tables[1].queryAll(By.css("tbody tr"));
                    expect(rows.length).toEqual(expectedValues.length);

                    for (let i = 0; i < rows.length; i++) {
                        let expected = expectedValues[i];

                        let cells = rows[i].children;
                        expect(cells.length).toEqual(3);

                        expect(getNormalizedTextContent(cells[0])).toEqual(expected.name + ":");
                        expect(getNormalizedTextContent(cells[1])).toEqual(expected.level.toString());
                        expect(getNormalizedTextContent(cells[2])).toEqual(expected.suggested.toString());
                    }

                    let footer = tables[1].queryAll(By.css("tfoot tr"));
                    expect(footer.length).toEqual(1);

                    let footerCells = footer[0].children;
                    expect(footerCells.length).toEqual(3);
                    expect(getNormalizedTextContent(footerCells[2])).toEqual("80");
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

                    let expectedValues: { name: string, stat: string, value: number, type: "exponential" | "percent" }[] =
                        [
                            { name: "Hero Souls Spent", stat: "heroSoulsSpent", value: 97623, type: "exponential" },
                            { name: "Hero Souls Sacrificed", stat: "heroSoulsSacrificed", value: 5.224e+99, type: "exponential" },
                            { name: "Ancient Souls Earned", stat: "totalAncientSouls", value: 498, type: "exponential" },
                            { name: "Transcendent Power", stat: "transcendentPower", value: 0.05192, type: "percent" },
                            { name: "Titan Damage", stat: "titanDamage", value: 5.224e+99, type: "exponential" },
                            { name: "Highest Zone", stat: "highestZoneThisTranscension", value: 695, type: "exponential" },
                            { name: "Highest Zone (Lifetime)", stat: "highestZoneLifetime", value: 23274, type: "exponential" },
                            { name: "Ascensions", stat: "ascensionsThisTranscension", value: 4, type: "exponential" },
                            { name: "Ascensions (Lifetime)", stat: "ascensionsLifetime", value: 3080, type: "exponential" },
                            { name: "Rubies", stat: "rubies", value: 260, type: "exponential" },
                            { name: "Autoclickers", stat: "autoclickers", value: 9, type: "exponential" },
                        ];

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    let tables = fixture.debugElement.queryAll(By.css("table"));
                    expect(tables.length).toEqual(3);

                    let rows = tables[2].queryAll(By.css("tbody tr"));
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
                                    expectedFormattedValue = exponentialPipe.transform(+expectedValue);
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

    describe("Optimal Ascension Zone", () => {
        it("should show data after clicking the calculate button", done => {
            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    // Not showing the table yet
                    let tables = fixture.debugElement.queryAll(By.css("table"));
                    expect(tables.length).toEqual(3);

                    let buttons = fixture.debugElement.queryAll(By.css("button"));
                    let calculateButton: DebugElement;
                    for (let i = 0; i < buttons.length; i++) {
                        if (getNormalizedTextContent(buttons[i]) === "Calculate") {
                            calculateButton = buttons[i];
                        }
                    }

                    expect(calculateButton).toBeDefined("Could not find the 'Calculate' button");

                    calculateButton.nativeElement.click();
                    fixture.detectChanges();

                    // The new table exists
                    tables = fixture.debugElement.queryAll(By.css("table"));
                    expect(tables.length).toEqual(4);

                    // Don't make specific assertions about the rows since there is some level of randomness in the calculation.
                    let rows = tables[3].queryAll(By.css("tbody tr"));
                    expect(rows.length).toBeGreaterThan(0);
                })
                .then(done)
                .catch(done.fail);
        });
    });

    function getUpload(): IUpload {
        return {
            id: 1234,
            user: {
                id: "d2547ac0-2c1f-4855-a6a1-a3dd71e9d8dc",
                name: "Omnes",
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
