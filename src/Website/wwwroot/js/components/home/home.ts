import { Component } from "@angular/core";
import { UploadDialogComponent } from "../uploadDialog/uploadDialog";

@Component({
    selector: "home",
    templateUrl: "./js/components/home/home.html",
})
export class HomeComponent
{
    public UploadDialogComponent = UploadDialogComponent;
}
