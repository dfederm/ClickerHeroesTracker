import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { By } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { NO_ERRORS_SCHEMA, DebugElement } from "@angular/core";
import { MockBackend, MockConnection } from "@angular/http/testing";
import { BaseRequestOptions, ConnectionBackend, Http, RequestOptions, Response, ResponseOptions, Headers } from "@angular/http";
import { Router } from "@angular/router";
import { Observable } from "rxjs/Observable";
import { BehaviorSubject } from "rxjs/BehaviorSubject";

import { UploadDialogComponent } from "./uploadDialog";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";

declare global
{
    // tslint:disable-next-line:interface-name - We don't own this interface name, just extending it
    interface Window
    {
        appInsights: Microsoft.ApplicationInsights.IAppInsights;
    }
}

describe("UploadDialogComponent", () =>
{
    let component: UploadDialogComponent;
    let fixture: ComponentFixture<UploadDialogComponent>;
    let authHeaders: Headers;
    let isLoggedIn: BehaviorSubject<boolean>;

    beforeEach(async(() =>
    {
        authHeaders = new Headers();
        isLoggedIn = new BehaviorSubject(false);
        let authenticationService =
        {
            isLoggedIn: (): Observable<boolean> => isLoggedIn,
            getAuthHeaders: (): Promise<Headers> => Promise.resolve(authHeaders),
        };
        let activeModal = { close: (): void => void 0 };
        let router = { navigate: (): void => void 0 };

        TestBed.configureTestingModule(
        {
            imports: [ FormsModule ],
            declarations: [ UploadDialogComponent ],
            providers:
            [
                { provide: ConnectionBackend, useClass: MockBackend },
                { provide: RequestOptions, useClass: BaseRequestOptions },
                Http,
                { provide: AuthenticationService, useValue: authenticationService },
                { provide: NgbActiveModal, useValue: activeModal },
                { provide: Router, useValue: router },
            ],
            schemas: [ NO_ERRORS_SCHEMA ],
        })
        .compileComponents()
        .then(() =>
        {
            fixture = TestBed.createComponent(UploadDialogComponent);
            component = fixture.componentInstance;
        });
    }));

    it("should display the modal header", () =>
    {
        fixture.detectChanges();

        let header = fixture.debugElement.query(By.css(".modal-header"));
        expect(header).not.toBeNull();

        let title = header.query(By.css(".modal-title"));
        expect(title).not.toBeNull();
        expect(title.nativeElement.textContent).toEqual("Upload your save");
    });

    describe("upload", () =>
    {
        let encodedSaveData: DebugElement;
        let playStyles: DebugElement[];
        let addToProgress: DebugElement;
        let warningMessage: DebugElement;
        let button: DebugElement;
        let lastConnection: MockConnection;
        let backend: MockBackend;

        beforeEach(async(() =>
        {
            // Wait for stability since ngModel is async
            fixture.detectChanges();
            fixture.whenStable().then(() =>
            {
                lastConnection = null;
                backend = TestBed.get(ConnectionBackend) as MockBackend;
                backend.connections.subscribe((connection: MockConnection) => lastConnection = connection);
            });
        }));

        afterEach(() =>
        {
            backend.verifyNoPendingRequests();
        });

        it("should display the form elements with 'add to progress' when the user is logged in", async(() =>
        {
            setIsLoggedIn(true)
                .then(() => {
                    expect(encodedSaveData).not.toBeNull();
                    expect(playStyles.length).toEqual(3);
                    expect(addToProgress).not.toBeNull();
                    expect(warningMessage).toBeNull();
                    expect(button).not.toBeNull();
                });
        }));

        it("should display the form elements without 'add to progress' when the user is not logged in", async(() =>
        {
            setIsLoggedIn(false)
                .then(() => {
                    expect(encodedSaveData).not.toBeNull();
                    expect(playStyles.length).toEqual(3);
                    expect(addToProgress).toBeNull();
                    expect(warningMessage).not.toBeNull();
                    expect(button).not.toBeNull();
                });
        }));

        it("should show an error with empty save data", async(() =>
        {
            setIsLoggedIn(false)
                .then(() => {
                    button.nativeElement.click();

                    expect(component.errorMessage).toBeTruthy();
                });
        }));

        it("should upload correct save data when user is not logged in", async(() =>
        {
            setIsLoggedIn(false)
                .then(() => {
                    let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
                    spyOn(authenticationService, "getAuthHeaders");

                    let router = TestBed.get(Router) as Router;
                    spyOn(router, "navigate");

                    setInputValue(encodedSaveData, "someEncodedSaveData");
                    playStyles[1].nativeElement.click();

                    button.nativeElement.click();

                    // Wait for stability from the headers promise
                    fixture.detectChanges();
                    fixture.whenStable().then(() =>
                    {
                        // Doesn't call it when not logged in
                        expect(authenticationService.getAuthHeaders).not.toHaveBeenCalled();

                        expect(lastConnection).not.toBeNull();
                        expect(lastConnection.request.url).toContain("/api/uploads");
                        expect(lastConnection.request.text()).toEqual("encodedSaveData=someEncodedSaveData&addToProgress=false&playStyle=Hybrid");
                        lastConnection.mockRespond(new Response(new ResponseOptions({ body: "123" })));

                        // Wait for stability from the http call
                        fixture.detectChanges();
                        fixture.whenStable().then(() =>
                        {
                            expect(component.errorMessage).toBeFalsy();
                            expect(router.navigate).toHaveBeenCalledWith(["/calculator/view", 123]);
                        });
                    });
                });
        }));

        it("should upload correct save data when user is logged in", async(() =>
        {
            setIsLoggedIn(true)
                .then(() => {
                    let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
                    spyOn(authenticationService, "getAuthHeaders").and.callThrough();

                    let router = TestBed.get(Router) as Router;
                    spyOn(router, "navigate");

                    setInputValue(encodedSaveData, "someEncodedSaveData");
                    playStyles[1].nativeElement.click();

                    button.nativeElement.click();

                    // Wait for stability from the headers promise
                    fixture.detectChanges();
                    fixture.whenStable().then(() =>
                    {
                        expect(authenticationService.getAuthHeaders).toHaveBeenCalled();

                        expect(lastConnection).not.toBeNull();
                        expect(lastConnection.request.url).toContain("/api/uploads");
                        expect(lastConnection.request.text()).toEqual("encodedSaveData=someEncodedSaveData&addToProgress=true&playStyle=Hybrid");
                        lastConnection.mockRespond(new Response(new ResponseOptions({ body: "123" })));

                        // Wait for stability from the http call
                        fixture.detectChanges();
                        fixture.whenStable().then(() =>
                        {
                            expect(component.errorMessage).toBeFalsy();
                            expect(router.navigate).toHaveBeenCalledWith(["/calculator/view", 123]);
                        });
                    });
                });
        }));

        it("should show an error when the http call fails", async(() =>
        {
            // Mock the global variable. We should figure out a better way to both inject this in the product and mock this in tests.
            window.appInsights = jasmine.createSpyObj("appInsights", [ "trackEvent" ]);

            setIsLoggedIn(true)
                .then(() => {
                    let router = TestBed.get(Router) as Router;
                    spyOn(router, "navigate");

                    setInputValue(encodedSaveData, "someEncodedSaveData");
                    playStyles[1].nativeElement.click();

                    button.nativeElement.click();

                    // Wait for stability from the headers promise
                    fixture.detectChanges();
                    fixture.whenStable().then(() =>
                    {
                        expect(lastConnection).not.toBeNull();
                        expect(lastConnection.request.url).toContain("/api/uploads");
                        lastConnection.mockError(new Response(new ResponseOptions({ status: 500 })) as {} as Error);

                        // Wait for stability from the http call
                        fixture.detectChanges();
                        fixture.whenStable().then(() =>
                        {
                            expect(window.appInsights.trackEvent).toHaveBeenCalled();
                            expect(component.errorMessage).toBeTruthy();
                            expect(router.navigate).not.toHaveBeenCalled();
                        });
                    });
                });
        }));

        function setIsLoggedIn(value: boolean): Promise<void>
        {
            isLoggedIn.next(value);
            fixture.detectChanges();
            return fixture.whenStable()
                .then(() =>
                {
                    fixture.detectChanges();

                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    encodedSaveData = form.query(By.css("#encodedSaveData"));
                    playStyles = form.queryAll(By.css("[name='playStyle']"));
                    addToProgress = form.query(By.css("#addToProgress"));
                    warningMessage = form.query(By.css(".alert.alert-warning"));
                    button = form.query(By.css("button"));
                });
        }
    });
});

function setInputValue(element: DebugElement, value: string): void
{
    element.nativeElement.value = value;

    // Tell Angular
    let evt = document.createEvent("CustomEvent");
    evt.initCustomEvent("input", false, false, null);
    element.nativeElement.dispatchEvent(evt);
}
