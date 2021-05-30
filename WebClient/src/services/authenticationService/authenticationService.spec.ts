import { TestBed, fakeAsync, tick, discardPeriodicTasks } from "@angular/core/testing";
import { HttpClientTestingModule, HttpTestingController } from "@angular/common/http/testing";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";

import { AuthenticationService, IAuthTokenModel, IUserInfo } from "./authenticationService";

describe("AuthenticationService", () => {
    const tokenRequest = { method: "post", url: "/api/auth/token" };

    let httpMock: HttpTestingController;

    const notLoggedInUser: IUserInfo = {
        isLoggedIn: false,
    };

    beforeEach(() => {
        let httpErrorHandlerService = {
            logError: (): void => void 0,
        };

        TestBed.configureTestingModule(
            {
                imports: [
                    HttpClientTestingModule,
                ],
                providers:
                    [
                        AuthenticationService,
                        { provide: HttpErrorHandlerService, useValue: httpErrorHandlerService },
                    ],
            });

        spyOn(localStorage, "getItem");
        spyOn(localStorage, "setItem");
        spyOn(localStorage, "removeItem");

        let now = Date.now();
        spyOn(Date, "now").and.callFake(() => now);

        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    describe("initialization", () => {
        it("should not be logged in initially when local storage is empty", fakeAsync(() => {
            (localStorage.getItem as jasmine.Spy).and.returnValue(null);

            let authenticationService = TestBed.inject(AuthenticationService);

            authenticationService.userInfo()
                .subscribe(userInfo => expect(userInfo).toEqual(notLoggedInUser));
            tick();

            expect(localStorage.getItem).toHaveBeenCalledWith("auth-tokens");
            expect(localStorage.setItem).not.toHaveBeenCalled();
        }));

        it("should be logged in initially and refresh the token when local storage is populated with a valid token", fakeAsync(() => {
            let tokens = createCachedAuthModel();
            (localStorage.getItem as jasmine.Spy).and.returnValue(JSON.stringify(tokens));
            let expectedUserInfo = createResponseUserInfo();

            let authenticationService = TestBed.inject(AuthenticationService);
            tick();

            let request = httpMock.expectOne(tokenRequest);
            expect(request.request.body).toEqual("grant_type=refresh_token&refresh_token=someRefreshToken&scope=openid%20offline_access%20profile%20email%20roles");

            let actualUserInfo: IUserInfo;
            authenticationService.userInfo().subscribe(_ => actualUserInfo = _);

            // Immediately logged in
            tick();
            expect(actualUserInfo).toEqual(createCachedUserInfo());

            let newTokens = createResponseAuthModel();
            request.flush(newTokens);
            newTokens.expiration_date = Date.now() + newTokens.expires_in * 1000;

            // Still logged in after the token refresh
            tick();
            expect(actualUserInfo).toEqual(expectedUserInfo);

            expect(localStorage.getItem).toHaveBeenCalledWith("auth-tokens");
            expect(localStorage.setItem).toHaveBeenCalledWith("auth-tokens", JSON.stringify(newTokens));

            // An interval was started, so abandon it.
            discardPeriodicTasks();
        }));

        it("should be logged out initially but logged in eventually once the token is refreshed when local storage is populated with an expired token", fakeAsync(() => {
            let tokens = createCachedAuthModel();
            tokens.expiration_date = 0;
            (localStorage.getItem as jasmine.Spy).and.returnValue(JSON.stringify(tokens));
            let expectedUserInfo = createResponseUserInfo();

            let authenticationService = TestBed.inject(AuthenticationService);
            tick();

            let request = httpMock.expectOne(tokenRequest);
            expect(request.request.body).toEqual("grant_type=refresh_token&refresh_token=someRefreshToken&scope=openid%20offline_access%20profile%20email%20roles");

            let actualUserInfo: IUserInfo;
            authenticationService.userInfo().subscribe(_ => actualUserInfo = _);

            // Immediately not logged in
            tick();
            expect(actualUserInfo).toEqual(notLoggedInUser);

            let newTokens = createResponseAuthModel();
            request.flush(newTokens);
            newTokens.expiration_date = Date.now() + newTokens.expires_in * 1000;

            // Logged in after the token refresh
            tick();
            expect(actualUserInfo).toEqual(expectedUserInfo);

            expect(localStorage.getItem).toHaveBeenCalledWith("auth-tokens");
            expect(localStorage.setItem).toHaveBeenCalledWith("auth-tokens", JSON.stringify(newTokens));

            // An interval was started, so abandon it.
            discardPeriodicTasks();
        }));
    });

    describe("getAuthHeaders", () => {
        it("should return empty headers when not logged in", async () => {
            (localStorage.getItem as jasmine.Spy).and.returnValue(null);

            let authenticationService = TestBed.inject(AuthenticationService);
            const headers = await authenticationService.getAuthHeaders();
            expect(headers.keys().length).toEqual(0);
        });

        it("should get auth headers when logged in", async () => {
            let tokens = createCachedAuthModel();
            (localStorage.getItem as jasmine.Spy).and.returnValue(JSON.stringify(tokens));

            let authenticationService = TestBed.inject(AuthenticationService);

            // It tries to refresh initially, and the headers will be blocked until the refresh responds.
            let request = httpMock.expectOne(tokenRequest);
            request.flush(tokens);

            const headers = await authenticationService.getAuthHeaders();
            expect(headers.keys().length).toEqual(1);
            expect(headers.get("Authorization")).toEqual("someTokenType someAccessToken");
        });
    });

    describe("logInWithPassword", () => {
        it("should reject with an unsuccessful log in", fakeAsync(() => {
            let logInSuccessful = false;
            let error: Error;
            let userInfoLog: IUserInfo[] = [];

            let authenticationService = TestBed.inject(AuthenticationService);
            authenticationService.userInfo()
                .subscribe(userInfo => {
                    userInfoLog.push(userInfo);
                });
            authenticationService.logInWithPassword("someUsername", "somePassword")
                .then(() => logInSuccessful = true)
                .catch(e => error = e);

            let request = httpMock.expectOne(tokenRequest);
            expect(request.request.body).toEqual("grant_type=password&username=someUsername&password=somePassword&scope=openid%20offline_access%20profile%20email%20roles");

            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(logInSuccessful).toEqual(false);
            expect(localStorage.setItem).not.toHaveBeenCalled();
            expect(error).toBeDefined();
            expect(userInfoLog).toEqual([notLoggedInUser]);
        }));

        it("should resolve with a successful log in and refresh the new token", fakeAsync(() => {
            let logInSuccessful = false;
            let error: Error;
            let userInfoLog: IUserInfo[] = [];

            let authenticationService = TestBed.inject(AuthenticationService);
            authenticationService.userInfo()
                .subscribe(_ => userInfoLog.push(_));
            authenticationService.logInWithPassword("someUsername", "somePassword")
                .then(() => logInSuccessful = true)
                .catch(e => error = e);

            let request = httpMock.expectOne(tokenRequest);
            expect(request.request.body).toEqual("grant_type=password&username=someUsername&password=somePassword&scope=openid%20offline_access%20profile%20email%20roles");

            let tokens = createResponseAuthModel(1);
            let userInfo = createResponseUserInfo(1);
            request.flush(tokens);
            tokens.expiration_date = Date.now() + tokens.expires_in * 1000;

            // Don't tick enough to trigger the refresh interval
            tick(1);

            expect(logInSuccessful).toEqual(true);
            expect(localStorage.setItem).toHaveBeenCalledWith("auth-tokens", JSON.stringify(tokens));
            expect(error).toBeUndefined();
            expect(userInfoLog).toEqual([notLoggedInUser, userInfo]);

            (localStorage.setItem as jasmine.Spy).calls.reset();

            // Let the refresh interval tick
            tick(tokens.expires_in / 2 * 1000);

            let refreshedRequest = httpMock.expectOne(tokenRequest);
            expect(refreshedRequest.request.body).toEqual("grant_type=refresh_token&refresh_token=someNewRefreshToken1&scope=openid%20offline_access%20profile%20email%20roles");

            let refreshedTokens = createResponseAuthModel(2);
            let refreshedUserInfo = createResponseUserInfo(2);
            refreshedRequest.flush(refreshedTokens);
            refreshedTokens.expiration_date = Date.now() + refreshedTokens.expires_in * 1000;
            tick();

            expect(localStorage.setItem).toHaveBeenCalledWith("auth-tokens", JSON.stringify(refreshedTokens));
            expect(error).toBeUndefined();
            expect(userInfoLog).toEqual([notLoggedInUser, userInfo, refreshedUserInfo]);

            // An interval was started, so abandon it.
            discardPeriodicTasks();
        }));
    });

    describe("logInWithAssertion", () => {
        it("should reject with an unsuccessful log in", fakeAsync(() => {
            let logInSuccessful = false;
            let error: Error;
            let userInfoLog: IUserInfo[] = [];

            let authenticationService = TestBed.inject(AuthenticationService);
            authenticationService.userInfo()
                .subscribe(userInfo => {
                    userInfoLog.push(userInfo);
                });
            authenticationService.logInWithAssertion("someGrantType", "someAssertion", null)
                .then(() => logInSuccessful = true)
                .catch(e => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(tokenRequest);
            expect(request.request.body).toEqual("grant_type=someGrantType&assertion=someAssertion&scope=openid%20offline_access%20profile%20email%20roles");

            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(logInSuccessful).toEqual(false);
            expect(localStorage.setItem).not.toHaveBeenCalled();
            expect(error).toBeDefined();
            expect(userInfoLog).toEqual([notLoggedInUser]);
        }));

        it("should resolve with a successful log in and refresh the new token", fakeAsync(() => {
            let logInSuccessful = false;
            let error: Error;
            let userInfoLog: IUserInfo[] = [];

            let authenticationService = TestBed.inject(AuthenticationService);
            authenticationService.userInfo()
                .subscribe(_ => userInfoLog.push(_));
            authenticationService.logInWithAssertion("someGrantType", "someAssertion", null)
                .then(() => logInSuccessful = true)
                .catch(e => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(tokenRequest);
            expect(request.request.body).toEqual("grant_type=someGrantType&assertion=someAssertion&scope=openid%20offline_access%20profile%20email%20roles");

            let tokens = createResponseAuthModel(1);
            let userInfo = createResponseUserInfo(1);
            request.flush(tokens);
            tokens.expiration_date = Date.now() + tokens.expires_in * 1000;

            // Don't tick enough to trigger the refresh interval
            tick(1);

            expect(logInSuccessful).toEqual(true);
            expect(localStorage.setItem).toHaveBeenCalledWith("auth-tokens", JSON.stringify(tokens));
            expect(error).toBeUndefined();
            expect(userInfoLog).toEqual([notLoggedInUser, userInfo]);

            (localStorage.setItem as jasmine.Spy).calls.reset();

            // Let the refresh interval tick
            tick(tokens.expires_in / 2 * 1000);

            let refreshRequest = httpMock.expectOne(tokenRequest);
            expect(refreshRequest.request.body).toEqual("grant_type=refresh_token&refresh_token=someNewRefreshToken1&scope=openid%20offline_access%20profile%20email%20roles");

            let refreshedTokens = createResponseAuthModel(2);
            let refreshedUserInfo = createResponseUserInfo(2);
            refreshRequest.flush(refreshedTokens);
            refreshedTokens.expiration_date = Date.now() + refreshedTokens.expires_in * 1000;
            tick();

            expect(localStorage.setItem).toHaveBeenCalledWith("auth-tokens", JSON.stringify(refreshedTokens));
            expect(error).toBeUndefined();
            expect(userInfoLog).toEqual([notLoggedInUser, userInfo, refreshedUserInfo]);

            // An interval was started, so abandon it.
            discardPeriodicTasks();
        }));

        it("should add the username when provided", fakeAsync(() => {
            let authenticationService = TestBed.inject(AuthenticationService);
            authenticationService.logInWithAssertion("someGrantType", "someAssertion", "someUsername");

            let request = httpMock.expectOne(tokenRequest);
            expect(request.request.body).toEqual("grant_type=someGrantType&assertion=someAssertion&username=someUsername&scope=openid%20offline_access%20profile%20email%20roles");
        }));
    });

    describe("logOut", () => {
        it("should successfully log out", fakeAsync(() => {
            let userInfoLog: IUserInfo[] = [];

            let tokens = createCachedAuthModel();
            (localStorage.getItem as jasmine.Spy).and.returnValue(JSON.stringify(tokens));
            let userInfo = createCachedUserInfo();

            let authenticationService = TestBed.inject(AuthenticationService);
            authenticationService.userInfo()
                .subscribe(_ => userInfoLog.push(_));

            // The initial refresh
            httpMock.expectOne(tokenRequest);

            authenticationService.logOut();

            expect(localStorage.removeItem).toHaveBeenCalledWith("auth-tokens");
            expect(userInfoLog).toEqual([userInfo, notLoggedInUser]);
        }));

        it("should stop the token refresh interval after logging out", fakeAsync(() => {
            let userInfoLog: IUserInfo[] = [];

            let tokens = createCachedAuthModel();
            (localStorage.getItem as jasmine.Spy).and.returnValue(JSON.stringify(tokens));
            let userInfo = createCachedUserInfo();

            let authenticationService = TestBed.inject(AuthenticationService);
            authenticationService.userInfo()
                .subscribe(_ => userInfoLog.push(_));

            // Let the refresh interval tick
            tick(tokens.expires_in / 2 * 1000);

            let refreshedRequest = httpMock.expectOne(tokenRequest);
            expect(refreshedRequest.request.body).toEqual("grant_type=refresh_token&refresh_token=someRefreshToken&scope=openid%20offline_access%20profile%20email%20roles");

            let refreshedTokens = createResponseAuthModel(2);
            let refreshedUserInfo = createResponseUserInfo(2);
            refreshedRequest.flush(refreshedTokens);
            refreshedTokens.expiration_date = Date.now() + refreshedTokens.expires_in * 1000;
            tick();

            expect(userInfoLog).toEqual([userInfo, refreshedUserInfo]);
            expect(localStorage.setItem).toHaveBeenCalledWith("auth-tokens", JSON.stringify(refreshedTokens));
            (localStorage.setItem as jasmine.Spy).calls.reset();

            authenticationService.logOut();

            expect(localStorage.removeItem).toHaveBeenCalledWith("auth-tokens");
            expect(userInfoLog).toEqual([userInfo, refreshedUserInfo, notLoggedInUser]);

            // Let the refresh interval tick again
            tick(tokens.expires_in / 2 * 1000);

            // And nothing changed
            expect(localStorage.setItem).not.toHaveBeenCalled();
            expect(userInfoLog).toEqual([userInfo, refreshedUserInfo, notLoggedInUser]);
        }));
    });

    function createCachedAuthModel(): IAuthTokenModel {
        return {
            token_type: "someTokenType",
            access_token: "someAccessToken",
            refresh_token: "someRefreshToken",
            id_token: createIdToken("some"),
            expires_in: 3600,
            expiration_date: Date.now() + 3600,
        };
    }

    function createCachedUserInfo(): IUserInfo {
        return {
            isLoggedIn: true,
            id: "someId",
            username: "someUsername",
            email: "someEmail",
            isAdmin: false,
        };
    }

    function createResponseAuthModel(num?: number): IAuthTokenModel {
        return {
            token_type: "someNewTokenType" + (num === undefined ? "" : num.toString()),
            access_token: "someNewAccessToken" + (num === undefined ? "" : num.toString()),
            refresh_token: "someNewRefreshToken" + (num === undefined ? "" : num.toString()),
            id_token: createIdToken("someNew", num),
            expires_in: 3600,
        };
    }

    function createResponseUserInfo(num?: number): IUserInfo {
        return {
            isLoggedIn: true,
            id: "someNewId" + (num === undefined ? "" : num.toString()),
            username: "someNewUsername" + (num === undefined ? "" : num.toString()),
            email: "someNewEmail" + (num === undefined ? "" : num.toString()),
            isAdmin: false,
        };
    }

    function createIdToken(prefix: string, num?: number): string {
        let claims = {
            sub: prefix + "Id" + (num === undefined ? "" : num.toString()),
            name: prefix + "Username" + (num === undefined ? "" : num.toString()),
            email: prefix + "Email" + (num === undefined ? "" : num.toString()),
        };

        return "." + btoa(JSON.stringify(claims)) + ".";
    }
});
