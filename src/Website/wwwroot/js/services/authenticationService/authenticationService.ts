import { Injectable } from "@angular/core";
import { Http, Headers, RequestOptions, URLSearchParams } from "@angular/http";
import { Observable } from "rxjs/Observable";
import { BehaviorSubject } from "rxjs/BehaviorSubject";

import "rxjs/add/operator/toPromise";

interface IAuthTokenModel {
    access_token: string;
    refresh_token: string;
    id_token: string;
    expires_in: number;
    token_type: string;
    expiration_date: string;
}

@Injectable()
export class AuthenticationService
{
    private static readonly tokensKey: string = "auth-tokens";

    private loggedInState: BehaviorSubject<boolean>;

    constructor(private http: Http)
    {
        let tokens = this.retrieveTokens();
        this.loggedInState = new BehaviorSubject(tokens != null);
    }

    public logIn(username: string, password: string): Promise<void>
    {
        let headers = new Headers({ "Content-Type": "application/x-www-form-urlencoded" });
        let options = new RequestOptions({ headers: headers });
        let params = new URLSearchParams();
        params.append("grant_type", "password");
        params.append("username", username);
        params.append("password", password);
        return this.http.post("/api/auth/token", params.toString(), options)
            .toPromise()
            .then(response =>
            {
                let tokens: IAuthTokenModel = response.json();
                if (!tokens
                    || !tokens.token_type
                    || !tokens.access_token)
                {
                    return Promise.reject("");
                }

                this.storeToken(tokens);
                this.loggedInState.next(true);
                return Promise.resolve();
            });
    }

    public logOut(): void
    {
        this.removeToken();
        this.loggedInState.next(false);
    }

    public isLoggedIn(): Observable<boolean>
    {
        return this.loggedInState;
    }

    public getAuthHeaders(): Promise<Headers>
    {
        if (!this.loggedInState.value)
        {
            return Promise.reject("NotLoggedIn");
        }

        let tokens = this.retrieveTokens();

        let headers = new Headers();
        headers.append("Authorization", `${tokens.token_type} ${tokens.access_token}`);
        return Promise.resolve(headers);
    }

    private storeToken(tokens: IAuthTokenModel): void
    {
        localStorage.setItem(AuthenticationService.tokensKey, JSON.stringify(tokens));
    }

    private retrieveTokens(): IAuthTokenModel
    {
        const tokensString = localStorage.getItem(AuthenticationService.tokensKey);
        return tokensString == null ? null : JSON.parse(tokensString);
    }

    private removeToken(): void
    {
        localStorage.removeItem(AuthenticationService.tokensKey);
    }
}
