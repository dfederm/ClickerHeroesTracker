import { NO_ERRORS_SCHEMA, DebugElement } from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { BaseRequestOptions, ConnectionBackend, Http, Headers, RequestOptions, RequestMethod, Response, ResponseOptions } from "@angular/http";
import { MockBackend, MockConnection } from "@angular/http/testing";

import { AdminComponent, IUploadQueueStats, IValidationErrorResponse } from "./admin";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { UploadService } from "../../services/uploadService/uploadService";

class MockError extends Response implements Error {
    public name: string;
    public message: string;
}

describe("AdminComponent", () => {
    let fixture: ComponentFixture<AdminComponent>;
    let backend: MockBackend;
    let lastConnection: MockConnection = null;

    const uploadQueueStats: IUploadQueueStats[] = [
        {
            priority: "somePriority0",
            numMessages: 0,
        },
        {
            priority: "somePriority1",
            numMessages: 1,
        },
        {
            priority: "somePriority2",
            numMessages: 2,
        },
    ];

    let staleuploads: number[] = [];
    for (let i = 0; i < 1000; i++) {
        staleuploads.push(i);
    }

    beforeEach(done => {
        let authenticationService = {
            getAuthHeaders: (): void => void 0,
        };
        let appInsights = {
            trackEvent: (): void => void 0,
        };
        let uploadService = {
            delete: (): void => void 0,
        };

        TestBed.configureTestingModule(
            {
                imports: [
                    FormsModule,
                ],
                providers:
                    [
                        { provide: AuthenticationService, useValue: authenticationService },
                        { provide: AppInsightsService, useValue: appInsights },
                        { provide: UploadService, useValue: uploadService },
                        { provide: ConnectionBackend, useClass: MockBackend },
                        { provide: RequestOptions, useClass: BaseRequestOptions },
                        Http,
                    ],
                declarations: [AdminComponent],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                backend = TestBed.get(ConnectionBackend) as MockBackend;
                backend.connections.subscribe((connection: MockConnection) => {
                    if (lastConnection != null) {
                        fail("Previous connection not handled");
                    }

                    lastConnection = connection;
                });
                fixture = TestBed.createComponent(AdminComponent);
            })
            .then(done)
            .catch(done.fail);
    });

    afterEach(() => {
        lastConnection = null;
        backend.verifyNoPendingRequests();
    });

    describe("Initialization", () => {
        let authenticationService: AuthenticationService;

        beforeEach(done => {
            authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "getAuthHeaders").and.returnValue(Promise.resolve(new Headers()));

            fixture.detectChanges();
            fixture.whenStable()
                .then(done)
                .catch(done.fail);
        });

        it("should make api call", () => {
            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/admin/queues", "url invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        });

        let queueDropdownIds = ["recomputePriority", "clearQueuePriority"];
        for (let i = 0; i < queueDropdownIds.length; i++) {
            let queueDropdownId = queueDropdownIds[i];
            it(`should populate the ${queueDropdownId} input with queue data`, done => {
                lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(uploadQueueStats) })));
                lastConnection = null;

                fixture.detectChanges();
                fixture.whenStable()
                    .then(() => {
                        // Need to wait for stability again since ngModel is async
                        fixture.detectChanges();
                        return fixture.whenStable();
                    })
                    .then(() => {
                        let dropdownDebugElement = fixture.debugElement.query(By.css("#" + queueDropdownId));
                        expect(dropdownDebugElement).not.toBeNull();

                        let dropdown = dropdownDebugElement.nativeElement as HTMLSelectElement;
                        expect(dropdown.selectedIndex).toEqual(0);

                        let options = dropdown.options;
                        expect(options.length).toEqual(uploadQueueStats.length);
                        for (let j = 0; j < options.length; j++) {
                            let option = options[j];
                            expect(option.value).toEqual(`${j}: ${uploadQueueStats[j].priority}`);
                            expect(option.text).toEqual(`${uploadQueueStats[j].priority} (~${uploadQueueStats[j].numMessages} messages)`);
                        }
                    })
                    .then(done)
                    .catch(done.fail);
            });
        }

        it("should show errors when queue data fetch fails", done => {
            lastConnection.mockError(new Error());
            lastConnection = null;

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(2);
                    expect(errors[0]).toEqual("Could not fetch queue data");
                    expect(errors[1]).toEqual("Could not fetch queue data");
                })
                .then(done)
                .catch(done.fail);
        });
    });

    describe("Recalculate form", () => {
        let form: DebugElement;
        let authenticationService: AuthenticationService;

        beforeEach(done => {
            authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "getAuthHeaders").and.returnValue(Promise.resolve(new Headers()));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
                    (authenticationService.getAuthHeaders as jasmine.Spy).calls.reset();

                    lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(uploadQueueStats) })));
                    lastConnection = null;

                    fixture.detectChanges();
                    return fixture.whenStable();
                })
                .then(() => {
                    let forms = fixture.debugElement.queryAll(By.css("form"));
                    expect(forms.length).toEqual(2);
                    form = forms[0];

                    // Need to wait for stability again since ngModel is async
                    fixture.detectChanges();
                    return fixture.whenStable();
                })
                .then(done)
                .catch(done.fail);
        });

        describe("Validation", () => {
            it("should disable the submit button initially", () => {
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                expect(button.properties.disabled).toEqual(true);

                let errors = getAllErrors();
                expect(errors.length).toEqual(0);
            });

            it("should enable the submit button when all inputs are valid", () => {
                setInputValue(form, "recomputeUploadIds", "123");

                fixture.detectChanges();
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                expect(button.properties.disabled).toEqual(false);

                let errors = getAllErrors();
                expect(errors.length).toEqual(0);
            });

            it("should disable the submit button with empty upload ids", () => {
                setInputValue(form, "recomputeUploadIds", "123");
                setInputValue(form, "recomputeUploadIds", "");

                fixture.detectChanges();
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                expect(button.properties.disabled).toEqual(true);

                // No errors actually shown. Admins know what to do.
                let errors = getAllErrors();
                expect(errors.length).toEqual(0);
            });
        });

        describe("Form submission", () => {
            it("should make api call and refresh queue data when successful", done => {
                setInputValue(form, "recomputeUploadIds", "123,456,789");

                let dropdown = form.query(By.css("select")).nativeElement as HTMLSelectElement;
                setSelectValue(dropdown, dropdown.selectedIndex + 1);

                submit()
                    .then(() => {
                        expect(lastConnection).toBeDefined("no http service connection made");
                        expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
                        expect(lastConnection.request.url).toEqual("/api/admin/recompute", "url invalid");
                        expect(lastConnection.request.json()).toEqual({ uploadIds: [123, 456, 789], priority: uploadQueueStats[dropdown.selectedIndex].priority }, "request body invalid");
                        expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
                        (authenticationService.getAuthHeaders as jasmine.Spy).calls.reset();

                        lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(uploadQueueStats) })));
                        lastConnection = null;
                        return fixture.whenStable();
                    })
                    .then(() => {
                        fixture.detectChanges();
                        return fixture.whenStable();
                    })
                    .then(() => {
                        fixture.detectChanges();

                        expect(lastConnection).toBeDefined("no http service connection made");
                        expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
                        expect(lastConnection.request.url).toEqual("/api/admin/queues", "url invalid");
                        expect(authenticationService.getAuthHeaders).toHaveBeenCalled();

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(0);
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should show an error when trying to submit a bad upload id", done => {
                setInputValue(form, "recomputeUploadIds", "123,abc,456");

                let dropdown = form.query(By.css("select")).nativeElement as HTMLSelectElement;
                setSelectValue(dropdown, dropdown.selectedIndex + 1);

                submit()
                    .then(() => {
                        expect(lastConnection).toBeNull();
                        expect(authenticationService.getAuthHeaders).not.toHaveBeenCalled();

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("abc is not a number.");
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should show an error when the api call fails", done => {
                setInputValue(form, "recomputeUploadIds", "123,456,789");

                let dropdown = form.query(By.css("select")).nativeElement as HTMLSelectElement;
                setSelectValue(dropdown, dropdown.selectedIndex + 1);

                submit()
                    .then(() => {
                        expect(lastConnection).toBeDefined("no http service connection made");
                        expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
                        expect(lastConnection.request.url).toEqual("/api/admin/recompute", "url invalid");
                        expect(lastConnection.request.json()).toEqual({ uploadIds: [123, 456, 789], priority: uploadQueueStats[dropdown.selectedIndex].priority }, "request body invalid");
                        expect(authenticationService.getAuthHeaders).toHaveBeenCalled();

                        let validationError: IValidationErrorResponse = {
                            field0: ["error0_0", "error0_1", "error0_2"],
                            field1: ["error1_0", "error1_1", "error1_2"],
                            field2: ["error2_0", "error2_1", "error2_2"],
                        };
                        lastConnection.mockError(new MockError(new ResponseOptions({ body: validationError })));
                        lastConnection = null;
                        return fixture.whenStable();
                    })
                    .then(() => {
                        fixture.detectChanges();

                        // No idea why this extra round is needed
                        return fixture.whenStable();
                    })
                    .then(() => {
                        fixture.detectChanges();

                        expect(lastConnection).toBeNull();

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("error0_0;error0_1;error0_2;error1_0;error1_1;error1_2;error2_0;error2_1;error2_2");
                    })
                    .then(done)
                    .catch(done.fail);
            });

            function submit(): Promise<void> {
                fixture.detectChanges();
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                button.nativeElement.click();

                return fixture.whenStable()
                    .then(() => fixture.detectChanges());
            }
        });
    });

    describe("Clear queue form", () => {
        let form: DebugElement;
        let authenticationService: AuthenticationService;

        beforeEach(done => {
            authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "getAuthHeaders").and.returnValue(Promise.resolve(new Headers()));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
                    (authenticationService.getAuthHeaders as jasmine.Spy).calls.reset();

                    lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(uploadQueueStats) })));
                    lastConnection = null;

                    fixture.detectChanges();
                    return fixture.whenStable();
                })
                .then(() => {
                    let forms = fixture.debugElement.queryAll(By.css("form"));
                    expect(forms.length).toEqual(2);
                    form = forms[1];

                    // Need to wait for stability again since ngModel is async
                    fixture.detectChanges();
                    return fixture.whenStable();
                })
                .then(done)
                .catch(done.fail);
        });

        describe("Form submission", () => {
            it("should make api call and refresh queue data when successful", done => {
                let dropdown = form.query(By.css("select")).nativeElement as HTMLSelectElement;
                setSelectValue(dropdown, dropdown.selectedIndex + 1);

                submit()
                    .then(() => {
                        expect(lastConnection).toBeDefined("no http service connection made");
                        expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
                        expect(lastConnection.request.url).toEqual("/api/admin/clearqueue", "url invalid");
                        expect(lastConnection.request.json()).toEqual({ priority: uploadQueueStats[dropdown.selectedIndex].priority }, "request body invalid");
                        expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
                        (authenticationService.getAuthHeaders as jasmine.Spy).calls.reset();

                        lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(uploadQueueStats) })));
                        lastConnection = null;
                        return fixture.whenStable();
                    })
                    .then(() => {
                        fixture.detectChanges();
                        return fixture.whenStable();
                    })
                    .then(() => {
                        fixture.detectChanges();

                        expect(lastConnection).toBeDefined("no http service connection made");
                        expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
                        expect(lastConnection.request.url).toEqual("/api/admin/queues", "url invalid");
                        expect(authenticationService.getAuthHeaders).toHaveBeenCalled();

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(0);
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should show an error when the api call fails", done => {
                let dropdown = form.query(By.css("select")).nativeElement as HTMLSelectElement;
                setSelectValue(dropdown, dropdown.selectedIndex + 1);

                submit()
                    .then(() => {
                        expect(lastConnection).toBeDefined("no http service connection made");
                        expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
                        expect(lastConnection.request.url).toEqual("/api/admin/clearqueue", "url invalid");
                        expect(lastConnection.request.json()).toEqual({ priority: uploadQueueStats[dropdown.selectedIndex].priority }, "request body invalid");
                        expect(authenticationService.getAuthHeaders).toHaveBeenCalled();

                        let validationError: IValidationErrorResponse = {
                            field0: ["error0_0", "error0_1", "error0_2"],
                            field1: ["error1_0", "error1_1", "error1_2"],
                            field2: ["error2_0", "error2_1", "error2_2"],
                        };
                        lastConnection.mockError(new MockError(new ResponseOptions({ body: validationError })));
                        lastConnection = null;
                        return fixture.whenStable();
                    })
                    .then(() => {
                        fixture.detectChanges();

                        // No idea why this extra round is needed
                        return fixture.whenStable();
                    })
                    .then(() => {
                        fixture.detectChanges();

                        expect(lastConnection).toBeNull();

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("error0_0;error0_1;error0_2;error1_0;error1_1;error1_2;error2_0;error2_1;error2_2");
                    })
                    .then(done)
                    .catch(done.fail);
            });

            function submit(): Promise<void> {
                fixture.detectChanges();
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                button.nativeElement.click();

                return fixture.whenStable()
                    .then(() => fixture.detectChanges());
            }
        });
    });

    describe("Stale uploads form", () => {
        let authenticationService: AuthenticationService;
        let container: DebugElement;

        beforeEach(done => {
            let containers = fixture.debugElement.queryAll(By.css(".col-md-4"));
            expect(containers.length).toEqual(3);
            container = containers[2];

            authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "getAuthHeaders").and.returnValue(Promise.resolve(new Headers()));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
                    (authenticationService.getAuthHeaders as jasmine.Spy).calls.reset();

                    // Ignore the queue data fetch
                    lastConnection = null;
                })
                .then(done)
                .catch(done.fail);
        });

        it("should initially just show the fetch button", () => {
            let progressBar = container.query(By.css("ngb-progressbar"));
            expect(progressBar).toBeNull();

            let buttons = container.queryAll(By.css("button"));
            expect(buttons.length).toEqual(1);
            expect(buttons[0].nativeElement.textContent.trim()).toEqual("Fetch");

            let errors = getAllErrors();
            expect(errors.length).toEqual(0);
        });

        it("should fetch stale uploads", done => {
            let fetchButton = container.query(By.css("button"));
            fetchButton.nativeElement.click();

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(lastConnection).toBeDefined("no http service connection made");
                    expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
                    expect(lastConnection.request.url).toEqual("/api/admin/staleuploads", "url invalid");
                    expect(authenticationService.getAuthHeaders).toHaveBeenCalled();

                    lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(staleuploads) })));
                    lastConnection = null;

                    fixture.detectChanges();
                    return fixture.whenStable();
                })
                .then(() => {
                    fixture.detectChanges();

                    let progressBar = container.query(By.css("ngb-progressbar"));
                    expect(progressBar).not.toBeNull();
                    expect(progressBar.properties.value).toEqual(0);

                    let buttons = container.queryAll(By.css("button"));
                    expect(buttons.length).toEqual(2);
                    expect(buttons[0].nativeElement.textContent.trim()).toEqual("Fetch");
                    expect(buttons[1].nativeElement.textContent.trim()).toEqual("Delete");

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(0);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show errors when stale uploads fetch fails", done => {
            let fetchButton = container.query(By.css("button"));
            fetchButton.nativeElement.click();

            let validationError: IValidationErrorResponse = {
                field0: ["error0_0", "error0_1", "error0_2"],
                field1: ["error1_0", "error1_1", "error1_2"],
                field2: ["error2_0", "error2_1", "error2_2"],
            };

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    lastConnection.mockError(new MockError(new ResponseOptions({ body: validationError })));
                    lastConnection = null;

                    fixture.detectChanges();
                    return fixture.whenStable();
                })
                .then(() => {
                    fixture.detectChanges();

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(1);
                    expect(errors[0]).toEqual("error0_0;error0_1;error0_2;error1_0;error1_1;error1_2;error2_0;error2_1;error2_2");
                })
                .then(done)
                .catch(done.fail);
        });

        it("should delete stale uploads", done => {
            let uploadService = TestBed.get(UploadService) as UploadService;
            spyOn(uploadService, "delete").and.returnValue(Promise.resolve());

            let fetchButton = container.query(By.css("button"));
            fetchButton.nativeElement.click();

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(staleuploads) })));
                    lastConnection = null;

                    fixture.detectChanges();
                    return fixture.whenStable();
                })
                .then(() => {
                    fixture.detectChanges();

                    let progressBar = container.query(By.css("ngb-progressbar"));
                    expect(progressBar).not.toBeNull();
                    expect(progressBar.properties.value).toEqual(0);

                    let buttons = container.queryAll(By.css("button"));
                    expect(buttons.length).toEqual(2);

                    let deleteButton = buttons[1];
                    deleteButton.nativeElement.click();

                    return fixture.whenStable();
                })
                .then(() => {
                    fixture.detectChanges();

                    expect(uploadService.delete).toHaveBeenCalledTimes(staleuploads.length);
                    for (let i = 0; i < staleuploads.length; i++) {
                        expect(uploadService.delete).toHaveBeenCalledWith(staleuploads[i]);
                    }

                    let progressBar = container.query(By.css("ngb-progressbar"));
                    expect(progressBar).not.toBeNull();
                    expect(progressBar.properties.value).toEqual(100);

                    let buttons = container.queryAll(By.css("button"));
                    expect(buttons.length).toEqual(1);
                    expect(buttons[0].nativeElement.textContent.trim()).toEqual("Fetch");

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(0);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should cancel deletion", done => {
            let unresolvedPromises: (() => void)[] = [];
            let numDeletedStaleUploads = staleuploads.length / 2;
            let uploadService = TestBed.get(UploadService) as UploadService;
            spyOn(uploadService, "delete").and.callFake((uploadId: number) => {
                // Pause half way through
                return uploadId < numDeletedStaleUploads
                    ? Promise.resolve()
                    : new Promise((resolve) => unresolvedPromises.push(resolve));
            });

            let fetchButton = container.query(By.css("button"));
            fetchButton.nativeElement.click();

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(staleuploads) })));
                    lastConnection = null;

                    fixture.detectChanges();
                    return fixture.whenStable();
                })
                .then(() => {
                    fixture.detectChanges();

                    let progressBar = container.query(By.css("ngb-progressbar"));
                    expect(progressBar).not.toBeNull();
                    expect(progressBar.properties.value).toEqual(0);

                    let buttons = container.queryAll(By.css("button"));
                    expect(buttons.length).toEqual(2);

                    let deleteButton = buttons[1];
                    deleteButton.nativeElement.click();

                    return fixture.whenStable();
                })
                .then(() => {
                    fixture.detectChanges();

                    let expectedNumberOfDeleteCalls = numDeletedStaleUploads + AdminComponent.numParallelDeletes;
                    expect(uploadService.delete).toHaveBeenCalledTimes(expectedNumberOfDeleteCalls);
                    for (let i = 0; i < expectedNumberOfDeleteCalls; i++) {
                        expect(uploadService.delete).toHaveBeenCalledWith(staleuploads[i]);
                    }

                    (uploadService.delete as jasmine.Spy).calls.reset();

                    let progressBar = container.query(By.css("ngb-progressbar"));
                    expect(progressBar).not.toBeNull();
                    expect(progressBar.properties.value).toEqual(100 * numDeletedStaleUploads / staleuploads.length);

                    let buttons = container.queryAll(By.css("button"));
                    expect(buttons.length).toEqual(1);

                    let cancelButton = buttons[0];
                    expect(cancelButton.nativeElement.textContent.trim()).toEqual("Cancel");
                    cancelButton.nativeElement.click();

                    expect(unresolvedPromises.length).toEqual(AdminComponent.numParallelDeletes);
                    for (let i = 0; i < unresolvedPromises.length; i++) {
                        unresolvedPromises[i]();
                    }

                    return fixture.whenStable();
                })
                .then(() => {
                    // Need a 2nd round to update the property
                    fixture.detectChanges();
                    return fixture.whenStable();
                })
                .then(() => {
                    fixture.detectChanges();

                    expect(uploadService.delete).not.toHaveBeenCalled();

                    // We will only have let the pending requests finish before stopping
                    let progressBar = container.query(By.css("ngb-progressbar"));
                    expect(progressBar).not.toBeNull();
                    expect(progressBar.properties.value).toEqual(100 * (numDeletedStaleUploads + AdminComponent.numParallelDeletes) / staleuploads.length);

                    let buttons = container.queryAll(By.css("button"));
                    expect(buttons.length).toEqual(2);
                    expect(buttons[0].nativeElement.textContent.trim()).toEqual("Fetch");
                    expect(buttons[1].nativeElement.textContent.trim()).toEqual("Delete");

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(0);
                })
                .then(done)
                .catch(done.fail);
        });
    });

    function setInputValue(form: DebugElement, id: string, value: string): void {
        fixture.detectChanges();
        let element = form.query(By.css("#" + id));
        expect(element).not.toBeNull();
        element.nativeElement.value = value;

        // Tell Angular
        let evt = document.createEvent("CustomEvent");
        evt.initCustomEvent("input", false, false, null);
        element.nativeElement.dispatchEvent(evt);
    }

    function setSelectValue(select: HTMLSelectElement, selectedIndex: number): void {
        select.selectedIndex = selectedIndex;

        // Tell Angular
        let evt = document.createEvent("CustomEvent");
        evt.initCustomEvent("change", false, false, null);
        select.dispatchEvent(evt);

        fixture.detectChanges();
    }

    function getAllErrors(): string[] {
        let errors: string[] = [];
        let errorElements = fixture.debugElement.queryAll(By.css(".alert-danger"));
        for (let i = 0; i < errorElements.length; i++) {
            errors.push(errorElements[i].nativeElement.textContent.trim());
        }

        return errors;
    }
});
