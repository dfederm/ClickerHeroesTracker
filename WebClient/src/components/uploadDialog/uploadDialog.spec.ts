import { ComponentFixture, TestBed } from "@angular/core/testing";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { By } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { Component, DebugElement, Input } from "@angular/core";
import { HttpErrorResponse } from "@angular/common/http";
import { Router } from "@angular/router";
import { BehaviorSubject } from "rxjs";

import { UploadDialogComponent } from "./uploadDialog";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { UploadService } from "../../services/uploadService/uploadService";
import { SettingsService } from "../../services/settingsService/settingsService";
import { NgxSpinnerModule } from "ngx-spinner";

describe("UploadDialogComponent", () => {
    let component: UploadDialogComponent;
    let fixture: ComponentFixture<UploadDialogComponent>;
    let userInfo: BehaviorSubject<IUserInfo>;

    @Component({ selector: "ngx-spinner", template: "", standalone: true })
    class MockNgxSpinnerComponent {
        @Input()
        public fullScreen: boolean;
    }
    
    const loggedInUser: IUserInfo = {
        isLoggedIn: true,
        id: "someId",
        username: "someUsername",
        email: "someEmail",
    };

    const notLoggedInUser: IUserInfo = {
        isLoggedIn: false,
    };

    const settings = SettingsService.defaultSettings;
    let settingsSubject = new BehaviorSubject(settings);

    beforeEach(async () => {
        userInfo = new BehaviorSubject(notLoggedInUser);
        let authenticationService = { userInfo: () => userInfo };
        let uploadService = {
            create: (): void => void 0,
        };
        let settingsService = { settings: () => settingsSubject };
        let activeModal = { close: (): void => void 0 };
        let router = { navigate: (): void => void 0 };

        await TestBed.configureTestingModule(
            {
                imports: [
                    FormsModule,
                    UploadDialogComponent,
                ],
                providers: [
                    { provide: AuthenticationService, useValue: authenticationService },
                    { provide: UploadService, useValue: uploadService },
                    { provide: SettingsService, useValue: settingsService },
                    { provide: NgbActiveModal, useValue: activeModal },
                    { provide: Router, useValue: router },
                ],
            })
            .compileComponents();
        TestBed.overrideComponent(UploadDialogComponent, {
            remove: { imports: [ NgxSpinnerModule ]},
            add: { imports: [ MockNgxSpinnerComponent ] },
        });

        fixture = TestBed.createComponent(UploadDialogComponent);
        component = fixture.componentInstance;
    });

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

        it("should display the form elements with 'add to progress' when the user is logged in", async () => {
            await setUserInfo(loggedInUser);
            expect(encodedSaveData).not.toBeNull();
            expect(playStyles.length).toEqual(3);
            expect(addToProgress).not.toBeNull();
            expect(warningMessage).toBeNull();
            expect(button).not.toBeNull();
        });

        it("should display the form elements without 'add to progress' when the user is not logged in", async () => {
            await setUserInfo(notLoggedInUser);
            expect(encodedSaveData).not.toBeNull();
            expect(playStyles.length).toEqual(3);
            expect(addToProgress).toBeNull();
            expect(warningMessage).not.toBeNull();
            expect(button).not.toBeNull();
        });

        it("should show an error with empty save data", async () => {
            await setUserInfo(notLoggedInUser);
            button.nativeElement.click();
            expect(component.errorMessage).toBeTruthy();
        });

        it("should upload correct save data when user is not logged in", async () => {
            await setUserInfo(notLoggedInUser);

            let uploadService = TestBed.inject(UploadService);
            spyOn(uploadService, "create").and.returnValue(Promise.resolve<number>(123));

            let router = TestBed.inject(Router);
            spyOn(router, "navigate");

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            setInputValue(encodedSaveData, "someEncodedSaveData");
            playStyles[1].nativeElement.click();

            button.nativeElement.click();

            // Wait for stability from the uploadService promise
            fixture.detectChanges();
            await fixture.whenStable();

            expect(uploadService.create).toHaveBeenCalledWith("someEncodedSaveData", true, "hybrid");
            expect(component.errorMessage).toBeFalsy();
            expect(router.navigate).toHaveBeenCalledWith(["/uploads", 123]);
            expect(activeModal.close).toHaveBeenCalled();
        });

        it("should upload correct save data when user is logged in", async () => {
            await setUserInfo(loggedInUser);

            let uploadService = TestBed.inject(UploadService);
            spyOn(uploadService, "create").and.returnValue(Promise.resolve<number>(123));

            let router = TestBed.inject(Router);
            spyOn(router, "navigate");

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            setInputValue(encodedSaveData, "someEncodedSaveData");
            playStyles[1].nativeElement.click();

            button.nativeElement.click();

            // Wait for stability from the uploadService promise
            fixture.detectChanges();
            await fixture.whenStable();

            expect(uploadService.create).toHaveBeenCalledWith("someEncodedSaveData", true, "hybrid");
            expect(component.errorMessage).toBeFalsy();
            expect(router.navigate).toHaveBeenCalledWith(["/uploads", 123]);
            expect(activeModal.close).toHaveBeenCalled();
        });

        it("should show an error when uploadService fails", async () => {
            await setUserInfo(loggedInUser);

            let uploadService = TestBed.inject(UploadService);
            spyOn(uploadService, "create").and.returnValue(Promise.reject(new HttpErrorResponse({ status: 500, statusText: "someStatusText" })));

            let router = TestBed.inject(Router);
            spyOn(router, "navigate");

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            setInputValue(encodedSaveData, "someEncodedSaveData");
            playStyles[1].nativeElement.click();

            button.nativeElement.click();

            // Wait for stability from the uploadService promise
            fixture.detectChanges();
            await fixture.whenStable();

            expect(component.errorMessage).toBeTruthy();
            expect(router.navigate).not.toHaveBeenCalled();
            expect(activeModal.close).not.toHaveBeenCalled();
        });

        async function setUserInfo(value: IUserInfo): Promise<void> {
            userInfo.next(value);

            fixture.detectChanges();
            await fixture.whenStable();
            fixture.detectChanges();

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let form = body.query(By.css("form"));
            expect(form).not.toBeNull();

            encodedSaveData = form.query(By.css("#encodedSaveData"));
            playStyles = form.queryAll(By.css("[name='playStyle']"));
            addToProgress = form.query(By.css("#addToProgress"));
            warningMessage = form.query(By.css(".alert-warning"));
            button = form.query(By.css("button[type='submit']"));
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
