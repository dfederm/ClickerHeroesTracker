import { NO_ERRORS_SCHEMA, DebugElement } from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { HttpClientTestingModule, HttpTestingController } from "@angular/common/http/testing";
import { HttpHeaders, HttpErrorResponse } from "@angular/common/http";

import { AdminComponent, IUploadQueueStats } from "./admin";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { HttpErrorHandlerService } from "../../services/httpErrorHandlerService/httpErrorHandlerService";
import { UploadService } from "../../services/uploadService/uploadService";

describe("AdminComponent", () => {
    let fixture: ComponentFixture<AdminComponent>;
    let httpMock: HttpTestingController;

    const queuesRequest = { method: "get", url: "/api/admin/queues" };
    const recomputeRequest = { method: "post", url: "/api/admin/recompute" };
    const clearQueueRequest = { method: "post", url: "/api/admin/clearqueue" };
    const staleUploadsRequest = { method: "get", url: "/api/admin/staleuploads" };

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

    beforeEach(done => {
        let authenticationService = {
            getAuthHeaders: (): void => void 0,
        };
        let httpErrorHandlerService = {
            logError: (): void => void 0,
            getValidationErrors: (): void => void 0,
        };
        let uploadService = {
            delete: (): void => void 0,
        };

        TestBed.configureTestingModule(
            {
                imports: [
                    FormsModule,
                    HttpClientTestingModule,
                ],
                providers:
                    [
                        { provide: AuthenticationService, useValue: authenticationService },
                        { provide: HttpErrorHandlerService, useValue: httpErrorHandlerService },
                        { provide: UploadService, useValue: uploadService },
                    ],
                declarations: [AdminComponent],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                httpMock = TestBed.get(HttpTestingController) as HttpTestingController;
                fixture = TestBed.createComponent(AdminComponent);
            })
            .then(done)
            .catch(done.fail);
    });

    afterEach(() => {
        httpMock.verify();
    });

    describe("Initialization", () => {
        let authenticationService: AuthenticationService;

        beforeEach(done => {
            authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "getAuthHeaders").and.returnValue(Promise.resolve(new HttpHeaders()));

            fixture.detectChanges();
            fixture.whenStable()
                .then(done)
                .catch(done.fail);
        });

        it("should make api call", () => {
            httpMock.expectOne(queuesRequest);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        });

        let queueDropdownIds = ["recomputePriority", "clearQueuePriority"];
        for (let i = 0; i < queueDropdownIds.length; i++) {
            let queueDropdownId = queueDropdownIds[i];
            it(`should populate the ${queueDropdownId} input with queue data`, done => {
                let request = httpMock.expectOne(queuesRequest);
                request.flush(uploadQueueStats);

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
            let request = httpMock.expectOne(queuesRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });

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
            spyOn(authenticationService, "getAuthHeaders").and.returnValue(Promise.resolve(new HttpHeaders()));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
                    (authenticationService.getAuthHeaders as jasmine.Spy).calls.reset();

                    let request = httpMock.expectOne(queuesRequest);
                    request.flush(uploadQueueStats);

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
                        let request = httpMock.expectOne(recomputeRequest);
                        expect(request.request.body).toEqual({ uploadIds: [123, 456, 789], priority: uploadQueueStats[dropdown.selectedIndex].priority });

                        expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
                        (authenticationService.getAuthHeaders as jasmine.Spy).calls.reset();

                        request.flush(null);
                        return fixture.whenStable();
                    })
                    .then(() => {
                        fixture.detectChanges();
                        return fixture.whenStable();
                    })
                    .then(() => {
                        fixture.detectChanges();

                        httpMock.expectOne(queuesRequest);
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
                        expect(authenticationService.getAuthHeaders).not.toHaveBeenCalled();

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("abc is not a number.");
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should show an error when the api call fails", done => {
                let httpErrorHandlerService = TestBed.get(HttpErrorHandlerService) as HttpErrorHandlerService;
                spyOn(httpErrorHandlerService, "getValidationErrors").and.returnValue(["error0", "error1", "error2"]);
                spyOn(httpErrorHandlerService, "logError");

                setInputValue(form, "recomputeUploadIds", "123,456,789");

                let dropdown = form.query(By.css("select")).nativeElement as HTMLSelectElement;
                setSelectValue(dropdown, dropdown.selectedIndex + 1);

                submit()
                    .then(() => {
                        let request = httpMock.expectOne(recomputeRequest);
                        expect(request.request.body).toEqual({ uploadIds: [123, 456, 789], priority: uploadQueueStats[dropdown.selectedIndex].priority });

                        expect(authenticationService.getAuthHeaders).toHaveBeenCalled();

                        request.flush(null, { status: 400, statusText: "someStatus" });

                        return fixture.whenStable();
                    })
                    .then(() => {
                        fixture.detectChanges();

                        // No idea why this extra round is needed
                        return fixture.whenStable();
                    })
                    .then(() => {
                        fixture.detectChanges();

                        expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("AdminComponent.recompute.error", jasmine.any(HttpErrorResponse));
                        expect(httpErrorHandlerService.getValidationErrors).toHaveBeenCalledWith(jasmine.any(HttpErrorResponse));

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("error0;error1;error2");
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
            spyOn(authenticationService, "getAuthHeaders").and.returnValue(Promise.resolve(new HttpHeaders()));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
                    (authenticationService.getAuthHeaders as jasmine.Spy).calls.reset();

                    let request = httpMock.expectOne(queuesRequest);
                    request.flush(uploadQueueStats);

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
                        let request = httpMock.expectOne(clearQueueRequest);
                        expect(request.request.body).toEqual({ priority: uploadQueueStats[dropdown.selectedIndex].priority });

                        expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
                        (authenticationService.getAuthHeaders as jasmine.Spy).calls.reset();

                        request.flush(null);

                        return fixture.whenStable();
                    })
                    .then(() => {
                        fixture.detectChanges();
                        return fixture.whenStable();
                    })
                    .then(() => {
                        fixture.detectChanges();

                        httpMock.expectOne(queuesRequest);
                        expect(authenticationService.getAuthHeaders).toHaveBeenCalled();

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(0);
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should show an error when the api call fails", done => {
                let httpErrorHandlerService = TestBed.get(HttpErrorHandlerService) as HttpErrorHandlerService;
                spyOn(httpErrorHandlerService, "getValidationErrors").and.returnValue(["error0", "error1", "error2"]);
                spyOn(httpErrorHandlerService, "logError");

                let dropdown = form.query(By.css("select")).nativeElement as HTMLSelectElement;
                setSelectValue(dropdown, dropdown.selectedIndex + 1);

                submit()
                    .then(() => {
                        let request = httpMock.expectOne(clearQueueRequest);
                        expect(request.request.body).toEqual({ priority: uploadQueueStats[dropdown.selectedIndex].priority });

                        expect(authenticationService.getAuthHeaders).toHaveBeenCalled();

                        request.flush(null, { status: 400, statusText: "someStatus" });

                        return fixture.whenStable();
                    })
                    .then(() => {
                        fixture.detectChanges();

                        // No idea why this extra round is needed
                        return fixture.whenStable();
                    })
                    .then(() => {
                        fixture.detectChanges();

                        expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("AdminComponent.clearQueue.error", jasmine.any(HttpErrorResponse));
                        expect(httpErrorHandlerService.getValidationErrors).toHaveBeenCalledWith(jasmine.any(HttpErrorResponse));

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("error0;error1;error2");
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

        let staleuploads: number[] = [];
        for (let i = 0; i < 1000; i++) {
            staleuploads.push(i);
        }

        beforeEach(done => {
            let containers = fixture.debugElement.queryAll(By.css(".col-md-4"));
            expect(containers.length).toEqual(3);
            container = containers[2];

            authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "getAuthHeaders").and.returnValue(Promise.resolve(new HttpHeaders()));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
                    (authenticationService.getAuthHeaders as jasmine.Spy).calls.reset();

                    httpMock.expectOne(queuesRequest);
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

                    expect(authenticationService.getAuthHeaders).toHaveBeenCalled();

                    let request = httpMock.expectOne(staleUploadsRequest);

                    // Make a copy so our mock data doesn't get altered
                    request.flush(JSON.parse(JSON.stringify(staleuploads)));

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
            let httpErrorHandlerService = TestBed.get(HttpErrorHandlerService) as HttpErrorHandlerService;
            spyOn(httpErrorHandlerService, "getValidationErrors").and.returnValue(["error0", "error1", "error2"]);
            spyOn(httpErrorHandlerService, "logError");

            let fetchButton = container.query(By.css("button"));
            fetchButton.nativeElement.click();

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    let request = httpMock.expectOne(staleUploadsRequest);
                    request.flush(null, { status: 400, statusText: "someStatus" });

                    fixture.detectChanges();
                    return fixture.whenStable();
                })
                .then(() => {
                    fixture.detectChanges();

                    expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("AdminComponent.fetchStaleUploads.error", jasmine.any(HttpErrorResponse));
                    expect(httpErrorHandlerService.getValidationErrors).toHaveBeenCalledWith(jasmine.any(HttpErrorResponse));

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(1);
                    expect(errors[0]).toEqual("error0;error1;error2");
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
                    let request = httpMock.expectOne(staleUploadsRequest);

                    // Make a copy so our mock data doesn't get altered
                    request.flush(JSON.parse(JSON.stringify(staleuploads)));

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
                    let request = httpMock.expectOne(staleUploadsRequest);

                    // Make a copy so our mock data doesn't get altered
                    request.flush(JSON.parse(JSON.stringify(staleuploads)));

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
