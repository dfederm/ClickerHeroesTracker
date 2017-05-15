import { Directive, ElementRef, HostListener, Input } from "@angular/core";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";

@Directive({
    selector: "[openDialog]",
})
export class OpenDialogDirective
{
    @Input("openDialog")
    public dialog: string;

    constructor(
        element: ElementRef,
        private modalService: NgbModal)
    {
       (element.nativeElement as HTMLAnchorElement).href = "#";
    }

    @HostListener("click", ["$event"])
    public onClick($event: MouseEvent): void
    {
        $event.preventDefault();
        ($event.target as HTMLElement).blur();
        this.modalService.open(this.dialog);
    }
}
