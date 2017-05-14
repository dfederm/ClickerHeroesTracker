import { NO_ERRORS_SCHEMA, Component, DebugElement } from "@angular/core";
import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";

import { OpenDialogDirective } from "./openDialog";

describe("OpenDialogDirective", () =>
{
    let link: DebugElement;
    let fixture: ComponentFixture<MockComponent>;
    let $event: MouseEvent;

    @Component({
        template: "someDialogContent",
    })
    class MockDialogComponent { }

    @Component({
        template: "<a [openDialog]=\"MockDialogComponent\"></a>",
    })
    class MockComponent
    {
        public MockDialogComponent = MockDialogComponent;
    }

    beforeEach(async(() =>
    {
        let modalService = { open: (): void => void 0 };
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
            ],
            schemas: [ NO_ERRORS_SCHEMA ],
        })
        .createComponent(MockComponent);

        // Initial binding
        fixture.detectChanges();

        link = fixture.debugElement.query(By.directive(OpenDialogDirective));

        let event = jasmine.createSpyObj("$event", ["preventDefault"]);
        event.target = jasmine.createSpyObj("eventTarget", ["blur"]);
        $event = event;
    }));

    it("should add an href attribute", () =>
    {
        expect((link.nativeElement as HTMLAnchorElement).href).toBeTruthy();
    });

    it("should prevent the default click behavior", () =>
    {
        link.triggerEventHandler("click", $event);

        expect($event.preventDefault).toHaveBeenCalled();
    });

    it("should reset the focus off the clicked element", () =>
    {
        link.triggerEventHandler("click", $event);

        expect(($event.target as HTMLElement).blur).toHaveBeenCalled();
    });

    it("should open a dialog", () =>
    {
        let modalService = TestBed.get(NgbModal) as NgbModal;
        spyOn(modalService, "open");

        link.triggerEventHandler("click", $event);

        expect(modalService.open).toHaveBeenCalledWith(MockDialogComponent);
    });
});
