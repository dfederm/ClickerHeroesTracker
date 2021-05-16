import { Directive, ElementRef, HostListener, Input, Optional } from "@angular/core";
import { NgbModal, NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

@Directive({
    selector: "[openDialog]",
})
export class OpenDialogDirective {
    @Input()
    // tslint:disable-next-line: no-any
    public openDialog: any;

    @Input()
    public dismissCurrentDialog: boolean;

    constructor(
        element: ElementRef,
        private readonly modalService: NgbModal,
        @Optional() private readonly activeModal: NgbActiveModal,
    ) {
        (element.nativeElement as HTMLAnchorElement).href = "#";
    }

    @HostListener("click", ["$event"])
    public onClick($event: MouseEvent): void {
        $event.preventDefault();
        ($event.target as HTMLElement).blur();

        if (this.activeModal && this.dismissCurrentDialog) {
            this.activeModal.dismiss();
        }

        this.modalService.open(this.openDialog);
    }
}
