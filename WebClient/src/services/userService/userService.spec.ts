import { ReflectiveInjector } from "@angular/core";
import { fakeAsync, tick } from "@angular/core/testing";
import { BaseRequestOptions, ConnectionBackend, Http, RequestOptions } from "@angular/http";
import { Response, ResponseOptions, RequestMethod } from "@angular/http";
import { MockBackend, MockConnection } from "@angular/http/testing";
import { BehaviorSubject } from "rxjs/BehaviorSubject";

import { UserService, IProgressData, IFollowsData, IValidationErrorResponse } from "./userService";
import { AuthenticationService } from "../authenticationService/authenticationService";

// tslint:disable-next-line:no-namespace
declare global {
    // tslint:disable-next-line:interface-name - We don't own this interface name, just extending it
    interface Window {
        appInsights: Microsoft.ApplicationInsights.IAppInsights;
    }
}

class MockError extends Response implements Error {
    public name: string;
    public message: string;
}

describe("UserService", () => {
    let userService: UserService;
    let authenticationService: AuthenticationService;
    let backend: MockBackend;
    let lastConnection: MockConnection;
    let isLoggedIn: BehaviorSubject<boolean>;

    let userName = "someUserName";
    let email = "someEmail";
    let password = "somePassword";

    beforeEach(() => {
        isLoggedIn = new BehaviorSubject(false);
        authenticationService = jasmine.createSpyObj("authenticationService", ["isLoggedIn", "getAuthHeaders"]);
        (authenticationService.isLoggedIn as jasmine.Spy).and.returnValue(isLoggedIn);
        (authenticationService.getAuthHeaders as jasmine.Spy).and.returnValue(Promise.resolve(new Headers()));

        let injector = ReflectiveInjector.resolveAndCreate(
            [
                UserService,
                { provide: ConnectionBackend, useClass: MockBackend },
                { provide: RequestOptions, useClass: BaseRequestOptions },
                Http,
                { provide: AuthenticationService, useValue: authenticationService },
            ]);

        userService = injector.get(UserService) as UserService;
        backend = injector.get(ConnectionBackend) as MockBackend;
        backend.connections.subscribe((connection: MockConnection) => lastConnection = connection);

        // Mock the global variable. We should figure out a better way to both inject this in the product and mock this in tests.
        window.appInsights = jasmine.createSpyObj("appInsights", ["trackEvent"]);
    });

    afterEach(() => {
        lastConnection = null;
        backend.verifyNoPendingRequests();
    });

    describe("create", () => {
        it("should make the correct api call when the use is not logged in", fakeAsync(() => {
            userService.create(userName, email, password);
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/users", "url invalid");
            expect(lastConnection.request.json()).toEqual({ userName, email, password }, "request body invalid");
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let error: string;
            userService.create(userName, email, password)
                .then(() => succeeded = true)
                .catch((e: string) => error = e);
            tick();

            lastConnection.mockRespond(new Response(new ResponseOptions()));
            tick();

            expect(succeeded).toEqual(true);
            expect(error).toBeUndefined();
            expect(appInsights.trackEvent).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let succeeded = false;
            let error: string;
            userService.create(userName, email, password)
                .then(() => succeeded = true)
                .catch((e: string) => error = e);
            tick();

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(succeeded).toEqual(false);
            expect(error).toEqual(["someError"]);
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));

        it("should handle validation errors", fakeAsync(() => {
            let succeeded = false;
            let errors: string[];
            userService.create(userName, email, password)
                .then(() => succeeded = true)
                .catch((e: string[]) => errors = e);
            tick();

            let validationError: IValidationErrorResponse = {
                field0: ["error0_0", "error0_1", "error0_2"],
                field1: ["error1_0", "error1_1", "error1_2"],
                field2: ["error2_0", "error2_1", "error2_2"],
            };
            lastConnection.mockError(new MockError(new ResponseOptions({ body: validationError })));
            tick();

            expect(succeeded).toEqual(false);
            expect(errors).toEqual(["error0_0", "error0_1", "error0_2", "error1_0", "error1_1", "error1_2", "error2_0", "error2_1", "error2_2"]);
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });

    describe("getProgress", () => {
        let startString = "2017-01-01T00:00:00.000Z";
        let startDate = new Date(startString);
        let endString = "2017-01-02T00:00:00.000Z";
        let endDate = new Date(endString);

        it("should make the correct api call when the use is not logged in", fakeAsync(() => {
            isLoggedIn.next(false);
            tick();

            userService.getProgress(userName, startDate, endDate);
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
            expect(lastConnection.request.url).toEqual(`/api/users/${userName}/progress?start=${encodeURIComponent(startString)}&end=${encodeURIComponent(endString)}`, "url invalid");
            expect(authenticationService.getAuthHeaders).not.toHaveBeenCalled();
        }));

        it("should make the correct api call when the use is logged in", fakeAsync(() => {
            isLoggedIn.next(true);
            tick();

            userService.getProgress(userName, startDate, endDate);
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
            expect(lastConnection.request.url).toEqual(`/api/users/${userName}/progress?start=${encodeURIComponent(startString)}&end=${encodeURIComponent(endString)}`, "url invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return user progress", fakeAsync(() => {
            let progress: IProgressData;
            userService.getProgress(userName, startDate, endDate)
                .then((r: IProgressData) => progress = r);
            tick();

            let expectedResponse: IProgressData = {
                titanDamageData: {},
                soulsSpentData: {},
                heroSoulsSacrificedData: {},
                totalAncientSoulsData: {},
                transcendentPowerData: {},
                rubiesData: {},
                highestZoneThisTranscensionData: {},
                highestZoneLifetimeData: {},
                ascensionsThisTranscensionData: {},
                ascensionsLifetimeData: {},
                ancientLevelData: {},
                outsiderLevelData: {},
            };
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(expectedResponse) })));
            tick();

            expect(progress).toEqual(expectedResponse, "should return the expected response");
        }));

        it("should handle errors from authenticationService.getAuthHeaders", fakeAsync(() => {
            (authenticationService.getAuthHeaders as jasmine.Spy).and.callFake(() => Promise.reject("someError"));

            isLoggedIn.next(true);
            tick();

            let progress: IProgressData;
            let error: string;
            userService.getProgress(userName, startDate, endDate)
                .then((r: IProgressData) => progress = r)
                .catch((e: string) => error = e);
            tick();

            expect(progress).toBeUndefined();
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let progress: IProgressData;
            let error: string;
            userService.getProgress(userName, startDate, endDate)
                .then((r: IProgressData) => progress = r)
                .catch((e: string) => error = e);
            tick();

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(progress).toBeUndefined();
            expect(error).toEqual("someError");
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });

    describe("getFollows", () => {
        it("should make the correct api call when the use is not logged in", fakeAsync(() => {
            isLoggedIn.next(false);
            tick();

            userService.getFollows(userName);
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
            expect(lastConnection.request.url).toEqual(`/api/users/${userName}/follows`, "url invalid");
            expect(authenticationService.getAuthHeaders).not.toHaveBeenCalled();
        }));

        it("should make the correct api call when the use is logged in", fakeAsync(() => {
            isLoggedIn.next(true);
            tick();

            userService.getFollows(userName);
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
            expect(lastConnection.request.url).toEqual(`/api/users/${userName}/follows`, "url invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return follow data", fakeAsync(() => {
            let follows: IFollowsData;
            userService.getFollows(userName)
                .then((r: IFollowsData) => follows = r);
            tick();

            let expectedResponse: IFollowsData = {
                follows: [],
            };
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(expectedResponse) })));
            tick();

            expect(follows).toEqual(expectedResponse, "should return the expected response");
        }));

        it("should handle errors from authenticationService.getAuthHeaders", fakeAsync(() => {
            (authenticationService.getAuthHeaders as jasmine.Spy).and.callFake(() => Promise.reject("someError"));

            isLoggedIn.next(true);
            tick();

            let follows: IFollowsData;
            let error: string;
            userService.getFollows(userName)
                .then((r: IFollowsData) => follows = r)
                .catch((e: string) => error = e);
            tick();

            expect(follows).toBeUndefined();
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let follows: IFollowsData;
            let error: string;
            userService.getFollows(userName)
                .then((r: IFollowsData) => follows = r)
                .catch((e: string) => error = e);
            tick();

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(follows).toBeUndefined();
            expect(error).toEqual("someError");
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });
});
