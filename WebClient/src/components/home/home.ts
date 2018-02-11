import { Component, OnInit } from "@angular/core";
import { UploadDialogComponent } from "../uploadDialog/uploadDialog";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";

@Component({
    selector: "home",
    templateUrl: "./home.html",
})
export class HomeComponent implements OnInit {
    public UploadDialogComponent = UploadDialogComponent;

    public userName: string;

    constructor(
        private readonly authenticationService: AuthenticationService,
    ) { }

    public ngOnInit(): void {
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => this.userName = userInfo.isLoggedIn ? userInfo.username : null);
    }
}
