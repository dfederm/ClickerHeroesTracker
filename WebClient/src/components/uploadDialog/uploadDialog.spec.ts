import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { By } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { NO_ERRORS_SCHEMA, DebugElement } from "@angular/core";
import { Response, ResponseOptions } from "@angular/http";
import { Router } from "@angular/router";
import { BehaviorSubject } from "rxjs/BehaviorSubject";

import { UploadDialogComponent } from "./uploadDialog";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { UploadService } from "../../services/uploadService/uploadService";

describe("UploadDialogComponent", () => {
    let component: UploadDialogComponent;
    let fixture: ComponentFixture<UploadDialogComponent>;
    let userInfo: BehaviorSubject<IUserInfo>;

    const loggedInUser: IUserInfo = {
        isLoggedIn: true,
        id: "someId",
        username: "someUsername",
        email: "someEmail",
    };

    const notLoggedInUser: IUserInfo = {
        isLoggedIn: false,
    };

    beforeEach(async(() => {
        userInfo = new BehaviorSubject(notLoggedInUser);
        let authenticationService = { userInfo: () => userInfo };
        let uploadService = {
            create: (): void => void 0,
        };
        let activeModal = { close: (): void => void 0 };
        let router = { navigate: (): void => void 0 };

        TestBed.configureTestingModule(
            {
                imports: [FormsModule],
                declarations: [UploadDialogComponent],
                providers:
                [
                    { provide: AuthenticationService, useValue: authenticationService },
                    { provide: UploadService, useValue: uploadService },
                    { provide: NgbActiveModal, useValue: activeModal },
                    { provide: Router, useValue: router },
                ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(UploadDialogComponent);
                component = fixture.componentInstance;
            });
    }));

    it("should display the modal header", () => {
        fixture.detectChanges();

        let header = fixture.debugElement.query(By.css(".modal-header"));
        expect(header).not.toBeNull();

        let title = header.query(By.css(".modal-title"));
        expect(title).not.toBeNull();
        expect(title.nativeElement.textContent).toEqual("Upload your save");
    });

    describe("upload", () => {
        let encodedSaveData: DebugElement;
        let playStyles: DebugElement[];
        let addToProgress: DebugElement;
        let warningMessage: DebugElement;
        let button: DebugElement;

        it("should display the form elements with 'add to progress' when the user is logged in", async(() => {
            setUserInfo(loggedInUser)
                .then(() => {
                    expect(encodedSaveData).not.toBeNull();
                    expect(playStyles.length).toEqual(3);
                    expect(addToProgress).not.toBeNull();
                    expect(warningMessage).toBeNull();
                    expect(button).not.toBeNull();
                });
        }));

        it("should display the form elements without 'add to progress' when the user is not logged in", async(() => {
            setUserInfo(notLoggedInUser)
                .then(() => {
                    expect(encodedSaveData).not.toBeNull();
                    expect(playStyles.length).toEqual(3);
                    expect(addToProgress).toBeNull();
                    expect(warningMessage).not.toBeNull();
                    expect(button).not.toBeNull();
                });
        }));

        it("should show an error with empty save data", async(() => {
            setUserInfo(notLoggedInUser)
                .then(() => {
                    button.nativeElement.click();

                    expect(component.errorMessage).toBeTruthy();
                });
        }));

        it("should upload correct save data when user is not logged in", async(() => {
            setUserInfo(notLoggedInUser)
                .then(() => {
                    let uploadService = TestBed.get(UploadService) as UploadService;
                    spyOn(uploadService, "create").and.returnValue(Promise.resolve<number>(123));

                    let router = TestBed.get(Router) as Router;
                    spyOn(router, "navigate");

                    let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
                    spyOn(activeModal, "close");

                    setInputValue(encodedSaveData, "someEncodedSaveData");
                    playStyles[1].nativeElement.click();

                    button.nativeElement.click();

                    // Wait for stability from the uploadService promise
                    fixture.detectChanges();
                    fixture.whenStable().then(() => {
                        expect(uploadService.create).toHaveBeenCalledWith("someEncodedSaveData", true, "Hybrid");

                        expect(component.errorMessage).toBeFalsy();
                        expect(router.navigate).toHaveBeenCalledWith(["/upload", 123]);
                        expect(activeModal.close).toHaveBeenCalled();
                    });
                });
        }));

        it("should upload correct save data when user is logged in", async(() => {
            setUserInfo(loggedInUser)
                .then(() => {
                    let uploadService = TestBed.get(UploadService) as UploadService;
                    spyOn(uploadService, "create").and.returnValue(Promise.resolve<number>(123));

                    let router = TestBed.get(Router) as Router;
                    spyOn(router, "navigate");

                    let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
                    spyOn(activeModal, "close");

                    setInputValue(encodedSaveData, "someEncodedSaveData");
                    playStyles[1].nativeElement.click();

                    button.nativeElement.click();

                    // Wait for stability from the uploadService promise
                    fixture.detectChanges();
                    fixture.whenStable().then(() => {
                        expect(uploadService.create).toHaveBeenCalledWith("someEncodedSaveData", true, "Hybrid");

                        expect(component.errorMessage).toBeFalsy();
                        expect(router.navigate).toHaveBeenCalledWith(["/upload", 123]);
                        expect(activeModal.close).toHaveBeenCalled();
                    });
                });
        }));

        it("should show an error when uploadService fails", async(() => {
            setUserInfo(loggedInUser)
                .then(() => {
                    let uploadService = TestBed.get(UploadService) as UploadService;
                    spyOn(uploadService, "create").and.returnValue(Promise.reject(new Response(new ResponseOptions({ status: 500 }))));

                    let router = TestBed.get(Router) as Router;
                    spyOn(router, "navigate");

                    let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
                    spyOn(activeModal, "close");

                    setInputValue(encodedSaveData, "someEncodedSaveData");
                    playStyles[1].nativeElement.click();

                    button.nativeElement.click();

                    // Wait for stability from the uploadService promise
                    fixture.detectChanges();
                    fixture.whenStable().then(() => {
                        expect(component.errorMessage).toBeTruthy();
                        expect(router.navigate).not.toHaveBeenCalled();
                        expect(activeModal.close).not.toHaveBeenCalled();
                    });
                });
        }));

        function setUserInfo(value: IUserInfo): Promise<void> {
            userInfo.next(value);
            fixture.detectChanges();
            return fixture.whenStable()
                .then(() => {
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

function setInputValue(element: DebugElement, value: string): void {
    element.nativeElement.value = value;

    // Tell Angular
    let evt = document.createEvent("CustomEvent");
    evt.initCustomEvent("input", false, false, null);
    element.nativeElement.dispatchEvent(evt);
}
