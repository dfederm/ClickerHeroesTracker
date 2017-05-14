import { ReflectiveInjector } from "@angular/core";
import { fakeAsync, tick } from "@angular/core/testing";
import { BaseRequestOptions, ConnectionBackend, Http, RequestOptions } from "@angular/http";
import { Response, ResponseOptions } from "@angular/http";
import { MockBackend, MockConnection } from "@angular/http/testing";

import { AuthenticationService } from "./authenticationService";

describe("AuthenticationService", () =>
{
    let injector: ReflectiveInjector;
    let backend: MockBackend;
    let lastConnection: MockConnection;

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
        }));

        it("should be logged in initially when local storage is populated", fakeAsync(() =>
        {
            let tokens = { token_type: "someTokenType", access_token: "someAccessToken"};
            (localStorage.getItem as jasmine.Spy).and.returnValue(JSON.stringify(tokens));

            let authenticationService = injector.get(AuthenticationService) as AuthenticationService;
            authenticationService.isLoggedIn()
                .subscribe(isLoggedIn => expect(isLoggedIn).toEqual(true));
            tick();

            expect(localStorage.getItem).toHaveBeenCalledWith("auth-tokens");
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
            let tokens = { token_type: "someTokenType", access_token: "someAccessToken"};
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
            let loggedIn = false;
            let error: Error;
            let isLoggedInLog: boolean[] = [];

            let authenticationService = injector.get(AuthenticationService) as AuthenticationService;
            authenticationService.isLoggedIn()
                .subscribe(isLoggedIn =>
                {
                    isLoggedInLog.push(isLoggedIn);
                });
            authenticationService.logIn("someUsername", "somePassword")
                .then(() => loggedIn = true)
                .catch(e => error = e);

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(loggedIn).toEqual(false);
            expect(localStorage.setItem).not.toHaveBeenCalled();
            expect(error).toBeDefined();
            expect(isLoggedInLog).toEqual([false]);
        }));

        it("should resolve with a successful log in", fakeAsync(() =>
        {
            let loggedIn = false;
            let error: Error;
            let isLoggedInLog: boolean[] = [];

            let authenticationService = injector.get(AuthenticationService) as AuthenticationService;
            authenticationService.isLoggedIn()
                .subscribe(isLoggedIn =>
                {
                    isLoggedInLog.push(isLoggedIn);
                });
            authenticationService.logIn("someUsername", "somePassword")
                .then(() => loggedIn = true)
                .catch(e => error = e);

            let tokens = { token_type: "someTokenType", access_token: "someAccessToken"};
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(tokens) })));
            tick();

            expect(loggedIn).toEqual(true);
            expect(localStorage.setItem).toHaveBeenCalledWith("auth-tokens", JSON.stringify(tokens));
            expect(error).toBeUndefined();
            expect(isLoggedInLog).toEqual([false, true]);
        }));
    });

    describe("logOut", () =>
    {
        it("should successfully log out", fakeAsync(() =>
        {
            let isLoggedInLog: boolean[] = [];

            let tokens = { token_type: "someTokenType", access_token: "someAccessToken"};
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
    });
});
