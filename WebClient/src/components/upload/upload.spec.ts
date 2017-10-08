import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { ActivatedRoute, Router, Params } from "@angular/router";
import { FormsModule } from "@angular/forms";
import { NO_ERRORS_SCHEMA, DebugElement, ChangeDetectorRef } from "@angular/core";
import { By } from "@angular/platform-browser";
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import { DatePipe, PercentPipe } from "@angular/common";
import { AppInsightsService } from "@markpieszak/ng-application-insights";

import { UploadComponent } from "./upload";
import { ExponentialPipe } from "../../pipes/exponentialPipe";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { UploadService, IUpload } from "../../services/uploadService/uploadService";
import { SettingsService, IUserSettings } from "../../services/settingsService/settingsService";

describe("UploadComponent", () => {
    let component: UploadComponent;
    let fixture: ComponentFixture<UploadComponent>;
    let routeParams: BehaviorSubject<Params>;
    let appInsights: AppInsightsService;

    let uploadServiceGetResolve: (upload: IUpload) => Promise<void>;
    let uploadServiceGetReject: () => Promise<void>;

    let uploadServiceDeleteResolve: () => Promise<void>;
    let uploadServiceDeleteReject: () => Promise<void>;

    let settings: IUserSettings = {
        areUploadsPublic: true,
        playStyle: "hybrid",
        useScientificNotation: true,
        scientificNotationThreshold: 1000000,
        useEffectiveLevelForSuggestions: false,
        useLogarithmicGraphScale: true,
        logarithmicGraphScaleThreshold: 1000000,
        hybridRatio: 1,
        theme: "light",
    };

    let settingsSubject = new BehaviorSubject(settings);

    beforeEach(async(() => {
        let userInfo = new BehaviorSubject({ isLoggedIn: false });
        let authenticationService = { userInfo: () => userInfo };
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
            expect(userNameValueElement.classes["text-muted"]).toEqual(true);
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
                    expect(userNameValueElement.classes["text-muted"]).toEqual(true);
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
                    let userNameValueElement = userNameItem.query(By.css("span"));
                    expect(userNameValueElement.classes["text-muted"]).toEqual(false);
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
        it("should display when there is upload content", async(() => {
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

        it("should not display when there is no upload content", async(() => {
            let upload = getUpload();
            delete upload.uploadContent;
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

                    expect(found).toEqual(false, "Unexpectedly found the 'View Save Data' button");
                });
        }));
    });

    describe("Delete button", () => {
        it("should display when there is upload content", async(() => {
            let upload = getUpload();
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

        it("should delete the upload when clicked", async(() => {
            let router = TestBed.get(Router) as Router;
            spyOn(router, "navigate");

            let upload = getUpload();
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
                    uploadServiceDeleteResolve()
                        .then(() => {
                            fixture.detectChanges();

                            expect(router.navigate).toHaveBeenCalledWith(["/dashboard"]);

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

                    let buttons = fixture.debugElement.queryAll(By.css(".col-md-6.pull-right button"));
                    let deleteButton: DebugElement;
                    for (let i = 0; i < buttons.length; i++) {
                        if (getNormalizedTextContent(buttons[i]) === "Delete") {
                            deleteButton = buttons[i];
                        }
                    }

                    expect(deleteButton).toBeDefined("Could not find the 'Delete' button");

                    deleteButton.nativeElement.click();
                    uploadServiceDeleteReject()
                        .then(() => {
                            fixture.detectChanges();

                            expect(router.navigate).not.toHaveBeenCalled();

                            errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                            expect(errorMessage).not.toBeNull("Error message not found");
                        });
                });
        }));

        it("should not display when there is no upload content", async(() => {
            let upload = getUpload();
            delete upload.uploadContent;
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

                    let useSoulsFromAscension = fixture.debugElement.query(By.css("input[type='checkbox']"));
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

                    let useSoulsFromAscension = fixture.debugElement.query(By.css("input[type='checkbox']"));
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

                    let useSoulsFromAscension = fixture.debugElement.query(By.css("input[type='checkbox']"));
                    expect(useSoulsFromAscension).not.toBeNull("Couldn't find the 'Use souls from ascension' checkbox");
                });
        }));
    });

    describe("Ancient Levels", () => {
        it("should display data based on hybrid playstyle and Available Souls using souls from ascension", async(() => {
            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let expectedValues: { name: string, suggested?: number, hasEffectiveLevel?: boolean }[] =
                        [
                            { name: "Argaiv", suggested: 5.306472455726447e+39 },
                            { name: "Atman", suggested: 255, hasEffectiveLevel: true },
                            { name: "Berserker" },
                            { name: "Bhaal", suggested: 5.306472455726447e+39, hasEffectiveLevel: true },
                            { name: "Bubos", suggested: 251 },
                            { name: "Chawedo" },
                            { name: "Chronos", suggested: 246 },
                            { name: "Dogcog", suggested: 257 },
                            { name: "Dora", suggested: 255 },
                            { name: "Energon" },
                            { name: "Fortuna", suggested: 255, hasEffectiveLevel: true },
                            { name: "Fragsworth", suggested: 5.306472455726447e+39, hasEffectiveLevel: true },
                            { name: "Hecatoncheir" },
                            { name: "Juggernaut", suggested: 6.023441980154034e+31 },
                            { name: "Kleptos" },
                            { name: "Kumawakamaru", suggested: 258, hasEffectiveLevel: true },
                            { name: "Libertas", suggested: 4.91379349400269e+39 },
                            { name: "Mammon", suggested: 4.91379349400269e+39 },
                            { name: "Mimzee", suggested: 4.91379349400269e+39 },
                            { name: "Morgulis", suggested: 2.8158649923383468e+79 },
                            { name: "Nogardnit", suggested: 5.66413402071902e+31 },
                            { name: "Revolc" },
                            { name: "Siyalatas", suggested: 5.306472455726447e+39 },
                            { name: "Sniperino", hasEffectiveLevel: true },
                            { name: "Solomon", suggested: 1.1873757490168236e+32, hasEffectiveLevel: true },
                            { name: "Vaagur" },
                        ];

                    verify(upload, expectedValues);
                });
        }));

        it("should display data based on hybrid playstyle and Available Souls without using souls from ascension", async(() => {
            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    // Don't use souls from ascension
                    let useSoulsFromAscension = fixture.debugElement.query(By.css("input[type='checkbox']"));
                    useSoulsFromAscension.nativeElement.click();
                    fixture.detectChanges();

                    let expectedValues: { name: string, suggested?: number, hasEffectiveLevel?: boolean }[] =
                        [
                            { name: "Argaiv", suggested: 2.2846000000530268e+39 },
                            { name: "Atman", suggested: 252, hasEffectiveLevel: true },
                            { name: "Berserker" },
                            { name: "Bhaal", suggested: 2.2846000000530268e+39, hasEffectiveLevel: true },
                            { name: "Bubos", suggested: 248 },
                            { name: "Chawedo" },
                            { name: "Chronos", suggested: 244 },
                            { name: "Dogcog", suggested: 254 },
                            { name: "Dora", suggested: 252 },
                            { name: "Energon" },
                            { name: "Fortuna", suggested: 252, hasEffectiveLevel: true },
                            { name: "Fragsworth", suggested: 2.2846000000530268e+39, hasEffectiveLevel: true },
                            { name: "Hecatoncheir" },
                            { name: "Juggernaut", suggested: 3.0693680095411105e+31 },
                            { name: "Kleptos" },
                            { name: "Kumawakamaru", suggested: 254, hasEffectiveLevel: true },
                            { name: "Libertas", suggested: 2.1155410000491033e+39 },
                            { name: "Mammon", suggested: 2.1155410000491033e+39 },
                            { name: "Mimzee", suggested: 2.1155410000491033e+39 },
                            { name: "Morgulis", suggested: 5.2194381e+78 },
                            { name: "Nogardnit", suggested: 2.8862730089719703e+31 },
                            { name: "Revolc" },
                            { name: "Siyalatas", suggested: 2.2846000000530268e+39 },
                            { name: "Sniperino", hasEffectiveLevel: true },
                            { name: "Solomon", suggested: 6.050525017318302e+31, hasEffectiveLevel: true },
                            { name: "Vaagur" },
                        ];

                    verify(upload, expectedValues);
                });
        }));

        it("should display data based on hybrid playstyle and the Rules of Thumb", async(() => {
            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    // Use rules of thumb
                    let suggestionTypes = fixture.debugElement.queryAll(By.css("input[type='radio']"));
                    suggestionTypes[1].nativeElement.click();
                    fixture.detectChanges();

                    let expectedValues: { name: string, suggested?: number, hasEffectiveLevel?: boolean, isPrimary?: boolean }[] =
                        [
                            { name: "Argaiv", suggested: 2.2846000000530268e+39 },
                            { name: "Atman", suggested: 253, hasEffectiveLevel: true },
                            { name: "Berserker" },
                            { name: "Bhaal", suggested: 2.2846000000530268e+39, hasEffectiveLevel: true },
                            { name: "Bubos", suggested: 248 },
                            { name: "Chawedo" },
                            { name: "Chronos", suggested: 244 },
                            { name: "Dogcog", suggested: 254 },
                            { name: "Dora", suggested: 253 },
                            { name: "Energon" },
                            { name: "Fortuna", suggested: 253, hasEffectiveLevel: true },
                            { name: "Fragsworth", suggested: 2.2846000000530268e+39, hasEffectiveLevel: true },
                            { name: "Hecatoncheir" },
                            { name: "Juggernaut", suggested: 3.069362183115342e+31 },
                            { name: "Kleptos" },
                            { name: "Kumawakamaru", suggested: 255, hasEffectiveLevel: true },
                            { name: "Libertas", suggested: 2.1155396000491028e+39 },
                            { name: "Mammon", suggested: 2.1155396000491028e+39 },
                            { name: "Mimzee", suggested: 2.1155396000491028e+39 },
                            { name: "Morgulis", suggested: 5.21939716024229e+78 },
                            { name: "Nogardnit", suggested: 2.886269813932436e+31 },
                            { name: "Revolc" },
                            { name: "Siyalatas", isPrimary: true },
                            { name: "Sniperino", hasEffectiveLevel: true },
                            { name: "Solomon", suggested: 6.050504401284679e+31, hasEffectiveLevel: true },
                            { name: "Vaagur" },
                        ];

                    verify(upload, expectedValues);
                });
        }));

        it("should display data based on idle playstyle and Available Souls without using souls from ascension", async(() => {
            let upload = getUpload();
            upload.playStyle = "idle";
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    // Don't use souls from ascension
                    let useSoulsFromAscension = fixture.debugElement.query(By.css("input[type='checkbox']"));
                    useSoulsFromAscension.nativeElement.click();
                    fixture.detectChanges();

                    let expectedValues: { name: string, suggested?: number, hasEffectiveLevel?: boolean }[] =
                        [
                            { name: "Argaiv", suggested: 2.2846000000530268e+39 },
                            { name: "Atman", suggested: 252, hasEffectiveLevel: true },
                            { name: "Berserker" },
                            { name: "Bhaal", hasEffectiveLevel: true },
                            { name: "Bubos", suggested: 248 },
                            { name: "Chawedo" },
                            { name: "Chronos", suggested: 244 },
                            { name: "Dogcog", suggested: 254 },
                            { name: "Dora", suggested: 252 },
                            { name: "Energon" },
                            { name: "Fortuna", suggested: 252, hasEffectiveLevel: true },
                            { name: "Fragsworth", hasEffectiveLevel: true },
                            { name: "Hecatoncheir" },
                            { name: "Juggernaut" },
                            { name: "Kleptos" },
                            { name: "Kumawakamaru", suggested: 254, hasEffectiveLevel: true },
                            { name: "Libertas", suggested: 2.1155410000491033e+39 },
                            { name: "Mammon", suggested: 2.1155410000491033e+39 },
                            { name: "Mimzee", suggested: 2.1155410000491033e+39 },
                            { name: "Morgulis", suggested: 5.2194381e+78 },
                            { name: "Nogardnit", suggested: 2.8862730089719703e+31 },
                            { name: "Revolc" },
                            { name: "Siyalatas", suggested: 2.2846000000530268e+39 },
                            { name: "Sniperino", hasEffectiveLevel: true },
                            { name: "Solomon", suggested: 6.050525017318302e+31, hasEffectiveLevel: true },
                            { name: "Vaagur" },
                        ];

                    verify(upload, expectedValues);
                });
        }));

        it("should display data based on idle playstyle and the Rules of Thumb", async(() => {
            let upload = getUpload();
            upload.playStyle = "idle";
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    // Use rules of thumb
                    let suggestionTypes = fixture.debugElement.queryAll(By.css("input[type='radio']"));
                    suggestionTypes[1].nativeElement.click();
                    fixture.detectChanges();

                    let expectedValues: { name: string, suggested?: number, hasEffectiveLevel?: boolean, isPrimary?: boolean }[] =
                        [
                            { name: "Argaiv", suggested: 2.2846000000530268e+39 },
                            { name: "Atman", suggested: 253, hasEffectiveLevel: true },
                            { name: "Berserker" },
                            { name: "Bhaal", hasEffectiveLevel: true },
                            { name: "Bubos", suggested: 248 },
                            { name: "Chawedo" },
                            { name: "Chronos", suggested: 244 },
                            { name: "Dogcog", suggested: 254 },
                            { name: "Dora", suggested: 253 },
                            { name: "Energon" },
                            { name: "Fortuna", suggested: 253, hasEffectiveLevel: true },
                            { name: "Fragsworth", hasEffectiveLevel: true },
                            { name: "Hecatoncheir" },
                            { name: "Juggernaut" },
                            { name: "Kleptos" },
                            { name: "Kumawakamaru", suggested: 255, hasEffectiveLevel: true },
                            { name: "Libertas", suggested: 2.1155396000491028e+39 },
                            { name: "Mammon", suggested: 2.1155396000491028e+39 },
                            { name: "Mimzee", suggested: 2.1155396000491028e+39 },
                            { name: "Morgulis", suggested: 5.21939716024229e+78 },
                            { name: "Nogardnit", suggested: 2.886269813932425e+31 },
                            { name: "Revolc" },
                            { name: "Siyalatas", isPrimary: true },
                            { name: "Sniperino", hasEffectiveLevel: true },
                            { name: "Solomon", suggested: 6.05050440128466e+31, hasEffectiveLevel: true },
                            { name: "Vaagur" },
                        ];

                    verify(upload, expectedValues);
                });
        }));

        it("should display data based on active playstyle and Available Souls without using souls from ascension", async(() => {
            let upload = getUpload();
            upload.playStyle = "active";
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    // Don't use souls from ascension
                    let useSoulsFromAscension = fixture.debugElement.query(By.css("input[type='checkbox']"));
                    useSoulsFromAscension.nativeElement.click();
                    fixture.detectChanges();

                    let expectedValues: { name: string, suggested?: number, hasEffectiveLevel?: boolean }[] =
                        [
                            { name: "Argaiv", suggested: 2.2846000000530268e+39 },
                            { name: "Atman", suggested: 252, hasEffectiveLevel: true },
                            { name: "Berserker" },
                            { name: "Bhaal", suggested: 2.2846000000530268e+39, hasEffectiveLevel: true },
                            { name: "Bubos", suggested: 248 },
                            { name: "Chawedo" },
                            { name: "Chronos", suggested: 244 },
                            { name: "Dogcog", suggested: 254 },
                            { name: "Dora", suggested: 252 },
                            { name: "Energon" },
                            { name: "Fortuna", suggested: 252, hasEffectiveLevel: true },
                            { name: "Fragsworth", suggested: 2.2846000000530268e+39, hasEffectiveLevel: true },
                            { name: "Hecatoncheir" },
                            { name: "Juggernaut", suggested: 3.0693680095411105e+31 },
                            { name: "Kleptos" },
                            { name: "Kumawakamaru", suggested: 254, hasEffectiveLevel: true },
                            { name: "Libertas" },
                            { name: "Mammon", suggested: 2.1155410000491033e+39 },
                            { name: "Mimzee", suggested: 2.1155410000491033e+39 },
                            { name: "Morgulis", suggested: 5.2194381e+78 },
                            { name: "Nogardnit" },
                            { name: "Revolc" },
                            { name: "Siyalatas" },
                            { name: "Sniperino", hasEffectiveLevel: true },
                            { name: "Solomon", suggested: 6.050525017318302e+31, hasEffectiveLevel: true },
                            { name: "Vaagur" },
                        ];

                    verify(upload, expectedValues);
                });
        }));

        it("should display data based on active playstyle and the Rules of Thumb", async(() => {
            let upload = getUpload();
            upload.playStyle = "active";
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    // Use rules of thumb
                    let suggestionTypes = fixture.debugElement.queryAll(By.css("input[type='radio']"));
                    suggestionTypes[1].nativeElement.click();
                    fixture.detectChanges();

                    let expectedValues: { name: string, suggested?: number, hasEffectiveLevel?: boolean, isPrimary?: boolean }[] =
                        [
                            { name: "Argaiv", suggested: 2.2846000000530268e+39 },
                            { name: "Atman", suggested: 253, hasEffectiveLevel: true },
                            { name: "Berserker" },
                            { name: "Bhaal", suggested: 2.2846000000530268e+39, hasEffectiveLevel: true },
                            { name: "Bubos", suggested: 248 },
                            { name: "Chawedo" },
                            { name: "Chronos", suggested: 244 },
                            { name: "Dogcog", suggested: 254 },
                            { name: "Dora", suggested: 253 },
                            { name: "Energon" },
                            { name: "Fortuna", suggested: 253, hasEffectiveLevel: true },
                            { name: "Fragsworth", isPrimary: true, hasEffectiveLevel: true },
                            { name: "Hecatoncheir" },
                            { name: "Juggernaut", suggested: 3.0693621831153293e+31 },
                            { name: "Kleptos" },
                            { name: "Kumawakamaru", suggested: 255, hasEffectiveLevel: true },
                            { name: "Libertas" },
                            { name: "Mammon", suggested: 2.1155396000491028e+39 },
                            { name: "Mimzee", suggested: 2.1155396000491028e+39 },
                            { name: "Morgulis", suggested: 5.21939716024229e+78 },
                            { name: "Nogardnit" },
                            { name: "Revolc" },
                            { name: "Siyalatas" },
                            { name: "Sniperino", hasEffectiveLevel: true },
                            { name: "Solomon", suggested: 6.05050440128466e+31, hasEffectiveLevel: true },
                            { name: "Vaagur" },
                        ];

                    verify(upload, expectedValues);
                });
        }));

        function verify(upload: IUpload, expectedValues: { name: string, suggested?: number, hasEffectiveLevel?: boolean, isPrimary?: boolean }[]): void {
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
                expect(getNormalizedTextContent(cells[0])).toEqual(expectedName);

                let expectedCurrentLevel = exponentialPipe.transform(+upload.stats["ancient" + expected.name]);
                if (expected.hasEffectiveLevel) {
                    expectedCurrentLevel += " (*)";
                }
                expect(getNormalizedTextContent(cells[1])).toEqual(expectedCurrentLevel);

                let expectedSuggestedLevel = expected.isPrimary
                    ? "N/A (*)"
                    : expected.suggested === undefined
                        ? "-"
                        : exponentialPipe.transform(expected.suggested);
                expect(getNormalizedTextContent(cells[2])).toEqual(expectedSuggestedLevel);

                let expectedDifference = expected.isPrimary || expected.suggested === undefined
                    ? "-"
                    : exponentialPipe.transform(expected.suggested - +upload.stats["ancient" + expected.name]);
                expect(getNormalizedTextContent(cells[3])).toEqual(expectedDifference);
            }
        }
    });

    describe("Outsider Levels", () => {
        it("should display data", async(() => {
            let upload = getUpload();
            uploadServiceGetResolve(upload)
                .then(() => {
                    fixture.detectChanges();

                    let expectedValues: { name: string, suggested?: number }[] =
                        [
                            { name: "Xyliqil", suggested: 11 },
                            { name: "Chor'gorloth", suggested: 10 },
                            { name: "Phandoryss", suggested: 20 },
                            { name: "Borb", suggested: 113 },
                            { name: "Ponyboy", suggested: 36 },
                        ];

                    let errorMessage = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(errorMessage).toBeNull("Error message found");

                    let tables = fixture.debugElement.queryAll(By.css("table"));
                    expect(tables.length).toEqual(3);

                    let rows = tables[1].queryAll(By.css("tbody tr"));
                    expect(rows.length).toEqual(expectedValues.length);

                    for (let i = 0; i < rows.length; i++) {
                        let cells = rows[i].children;
                        let expected = expectedValues[i];

                        let expectedName = expected.name + ":";
                        expect(getNormalizedTextContent(cells[0])).toEqual(expectedName);

                        let expectedCurrentLevel = upload.stats["outsider" + expected.name.replace("'", "")].toString();
                        expect(getNormalizedTextContent(cells[1])).toEqual(expectedCurrentLevel);

                        let expectedSuggestedLevel = expected.suggested.toString();
                        expect(getNormalizedTextContent(cells[2])).toEqual(expectedSuggestedLevel);
                    }
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
                            { name: "Hero Souls Spent", stat: "heroSoulsSpent", value: 2.260e+79, type: "exponential" },
                            { name: "Hero Souls Sacrificed", stat: "heroSoulsSacrificed", value: 2.641e+76, type: "exponential" },
                            { name: "Ancient Souls Earned", stat: "totalAncientSouls", value: 382, type: "exponential" },
                            { name: "Transcendent Power", stat: "transcendentPower", value: 0.03836, type: "percent" },
                            { name: "Max Transcendent Primal Reward", stat: "maxTranscendentPrimalReward", value: 1.651e+76, type: "exponential" },
                            { name: "Transcendent Primal Cap Zone", stat: "bossLevelToTranscendentPrimalCap", value: 13425, type: "exponential" },
                            { name: "Titan Damage", stat: "titanDamage", value: 2.262e+79, type: "exponential" },
                            { name: "Highest Zone", stat: "highestZoneThisTranscension", value: 19168, type: "exponential" },
                            { name: "Highest Zone (Lifetime)", stat: "highestZoneLifetime", value: 19168, type: "exponential" },
                            { name: "Ascensions", stat: "ascensionsThisTranscension", value: 11, type: "exponential" },
                            { name: "Ascensions (Lifetime)", stat: "ascensionsLifetime", value: 3000, type: "exponential" },
                            { name: "Rubies", stat: "rubies", value: 112, type: "exponential" },
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

                        let expectedValue = upload.stats[expected.stat];
                        let expectedFormattedValue: string;
                        switch (expected.type) {
                            case "exponential":
                                {
                                    expectedFormattedValue = exponentialPipe.transform(+expectedValue);
                                    break;
                                }
                            case "percent":
                                {
                                    expectedFormattedValue = percentPipe.transform(expectedValue);
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
        // Based on the real upload 355734
        return {
            id: 355734,
            user: {
                id: "d2547ac0-2c1f-4855-a6a1-a3dd71e9d8dc",
                name: "Omnes",
            },
            timeSubmitted: "2017-06-17T16:40:22",
            uploadContent: "someUploadContent",
            playStyle: "hybrid",
            stats: {
                ancientSolomon: "6.050525017318302e+31",
                itemSolomon: "2.9129311827723887",
                ancientLibertas: "2.1155410000491033e+39",
                ancientSiyalatas: "2.2846000000530268e+39",
                ancientMammon: "2.1155410000491033e+39",
                ancientMimzee: "2.1155410000491033e+39",
                ancientDogcog: "254",
                ancientFortuna: "252",
                itemFortuna: "2.9129311827723887",
                ancientAtman: "252",
                itemAtman: "6.513408575675979",
                ancientDora: "252",
                ancientBhaal: "2.2846000000530268e+39",
                itemBhaal: "7",
                ancientMorgulis: "5.2194381e+78",
                ancientChronos: "244",
                ancientBubos: "248",
                ancientFragsworth: "2.2846000000530268e+39",
                itemFragsworth: "8",
                ancientVaagur: "240",
                ancientKumawakamaru: "253",
                itemKumawakamaru: "10.55700562193223",
                ancientChawedo: "240",
                ancientHecatoncheir: "240",
                ancientBerserker: "240",
                ancientSniperino: "240",
                itemSniperino: "7",
                ancientKleptos: "240",
                ancientEnergon: "240",
                ancientArgaiv: "2.2846000000530268e+39",
                ancientJuggernaut: "3.0693680095411105e+31",
                ancientRevolc: "240",
                ancientNogardnit: "2.8862730089719703e+31",
                outsiderXyliqil: "11",
                outsiderChorgorloth: "10",
                outsiderPhandoryss: "20",
                outsiderBorb: "115",
                outsiderPonyboy: "36",
                ascensionsLifetime: "3000",
                ascensionsThisTranscension: "11",
                heroSoulsSacrificed: "2.641131449478056e+76",
                heroSoulsSpent: "2.259769934447405e+79",
                highestZoneLifetime: "19168",
                highestZoneThisTranscension: "19168",
                rubies: "112",
                titanDamage: "2.2615094954691136e+79",
                totalAncientSouls: "382",
                transcendentPower: "0.038364995397561684",
                maxTranscendentPrimalReward: "1.6507071559237853e+76",
                bossLevelToTranscendentPrimalCap: "13425",
                heroSouls: "1.739561034051967e+76",
                pendingSouls: "1e+80",
            },
        };
    }

    function getNormalizedTextContent(element: DebugElement): string {
        let nativeElement: HTMLElement = element.nativeElement;
        return nativeElement.textContent.trim().replace(/\s\s+/g, " ");
    }
});
