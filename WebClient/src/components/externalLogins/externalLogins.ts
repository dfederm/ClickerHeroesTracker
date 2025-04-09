import { Component, OnInit, Input, AfterViewInit, NgZone } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { PublicClientApplication } from "@azure/msal-browser";

import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { UserService, IUserLogins, IExternalLogin } from "../../services/userService/userService";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";
import { FormsModule } from "@angular/forms";
import { ValidateEqualModule } from "ng-validate-equal";

export interface IErrorResponse {
    error: string;
    error_description?: string;
}

@Component({
    selector: "externalLogins",
    templateUrl: "./externalLogins.html",
    imports: [
        FormsModule,
        NgxSpinnerModule,
        ValidateEqualModule,
    ]
})
export class ExternalLoginsComponent implements OnInit, AfterViewInit {
    public static facebookInitialized = false;

    public microsoftApp: PublicClientApplication;

    @Input()
    public isManageMode: boolean;

    public logins: IUserLogins;

    public error: string;

    public needUsername: boolean;

    public provider: string;

    public username: string;

    public addLogins: string[];

    private grantType: string;

    private assertion: string;

    private readonly allLogins: string[] = [
        "Google",
        "Facebook",
        "Microsoft",
    ];

    constructor(
        private readonly authenticationService: AuthenticationService,
        public activeModal: NgbActiveModal,
        private readonly userService: UserService,
        private readonly spinnerService: NgxSpinnerService,
        private readonly zone: NgZone
    ) { }

    public ngOnInit(): void {
        if (this.isManageMode) {
            this.spinnerService.show("externalLogins");
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

        if (typeof (google) !== "undefined") {
            google.accounts.id.initialize({
                client_id: "371697338749-cbgs417cd45vgktq0kmjanbn3lh2lbl6.apps.googleusercontent.com",
                callback: ({ credential }) => {
                    // This callback is not covered by Angular so we have to get it back into Angular
                    this.zone.run(() => {
                        this.error = null;
                        this.provider = "Google";

                        this.logIn("urn:ietf:params:oauth:grant-type:google_identity_token", credential)
                            .catch(() => {
                                this.error = "There was a problem logging in with Google";
                            });
                    });
                },
            });
        }

        // eslint-disable-next-line
        if (!ExternalLoginsComponent.facebookInitialized && typeof (FB) !== "undefined") {
            FB.init({
                appId: "246885142330300",
                version: "v16.0",
            });
            ExternalLoginsComponent.facebookInitialized = true;
        }

        this.microsoftApp = new PublicClientApplication({ auth: { clientId: "4ecf3d26-e844-4855-9158-b8f6c0121b50" } });
    }

    public ngAfterViewInit(): void {
        if (google?.accounts?.id?.renderButton) {
            let googleButton = document.getElementById("google-signin-button");
            if (googleButton) {
                google.accounts.id.renderButton(googleButton, {
                    type: "standard",
                    size: "large",
                });
            }
        }
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
        this.microsoftApp.initialize()
            .then(() => this.microsoftApp.loginPopup({ scopes: ["openid", "email"] }))
            .then(loginResponse => this.logIn("urn:ietf:params:oauth:grant-type:microsoft_identity_token", loginResponse.idToken))
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
        this.spinnerService.show("externalLogins");
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

        this.spinnerService.show("externalLogins");
        return this.authenticationService.logInWithAssertion(grantType, assertion, this.isManageMode ? undefined : this.username)
            .then(() => {
                if (this.isManageMode) {
                    return this.fetchLoginData();
                }

                this.activeModal.close();
                return Promise.resolve();
            })
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
            })
            .finally(() => {
                this.spinnerService.hide("externalLogins");
            });
    }

    private fetchLoginData(): Promise<void> {
        this.spinnerService.show("externalLogins");
        return this.userService
            .getLogins(this.username)
            .then(logins => {
                this.logins = logins;

                let loginProviderNames: { [name: string]: boolean } = {};
                for (let i = 0; i < this.logins.externalLogins.length; i++) {
                    loginProviderNames[this.logins.externalLogins[i].providerName] = true;
                }

                this.addLogins = [];
                for (let i = 0; i < this.allLogins.length; i++) {
                    if (!loginProviderNames[this.allLogins[i]]) {
                        this.addLogins.push(this.allLogins[i]);
                    }
                }
            })
            .finally(() => {
                this.spinnerService.hide("externalLogins");
            });
    }
}
