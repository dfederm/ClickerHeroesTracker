import { ReflectiveInjector } from "@angular/core";
import { fakeAsync, tick, discardPeriodicTasks } from "@angular/core/testing";
import { BaseRequestOptions, ConnectionBackend, Http, RequestOptions } from "@angular/http";
import { Response, ResponseOptions, RequestMethod } from "@angular/http";
import { MockBackend, MockConnection } from "@angular/http/testing";

import { AuthenticationService, IAuthTokenModel } from "./authenticationService";

describe("AuthenticationService", () =>
{
    let injector: ReflectiveInjector;
    let backend: MockBackend;
    let lastConnection: MockConnection = null;

    beforeEach(() =>
    {
        injector = ReflectiveInjector.resolveAndCreate(
            [
                { provide: ConnectionBackend, useClass: MockBackend },
                { provide: RequestOptions, useClass: BaseRequestOptions },
                Http,
                AuthenticationService,
            ]);

        spyOn(localStorage, "getItem");
        spyOn(localStorage, "setItem");
        spyOn(localStorage, "removeItem");

        backend = injector.get(ConnectionBackend) as MockBackend;
        backend.connections.subscribe((connection: MockConnection) => lastConnection = connection);
    });

    afterEach(() =>
    {
        lastConnection = null;
        backend.verifyNoPendingRequests();
    });

    describe("initialization", () =>
    {
        it("should not be logged in initially when local storage is empty", fakeAsync(() =>
        {
            (localStorage.getItem as jasmine.Spy).and.returnValue(null);

            let authenticationService = injector.get(AuthenticationService) as AuthenticationService;

            authenticationService.isLoggedIn()
                .subscribe(isLoggedIn => expect(isLoggedIn).toEqual(false));
            tick();

            expect(localStorage.getItem).toHaveBeenCalledWith("auth-tokens");
            expect(localStorage.setItem).not.toHaveBeenCalled();
            expect(lastConnection).toBeNull();
        }));

        it("should be logged in initially and refresh the token when local storage is populated with a valid token", fakeAsync(() =>
        {
            let tokens = createCachedAuthModel();
            (localStorage.getItem as jasmine.Spy).and.returnValue(JSON.stringify(tokens));

            let authenticationService = injector.get(AuthenticationService) as AuthenticationService;

            expect(lastConnection).not.toBeNull("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/auth/token", "url invalid");
            expect(lastConnection.request.text()).toEqual("grant_type=refresh_token&refresh_token=someRefreshToken&scope=openid%20offline_access", "request body invalid");

            let isLoggedIn = false;
            authenticationService.isLoggedIn().subscribe(_ => isLoggedIn = _);

            // Immediately logged in
            tick();
            expect(isLoggedIn).toEqual(true);

            let newTokens = createResponseAuthModel();
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(newTokens) })));
            newTokens.expiration_date = new Date().getTime() + newTokens.expires_in * 1000;

            // Still logged in after the token refresh
            tick();
            expect(isLoggedIn).toEqual(true);

            expect(localStorage.getItem).toHaveBeenCalledWith("auth-tokens");
            expect(localStorage.setItem).toHaveBeenCalledWith("auth-tokens", JSON.stringify(newTokens));

            // An interval was started, so abandon it.
            discardPeriodicTasks();
        }));

        it("should be logged out initially but logged in eventually once the token is refreshed when local storage is populated with an expired token", fakeAsync(() =>
        {
            let tokens = createCachedAuthModel();
            tokens.expiration_date = 0;
            (localStorage.getItem as jasmine.Spy).and.returnValue(JSON.stringify(tokens));

            let authenticationService = injector.get(AuthenticationService) as AuthenticationService;

            expect(lastConnection).not.toBeNull("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/auth/token", "url invalid");
            expect(lastConnection.request.text()).toEqual("grant_type=refresh_token&refresh_token=someRefreshToken&scope=openid%20offline_access", "request body invalid");

            let isLoggedIn = false;
            authenticationService.isLoggedIn().subscribe(_ => isLoggedIn = _);

            // Immediately not logged in
            tick();
            expect(isLoggedIn).toEqual(false);

            let newTokens = createResponseAuthModel();
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(newTokens) })));
            newTokens.expiration_date = new Date().getTime() + newTokens.expires_in * 1000;

            // Logged in after the token refresh
            tick();
            expect(isLoggedIn).toEqual(true);

            expect(localStorage.getItem).toHaveBeenCalledWith("auth-tokens");
            expect(localStorage.setItem).toHaveBeenCalledWith("auth-tokens", JSON.stringify(newTokens));

            // An interval was started, so abandon it.
            discardPeriodicTasks();
        }));
    });

    describe("getAuthHeaders", () =>
    {
        it("should reject when not logged in", fakeAsync(() =>
        {
            (localStorage.getItem as jasmine.Spy).and.returnValue(null);

            let authenticationService = injector.get(AuthenticationService) as AuthenticationService;
            authenticationService.getAuthHeaders()
                .then(fail)
                .catch(error =>
                {
                    expect(error).toEqual("NotLoggedIn");
                });
            tick();
        }));

        it("should get auth headers when logged in", fakeAsync(() =>
        {
            let tokens = createCachedAuthModel();
            (localStorage.getItem as jasmine.Spy).and.returnValue(JSON.stringify(tokens));

            let authenticationService = injector.get(AuthenticationService) as AuthenticationService;
            authenticationService.getAuthHeaders()
                .then(headers =>
                {
                    expect(headers.keys().length).toEqual(1);
                    expect(headers.get("Authorization")).toEqual("someTokenType someAccessToken");
                });
            tick();
        }));
    });

    describe("logIn", () =>
    {
        it("should reject with an unsuccessful log in", fakeAsync(() =>
        {
            let logInSuccessful = false;
            let error: Error;
            let isLoggedInLog: boolean[] = [];

            let authenticationService = injector.get(AuthenticationService) as AuthenticationService;
            authenticationService.isLoggedIn()
                .subscribe(isLoggedIn =>
                {
                    isLoggedInLog.push(isLoggedIn);
                });
            authenticationService.logIn("someUsername", "somePassword")
                .then(() => logInSuccessful = true)
                .catch(e => error = e);

            expect(lastConnection).not.toBeNull("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/auth/token", "url invalid");
            expect(lastConnection.request.text()).toEqual("grant_type=password&username=someUsername&password=somePassword&scope=openid%20offline_access", "request body invalid");

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(logInSuccessful).toEqual(false);
            expect(localStorage.setItem).not.toHaveBeenCalled();
            expect(error).toBeDefined();
            expect(isLoggedInLog).toEqual([false]);
        }));

        it("should resolve with a successful log in and refresh the new token", fakeAsync(() =>
        {
            let logInSuccessful = false;
            let error: Error;
            let isLoggedInLog: boolean[] = [];

            let authenticationService = injector.get(AuthenticationService) as AuthenticationService;
            authenticationService.isLoggedIn()
                .subscribe(isLoggedIn =>
                {
                    isLoggedInLog.push(isLoggedIn);
                });
            authenticationService.logIn("someUsername", "somePassword")
                .then(() => logInSuccessful = true)
                .catch(e => error = e);

            expect(lastConnection).not.toBeNull("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/auth/token", "url invalid");
            expect(lastConnection.request.text()).toEqual("grant_type=password&username=someUsername&password=somePassword&scope=openid%20offline_access", "request body invalid");

            let tokens = createResponseAuthModel(1);
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(tokens) })));
            lastConnection = null;
            tokens.expiration_date = new Date().getTime() + tokens.expires_in * 1000;

            // Don't tick enough to trigger the refresh interval
            tick(1);

            expect(logInSuccessful).toEqual(true);
            expect(localStorage.setItem).toHaveBeenCalledWith("auth-tokens", JSON.stringify(tokens));
            expect(error).toBeUndefined();
            expect(isLoggedInLog).toEqual([false, true]);

            (localStorage.setItem as jasmine.Spy).calls.reset();

            // Let the refresh interval tick
            tick(tokens.expires_in / 2 * 1000);

            expect(lastConnection).not.toBeNull("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/auth/token", "url invalid");
            expect(lastConnection.request.text()).toEqual("grant_type=refresh_token&refresh_token=someNewRefreshToken1&scope=openid%20offline_access", "request body invalid");

            let refreshedTokens = createResponseAuthModel(2);
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(refreshedTokens) })));
            refreshedTokens.expiration_date = new Date().getTime() + refreshedTokens.expires_in * 1000;
            tick();

            expect(localStorage.setItem).toHaveBeenCalledWith("auth-tokens", JSON.stringify(refreshedTokens));
            expect(error).toBeUndefined();
            expect(isLoggedInLog).toEqual([false, true, true]);

            // An interval was started, so abandon it.
            discardPeriodicTasks();
        }));
    });

    describe("logOut", () =>
    {
        it("should successfully log out", fakeAsync(() =>
        {
            let isLoggedInLog: boolean[] = [];

            let tokens = createCachedAuthModel();
            (localStorage.getItem as jasmine.Spy).and.returnValue(JSON.stringify(tokens));

            let authenticationService = injector.get(AuthenticationService) as AuthenticationService;
            authenticationService.isLoggedIn()
                .subscribe(isLoggedIn =>
                {
                    isLoggedInLog.push(isLoggedIn);
                });
            authenticationService.logOut();

            expect(localStorage.removeItem).toHaveBeenCalledWith("auth-tokens");
            expect(isLoggedInLog).toEqual([true, false]);
        }));

        it("should stop the token refresh interval after logging out", fakeAsync(() =>
        {
            let isLoggedInLog: boolean[] = [];

            let tokens = createCachedAuthModel();
            (localStorage.getItem as jasmine.Spy).and.returnValue(JSON.stringify(tokens));

            let authenticationService = injector.get(AuthenticationService) as AuthenticationService;
            authenticationService.isLoggedIn()
                .subscribe(isLoggedIn =>
                {
                    isLoggedInLog.push(isLoggedIn);
                });

            // Let the refresh interval tick
            tick(tokens.expires_in / 2 * 1000);

            expect(lastConnection).not.toBeNull("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/auth/token", "url invalid");
            expect(lastConnection.request.text()).toEqual("grant_type=refresh_token&refresh_token=someRefreshToken&scope=openid%20offline_access", "request body invalid");

            let refreshedTokens = createResponseAuthModel(2);
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(refreshedTokens) })));
            lastConnection = null;
            refreshedTokens.expiration_date = new Date().getTime() + refreshedTokens.expires_in * 1000;
            tick();

            expect(isLoggedInLog).toEqual([true, true]);
            expect(localStorage.setItem).toHaveBeenCalledWith("auth-tokens", JSON.stringify(refreshedTokens));
            (localStorage.setItem as jasmine.Spy).calls.reset();

            authenticationService.logOut();

            expect(localStorage.removeItem).toHaveBeenCalledWith("auth-tokens");
            expect(isLoggedInLog).toEqual([true, true, false]);

            // Let the refresh interval tick again
            tick(tokens.expires_in / 2 * 1000);

            // And nothing changed
            expect(lastConnection).toBeNull();
            expect(localStorage.setItem).not.toHaveBeenCalled();
            expect(isLoggedInLog).toEqual([true, true, false]);
        }));
    });

    function createCachedAuthModel(): IAuthTokenModel
    {
        return {
            token_type: "someTokenType",
            access_token: "someAccessToken",
            refresh_token: "someRefreshToken",
            id_token: "someIdToken",
            expires_in: 3600,
            expiration_date: new Date().getTime() + 3600,
        };
    }

    function createResponseAuthModel(num?: number): IAuthTokenModel
    {
        return {
            token_type: "someNewTokenType" + (num === undefined ? "" : num.toString()),
            access_token: "someNewAccessToken" + (num === undefined ? "" : num.toString()),
            refresh_token: "someNewRefreshToken" + (num === undefined ? "" : num.toString()),
            id_token: "someNewIdToken" + (num === undefined ? "" : num.toString()),
            expires_in: 3600,
        };
    }
});
