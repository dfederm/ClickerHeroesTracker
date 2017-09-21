import { Injectable } from "@angular/core";
import { Http, Headers, RequestOptions, URLSearchParams } from "@angular/http";
import { Observable } from "rxjs/Observable";
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import { Subscription } from "rxjs/Subscription";

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

@Injectable()
export class AuthenticationService {
    private static readonly tokensKey: string = "auth-tokens";

    private tokens: BehaviorSubject<IAuthTokenModel>;

    private refreshSubscription: Subscription;

    constructor(private http: Http) {
        let currentTokens = this.retrieveTokens();

        // If the current token is expired at startup, pretend like the user is not logged in until the refresh below happens.
        let isTokenValid = currentTokens && currentTokens.expiration_date > Date.now();
        this.tokens = new BehaviorSubject(isTokenValid ? currentTokens : null);

        // If the user was already logged in, always try and refresh the token.
        if (currentTokens) {
            this.refreshTokens(currentTokens)
                .then(() => this.scheduleRefresh())
                .catch(() => this.logOut());
        }
    }

    public logInWithPassword(username: string, password: string): Promise<void> {
        let params = new URLSearchParams();
        params.append("grant_type", "password");
        params.append("username", username);
        params.append("password", password);
        return this.getTokens(params)
            .then(() => this.scheduleRefresh());
    }

    public logInWithAssertion(grantType: string, assertion: string): Promise<void> {
        let params = new URLSearchParams();
        params.append("grant_type", grantType);
        params.append("assertion", assertion);
        return this.getTokens(params)
            .then(() => this.scheduleRefresh());
    }

    public logOut(): void {
        this.removeToken();
        this.tokens.next(null);

        if (this.refreshSubscription) {
            this.refreshSubscription.unsubscribe();
            this.refreshSubscription = null;
        }
    }

    public isLoggedIn(): Observable<boolean> {
        return this.tokens
            .map(tokens => tokens != null);
    }

    public getAuthHeaders(): Promise<Headers> {
        let currentTokens = this.tokens.getValue();
        if (!currentTokens) {
            return Promise.reject("NotLoggedIn");
        }

        let headers = new Headers();
        headers.append("Authorization", `${currentTokens.token_type} ${currentTokens.access_token}`);
        return Promise.resolve(headers);
    }

    private storeToken(tokens: IAuthTokenModel): void {
        localStorage.setItem(AuthenticationService.tokensKey, JSON.stringify(tokens));
    }

    private retrieveTokens(): IAuthTokenModel {
        const tokensString = localStorage.getItem(AuthenticationService.tokensKey);
        return tokensString == null ? null : JSON.parse(tokensString);
    }

    private removeToken(): void {
        localStorage.removeItem(AuthenticationService.tokensKey);
    }

    private getTokens(params: URLSearchParams): Promise<void> {
        let headers = new Headers({ "Content-Type": "application/x-www-form-urlencoded" });
        let options = new RequestOptions({ headers });
        params.append("scope", "openid offline_access");
        return this.http.post("/api/auth/token", params.toString(), options)
            .toPromise()
            .then(response => {
                let newTokens: IAuthTokenModel = response.json();
                if (!newTokens
                    || !newTokens.token_type
                    || !newTokens.access_token) {
                    return Promise.reject("Invalid token response");
                }

                newTokens.expiration_date = Date.now() + newTokens.expires_in * 1000;

                this.storeToken(newTokens);
                this.tokens.next(newTokens);
                return Promise.resolve();
            });
    }

    private refreshTokens(currentTokens: IAuthTokenModel): Promise<void> {
        let params = new URLSearchParams();
        params.append("grant_type", "refresh_token");
        params.append("refresh_token", currentTokens.refresh_token);
        return this.getTokens(params);
    }

    private scheduleRefresh(): void {
        // Refresh every half the total expiration time. This assumes the expire duration doesn't change over time.
        this.refreshSubscription = Observable.interval(this.tokens.getValue().expires_in / 2 * 1000)
            // Make sure to get the current tokens when refreshing, not the ones from above
            .map(() => this.refreshTokens(this.tokens.getValue()))
            .subscribe();
    }
}
