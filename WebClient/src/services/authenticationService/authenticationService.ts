import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders, HttpParams, HttpErrorResponse } from "@angular/common/http";
import { Observable, BehaviorSubject, Subscription, interval } from "rxjs";
import jwt_decode from "jwt-decode";
import { map, distinctUntilChanged } from "rxjs/operators";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";

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

@Injectable({
    providedIn: "root",
})
export class AuthenticationService {
    private static readonly tokensKey: string = "auth-tokens";

    private currentTokens: IAuthTokenModel;

    private readonly userInfoSubject: BehaviorSubject<IUserInfo>;

    private refreshSubscription: Subscription;

    // This promise is used effectively like a lock.
    // It should always be assigned just before making a token request and resolve after the request returns.
    private fetchTokensPromise: Promise<void>;

    constructor(
        private readonly httpErrorHandlerService: HttpErrorHandlerService,
        private readonly http: HttpClient,
    ) {
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
        let params = new HttpParams()
            .set("grant_type", "password")
            .set("username", username)
            .set("password", password);
        return this.fetchTokens(params)
            .then(() => this.scheduleRefresh());
    }

    public logInWithAssertion(grantType: string, assertion: string, username: string): Promise<void> {
        let params = new HttpParams()
            .set("grant_type", grantType)
            .set("assertion", assertion);

        if (username) {
            params = params.set("username", username);
            return this.fetchTokens(params)
                .then(() => this.scheduleRefresh());
        }

        // Get the auth headers in case it's an existing user adding an external login
        return this.getAuthHeaders()
            .then(headers => this.fetchTokens(params, headers))
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
        return this.userInfoSubject.pipe(
            distinctUntilChanged((x, y) => JSON.stringify(x) === JSON.stringify(y)),
        );
    }

    public getAuthHeaders(): Promise<HttpHeaders> {
        if (this.fetchTokensPromise) {
            return this.fetchTokensPromise
                .catch(() => void 0) // Swallow errors as we just use this effectively like a lock
                .then(() => {
                    let headers = new HttpHeaders();

                    if (this.currentTokens) {
                        headers = headers.set("Authorization", `${this.currentTokens.token_type} ${this.currentTokens.access_token}`);
                    }

                    return headers;
                });
        }

        return Promise.resolve(new HttpHeaders());
    }

    private fetchTokens(params: HttpParams, headers?: HttpHeaders): Promise<void> {
        if (!headers) {
            headers = new HttpHeaders();
        }
        headers = headers.set("Content-Type", "application/x-www-form-urlencoded");

        params = params.set("scope", "openid offline_access profile email roles");

        // Angular doesn't encode '+' correctly. See: https://github.com/angular/angular/issues/11058
        let body = params.toString().replace(/\+/gi, "%2B");

        this.fetchTokensPromise = this.http.post<IAuthTokenModel>("/api/auth/token", body, { headers })
            .toPromise()
            .then(tokens => {
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
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("AuthenticationService.fetchTokens.error", err);
                return Promise.reject(err);
            });
        return this.fetchTokensPromise;
    }

    private refreshTokens(): Promise<void> {
        let params = new HttpParams()
            .set("grant_type", "refresh_token")
            .set("refresh_token", this.currentTokens.refresh_token);
        return this.fetchTokens(params);
    }

    private scheduleRefresh(): void {
        // Refresh every half the total expiration time. This assumes the expire duration doesn't change over time.
        this.refreshSubscription = interval(this.currentTokens.expires_in / 2 * 1000).pipe(
            map(() => this.refreshTokens()),
        ).subscribe();
    }

    private getUserInfo(): IUserInfo {
        if (this.currentTokens
            && this.currentTokens.id_token
            && this.currentTokens.expiration_date > Date.now()
        ) {
            let claims: { [claim: string]: string } = jwt_decode(this.currentTokens.id_token);
            return {
                isLoggedIn: true,
                id: claims.sub,
                username: claims.name,
                email: claims.email,
                isAdmin: claims.role === "Admin",
            };
        }

        return {
            isLoggedIn: false,
        };
    }
}
