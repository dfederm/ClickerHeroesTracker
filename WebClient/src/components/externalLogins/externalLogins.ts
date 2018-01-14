import { Component, OnInit, Input } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { UserAgentApplication } from "msal";

import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { UserService, IUserLogins, IExternalLogin } from "../../services/userService/userService";

export interface ILoginButton {
    name: string;
    logIn(): void;
}

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
    public static facebookInitialized = false;

    public microsoftApp: UserAgentApplication;

    @Input()
    public isManageMode: boolean;

    public logins: IUserLogins;

    public error: string;

    public isLoading: boolean;

    public needUsername: boolean;

    public provider: string;

    public username: string;

    public addLogins: ILoginButton[];

    private grantType: string;

    private assertion: string;

    private readonly allLogins: ILoginButton[] = [
        {
            name: "Google",
            logIn: () => this.googleLogIn(),
        },
        {
            name: "Facebook",
            logIn: () => this.facebookLogIn(),
        },
        {
            name: "Microsoft",
            logIn: () => this.microsoftLogIn(),
        },
    ];

    constructor(
        private readonly authenticationService: AuthenticationService,
        public activeModal: NgbActiveModal,
        private readonly userService: UserService,
    ) { }

    public ngOnInit(): void {
        if (this.isManageMode) {
            this.isLoading = true;
            this.authenticationService
                .userInfo()
                .subscribe(userInfo => {
                    this.username = userInfo.username;
                    this.fetchLoginData()
                        .catch(() => {
                            this.error = "There was a problem fetching your login data";
                        });
                });
        } else {
            this.addLogins = this.allLogins;
        }

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

        this.microsoftApp = new UserAgentApplication(
            "4ecf3d26-e844-4855-9158-b8f6c0121b50",
            null,
            null);
    }

    public googleLogIn(): void {
        this.error = null;
        this.provider = "Google";

        // Need to wrap the promise since the promise returned from gapi doesn't seem to play well with Angular change detection.
        Promise.resolve(gapi.auth2.getAuthInstance().signIn())
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
        this.microsoftApp.loginPopup(["openid", "email"])
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

    public removeLogin(login: IExternalLogin): void {
        this.isLoading = true;
        this.userService
            .removeLogin(this.username, login)
            .then(() => this.fetchLoginData())
            .catch(() => {
                this.error = "There was a problem removing the login";
            });
    }

    private logIn(grantType: string, assertion: string): Promise<void> {
        // Save off for reuse in case the user has to select a username
        this.grantType = grantType;
        this.assertion = assertion;

        this.isLoading = true;
        return this.authenticationService.logInWithAssertion(grantType, assertion, this.isManageMode ? undefined : this.username)
            .then(() => {
                this.isLoading = false;
                if (this.isManageMode) {
                    return this.fetchLoginData();
                } else {
                    this.activeModal.close();
                    return Promise.resolve();
                }
            })
            .catch(error => {
                this.isLoading = false;

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

    private fetchLoginData(): Promise<void> {
        this.isLoading = true;
        return this.userService
            .getLogins(this.username)
            .then(logins => {
                this.isLoading = false;
                this.logins = logins;

                let loginProviderNames: { [name: string]: boolean } = {};
                for (let i = 0; i < this.logins.externalLogins.length; i++) {
                    loginProviderNames[this.logins.externalLogins[i].providerName] = true;
                }

                this.addLogins = [];
                for (let i = 0; i < this.allLogins.length; i++) {
                    if (!loginProviderNames[this.allLogins[i].name]) {
                        this.addLogins.push(this.allLogins[i]);
                    }
                }
            });
    }
}
