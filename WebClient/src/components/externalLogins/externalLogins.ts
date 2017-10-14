import { Component, OnInit } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { UserAgentApplication } from "msalx";

import { AuthenticationService } from "../../services/authenticationService/authenticationService";

export interface IErrorResponse {
    error: string;
    error_description?: string;
}

@Component({
    selector: "externalLogins",
    templateUrl: "./externalLogins.html",
})
export class ExternalLoginsComponent implements OnInit {
    // Facebook doesn't give us a way to check if it's initialized, so we track it ourselves.
    private static facebookInitialized = false;

    private static microsoftApp: UserAgentApplication;

    public error: string;

    public needUsername: boolean;

    public provider: string;

    public username: string;

    private grantType: string;

    private assertion: string;

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

        if (!ExternalLoginsComponent.facebookInitialized) {
            FB.init({
                appId: "246885142330300",
                version: "v2.10",
            });
            ExternalLoginsComponent.facebookInitialized = true;
        }

        if (!ExternalLoginsComponent.microsoftApp) {
            ExternalLoginsComponent.microsoftApp = new UserAgentApplication(
                "4ecf3d26-e844-4855-9158-b8f6c0121b50",
                null,
                null,
                {
                    redirectUri: location.origin,
                });
        }
    }

    public googleLogIn(): void {
        this.error = null;
        this.provider = "Google";

        gapi.auth2.getAuthInstance()
            .signIn()
            .then((user: gapi.auth2.GoogleUser) => this.logIn("urn:ietf:params:oauth:grant-type:google_identity_token", user.getAuthResponse().id_token))
            .catch((error: { error: string }) => {
                if (error && error.error === "popup_closed_by_user") {
                    return;
                }

                this.error = "There was a problem logging in with Google";
            });
    }

    public facebookLogIn(): void {
        this.error = null;
        this.provider = "Facebook";

        FB.login(response => {
            if (!response.authResponse) {
                // User cancelled login or did not fully authorize
                return;
            }

            if (response.status === "connected") {
                this.logIn("urn:ietf:params:oauth:grant-type:facebook_access_token", response.authResponse.accessToken)
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
        this.provider = "Microsoft";

        // There is no signal when the user closes the popup. The promise just never resoles.
        // See: https://github.com/AzureAD/microsoft-authentication-library-for-js/issues/129
        ExternalLoginsComponent.microsoftApp.loginPopup(["openid", "email"])
            .then(token => this.logIn("urn:ietf:params:oauth:grant-type:microsoft_identity_token", token))
            .catch((error: string) => {
                if (error && error.startsWith("user_cancelled:")) {
                    return;
                }

                this.error = "There was a problem logging in with Microsoft";
            });
    }

    public chooseUserName(): void {
        this.logIn(this.grantType, this.assertion)
            .catch(error => {
                let errorResponse: IErrorResponse;
                try {
                    errorResponse = error.json();
                } catch (error) {
                    // It must not have been json
                }

                this.error = errorResponse && errorResponse.error_description
                    ? errorResponse.error_description
                    : "There was a problem creating your account";
            });
    }

    private logIn(grantType: string, assertion: string): Promise<void> {
        // Save off for reuse in case the user has to select a username
        this.grantType = grantType;
        this.assertion = assertion;

        return this.authenticationService.logInWithAssertion(grantType, assertion, this.username)
            .then(() => this.activeModal.close())
            .catch(error => {
                let errorResponse: IErrorResponse;
                try {
                    errorResponse = error.json();
                } catch (error) {
                    // It must not have been json
                }

                if (errorResponse && errorResponse.error === "account_selection_required") {
                    this.needUsername = true;
                    return Promise.resolve();
                }

                return Promise.reject(error);
            });
    }
}
