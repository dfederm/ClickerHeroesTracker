import { Injectable } from "@angular/core";
import { Http, Headers, RequestOptions, URLSearchParams } from "@angular/http";
import { Observable } from "rxjs/Observable";
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import { Subscription } from "rxjs/Subscription";
import * as JwtDecode from "jwt-decode";

import "rxjs/add/operator/toPromise";
import "rxjs/add/operator/map";
import "rxjs/add/observable/interval";

export interface IAuthTokenModel {
    access_token: string;
    refresh_token: string;
    id_token: string;
    expires_in: number;
    token_type: string;
    expiration_date?: number;
}

export interface IUserInfo {
    isLoggedIn: boolean;
    id?: string;
    username?: string;
    email?: string;
    isAdmin?: boolean;
}

@Injectable()
export class AuthenticationService {
    private static readonly tokensKey: string = "auth-tokens";

    private currentTokens: IAuthTokenModel;

    private userInfoSubject: BehaviorSubject<IUserInfo>;

    private refreshSubscription: Subscription;

    constructor(private http: Http) {
        let tokensString = localStorage.getItem(AuthenticationService.tokensKey);
        this.currentTokens = tokensString == null ? null : JSON.parse(tokensString);
        this.userInfoSubject = new BehaviorSubject(this.getUserInfo());

        // If the user was already logged in, always try and refresh the token.
        if (this.currentTokens) {
            this.refreshTokens()
                .then(() => this.scheduleRefresh())
                .catch(() => this.logOut());
        }
    }

    public logInWithPassword(username: string, password: string): Promise<void> {
        let params = new URLSearchParams();
        params.append("grant_type", "password");
        params.append("username", username);
        params.append("password", password);
        return this.fetchTokens(params)
            .then(() => this.scheduleRefresh());
    }

    public logInWithAssertion(grantType: string, assertion: string, username?: string): Promise<void> {
        let params = new URLSearchParams();
        params.append("grant_type", grantType);
        params.append("assertion", assertion);

        if (username) {
            params.append("username", username);
        }

        return this.fetchTokens(params)
            .then(() => this.scheduleRefresh());
    }

    public logOut(): void {
        localStorage.removeItem(AuthenticationService.tokensKey);
        this.currentTokens = null;
        this.userInfoSubject.next(this.getUserInfo());

        if (this.refreshSubscription) {
            this.refreshSubscription.unsubscribe();
            this.refreshSubscription = null;
        }
    }

    public userInfo(): Observable<IUserInfo> {
        return this.userInfoSubject;
    }

    public getAuthHeaders(): Headers {
        let headers = new Headers();

        if (this.currentTokens) {
            headers.append("Authorization", `${this.currentTokens.token_type} ${this.currentTokens.access_token}`);
        }

        return headers;
    }

    private fetchTokens(params: URLSearchParams): Promise<void> {
        let headers = this.getAuthHeaders();
        headers.append("Content-Type", "application/x-www-form-urlencoded");

        let options = new RequestOptions({ headers });
        params.append("scope", "openid offline_access profile email roles");
        return this.http.post("/api/auth/token", params.toString(), options)
            .toPromise()
            .then(response => {
                let tokens: IAuthTokenModel = response.json();
                if (!tokens
                    || !tokens.token_type
                    || !tokens.access_token
                    || !tokens.id_token) {
                    return Promise.reject("Invalid token response");
                }

                tokens.expiration_date = Date.now() + tokens.expires_in * 1000;

                localStorage.setItem(AuthenticationService.tokensKey, JSON.stringify(tokens));
                this.currentTokens = Object.assign(this.currentTokens || {}, tokens); // Merge them in in case a new refresh token was not issued
                this.userInfoSubject.next(this.getUserInfo());

                return Promise.resolve();
            });
    }

    private refreshTokens(): Promise<void> {
        let params = new URLSearchParams();
        params.append("grant_type", "refresh_token");
        params.append("refresh_token", this.currentTokens.refresh_token);
        return this.fetchTokens(params);
    }

    private scheduleRefresh(): void {
        // Refresh every half the total expiration time. This assumes the expire duration doesn't change over time.
        this.refreshSubscription = Observable.interval(this.currentTokens.expires_in / 2 * 1000)
            .map(() => this.refreshTokens())
            .subscribe();
    }

    private getUserInfo(): IUserInfo {
        if (this.currentTokens
            && this.currentTokens.id_token
            && this.currentTokens.expiration_date > Date.now()
        ) {
            let claims: { [claim: string]: string } = JwtDecode(this.currentTokens.id_token);
            return {
                isLoggedIn: true,
                id: claims.sub,
                username: claims.name,
                email: claims.email,
                isAdmin: claims.role === "Admin",
            };
        } else {
            return {
                isLoggedIn: false,
            };
        }
    }
}
