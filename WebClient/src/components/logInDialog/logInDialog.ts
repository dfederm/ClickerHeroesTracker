import { Component, OnInit } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { UserAgentApplication } from "msalx";

import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { RegisterDialogComponent } from "../registerDialog/registerDialog";

@Component({
    selector: "logInDialog",
    templateUrl: "./logInDialog.html",
})
export class LogInDialogComponent implements OnInit {
    // Facebook doesn't give us a way to check if it's initialized, so we track it ourselves.
    private static facebookInitialized = false;

    private static microsoftApp: UserAgentApplication;

    public error: string;

    public username = "";

    public password = "";

    public RegisterDialogComponent = RegisterDialogComponent;

    constructor(
        private authenticationService: AuthenticationService,
        public activeModal: NgbActiveModal,
    ) { }

    public ngOnInit(): void {
        if (!gapi.auth2) {
            gapi.load("auth2", () => {
                gapi.auth2.init({
                    client_id: "371697338749-cbgs417cd45vgktq0kmjanbn3lh2lbl6.apps.googleusercontent.com",
                    scope: "profile",
                });
            });
        }

        if (!LogInDialogComponent.facebookInitialized) {
            FB.init({
                appId: "246885142330300",
                version: "v2.10",
            });
            LogInDialogComponent.facebookInitialized = true;
        }

        if (!LogInDialogComponent.microsoftApp) {
            LogInDialogComponent.microsoftApp = new UserAgentApplication(
                "4ecf3d26-e844-4855-9158-b8f6c0121b50",
                null,
                null,
                {
                    redirectUri: location.origin,
                });
        }
    }

    public logIn(): void {
        this.error = null;
        this.authenticationService.logInWithPassword(this.username, this.password)
            .then(() => {
                this.activeModal.close();
            })
            .catch(() => {
                this.error = "Incorrect username or password";
            });
    }

    public googleLogIn(): void {
        this.error = null;
        gapi.auth2.getAuthInstance()
            .signIn()
            .then((user: gapi.auth2.GoogleUser) => {
                return this.authenticationService.logInWithAssertion(
                    "urn:ietf:params:oauth:grant-type:google_identity_token",
                    user.getAuthResponse().id_token,
                );
            })
            .then(() => {
                this.activeModal.close();
            })
            .catch((error: { error: string }) => {
                if (error && error.error === "popup_closed_by_user") {
                    return;
                }

                this.error = "There was a problem logging in with Google";
            });
    }

    public facebookLogIn(): void {
        this.error = null;
        FB.login(response => {
            if (!response.authResponse) {
                // User cancelled login or did not fully authorize
                return;
            }

            if (response.status === "connected") {
                this.authenticationService.logInWithAssertion(
                    "urn:ietf:params:oauth:grant-type:facebook_access_token",
                    response.authResponse.accessToken,
                )
                    .then(() => {
                        this.activeModal.close();
                    })
                    .catch(() => {
                        this.error = "There was a problem logging in with Facebook";
                    });
            } else {
                // The person is not logged into this app or we are unable to tell.
                this.error = "There was a problem logging in with Facebook";
            }
        });
    }

    public microsoftLogIn(): void {
        this.error = null;

        // There is no signal when the user closes the popup. The promise just never resoles.
        // See: https://github.com/AzureAD/microsoft-authentication-library-for-js/issues/129
        LogInDialogComponent.microsoftApp.loginPopup(["openid"])
            .then(token => {
                return this.authenticationService.logInWithAssertion(
                    "urn:ietf:params:oauth:grant-type:microsoft_identity_token",
                    token,
                );
            })
            .then(() => {
                this.activeModal.close();
            })
            .catch(() => {
                this.error = "There was a problem logging in with Microsoft";
            });
    }
}
