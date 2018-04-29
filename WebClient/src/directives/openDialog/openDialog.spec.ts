import { NO_ERRORS_SCHEMA, Component, DebugElement } from "@angular/core";
import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { NgbModal, NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

import { OpenDialogDirective } from "./openDialog";

describe("OpenDialogDirective", () => {
    let link: DebugElement;
    let fixture: ComponentFixture<MockComponent>;
    let $event: MouseEvent;

    @Component({
        template: "someDialogContent",
        selector: "mockDialog",
    })
    class MockDialogComponent { }

    // tslint:disable-next-line:max-classes-per-file - Allow multiple mock classes in a file
    @Component({
        template: "<a [openDialog]=\"MockDialogComponent\" [dismissCurrentDialog]=\"dismissCurrentDialog\" ></a>",
        selector: "mock",
    })
    class MockComponent {
        public MockDialogComponent = MockDialogComponent;
        public dismissCurrentDialog: boolean;
    }

    beforeEach(async(() => {
        let modalService = { open: (): void => void 0 };
        let activeModal = { dismiss: (): void => void 0 };
        fixture = TestBed.configureTestingModule(
            {
                declarations:
                    [
                        OpenDialogDirective,
                        MockComponent,
                        MockDialogComponent,
                    ],
                providers:
                    [
                        { provide: NgbModal, useValue: modalService },
                        { provide: NgbActiveModal, useValue: activeModal },
                    ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .createComponent(MockComponent);

        // Initial binding
        fixture.detectChanges();

        link = fixture.debugElement.query(By.directive(OpenDialogDirective));

        let event = jasmine.createSpyObj("$event", ["preventDefault"]);
        event.target = jasmine.createSpyObj("eventTarget", ["blur"]);
        $event = event;
    }));

    it("should add an href attribute", () => {
        expect((link.nativeElement as HTMLAnchorElement).href).toBeTruthy();
    });

    it("should prevent the default click behavior", () => {
        link.triggerEventHandler("click", $event);

        expect($event.preventDefault).toHaveBeenCalled();
    });

    it("should reset the focus off the clicked element", () => {
        link.triggerEventHandler("click", $event);

        expect(($event.target as HTMLElement).blur).toHaveBeenCalled();
    });

    it("should open a dialog", () => {
        let modalService = TestBed.get(NgbModal) as NgbModal;
        spyOn(modalService, "open");

        let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
        spyOn(activeModal, "dismiss");

        link.triggerEventHandler("click", $event);

        expect(modalService.open).toHaveBeenCalledWith(MockDialogComponent);
        expect(activeModal.dismiss).not.toHaveBeenCalled();
    });

    it("should close the active dialog when dismissCurrentDialog is true", () => {
        let modalService = TestBed.get(NgbModal) as NgbModal;
        spyOn(modalService, "open");

        let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
        spyOn(activeModal, "dismiss");

        fixture.componentInstance.dismissCurrentDialog = true;
        fixture.detectChanges();
        link.triggerEventHandler("click", $event);

        expect(modalService.open).toHaveBeenCalledWith(MockDialogComponent);
        expect(activeModal.dismiss).toHaveBeenCalled();
    });

    it("should not close the active dialog when dismissCurrentDialog is false", () => {
        let modalService = TestBed.get(NgbModal) as NgbModal;
        spyOn(modalService, "open");

        let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
        spyOn(activeModal, "dismiss");

        fixture.componentInstance.dismissCurrentDialog = false;
        fixture.detectChanges();
        link.triggerEventHandler("click", $event);

        expect(modalService.open).toHaveBeenCalledWith(MockDialogComponent);
        expect(activeModal.dismiss).not.toHaveBeenCalled();
    });
});
