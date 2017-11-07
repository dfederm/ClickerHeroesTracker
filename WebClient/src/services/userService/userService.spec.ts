import { ReflectiveInjector } from "@angular/core";
import { fakeAsync, tick } from "@angular/core/testing";
import { BaseRequestOptions, ConnectionBackend, Http, Headers, RequestOptions, Response, ResponseOptions, RequestMethod } from "@angular/http";
import { MockBackend, MockConnection } from "@angular/http/testing";
import { AppInsightsService } from "@markpieszak/ng-application-insights";

import { UserService, IProgressData, IFollowsData, IValidationErrorResponse, IUserLogins, IUploadSummaryListResponse } from "./userService";
import { AuthenticationService } from "../authenticationService/authenticationService";

class MockError extends Response implements Error {
    public name: string;
    public message: string;
}

describe("UserService", () => {
    let userService: UserService;
    let authenticationService: AuthenticationService;
    let appInsights: AppInsightsService;
    let backend: MockBackend;
    let lastConnection: MockConnection;

    const userName = "someUserName";
    const email = "someEmail";
    const password = "somePassword";
    const code = "someCode";

    beforeEach(() => {
        authenticationService = jasmine.createSpyObj("authenticationService", ["getAuthHeaders"]);
        (authenticationService.getAuthHeaders as jasmine.Spy).and.returnValue(Promise.resolve(new Headers()));

        appInsights = jasmine.createSpyObj("appInsights", ["trackEvent"]);

        let injector = ReflectiveInjector.resolveAndCreate(
            [
                UserService,
                { provide: ConnectionBackend, useClass: MockBackend },
                { provide: RequestOptions, useClass: BaseRequestOptions },
                Http,
                { provide: AuthenticationService, useValue: authenticationService },
                { provide: AppInsightsService, useValue: appInsights },
            ]);

        userService = injector.get(UserService) as UserService;
        backend = injector.get(ConnectionBackend) as MockBackend;
        backend.connections.subscribe((connection: MockConnection) => lastConnection = connection);
    });

    afterEach(() => {
        lastConnection = null;
        backend.verifyNoPendingRequests();
    });

    describe("create", () => {
        it("should make the correct api call", fakeAsync(() => {
            userService.create(userName, email, password);

            // Tick the getAuthHeaders call
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

            // Tick the getAuthHeaders call
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

            // Tick the getAuthHeaders call
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

            // Tick the getAuthHeaders call
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

    describe("getUploads", () => {
        let page = 1;
        let count = 2;

        it("should make an api call", fakeAsync(() => {
            userService.getUploads(userName, page, count);

            // Tick the getAuthHeaders call
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
            expect(lastConnection.request.url).toEqual(`/api/users/${userName}/uploads?page=${page}&count=${count}`, "url invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return some uploads", fakeAsync(() => {
            let response: IUploadSummaryListResponse;
            userService.getUploads(userName, page, count)
                .then((r: IUploadSummaryListResponse) => response = r);

            // Tick the getAuthHeaders call
            tick();

            let expectedResponse: IUploadSummaryListResponse = { pagination: { count: 0, next: "", previous: "" }, uploads: [] };
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(expectedResponse) })));
            tick();

            expect(response).toEqual(expectedResponse, "should return the expected response");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let response: IUploadSummaryListResponse;
            let error: string;
            userService.getUploads(userName, page, count)
                .then((r: IUploadSummaryListResponse) => response = r)
                .catch((e: string) => error = e);

            // Tick the getAuthHeaders call
            tick();

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(response).toBeUndefined();
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });

    describe("getProgress", () => {
        let startString = "2017-01-01T00:00:00.000Z";
        let startDate = new Date(startString);
        let endString = "2017-01-02T00:00:00.000Z";
        let endDate = new Date(endString);

        it("should make the correct api call", fakeAsync(() => {
            userService.getProgress(userName, startDate, endDate);

            // Tick the getAuthHeaders call
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
            expect(lastConnection.request.url).toEqual(`/api/users/${userName}/progress?start=${startString}&end=${endString}`, "url invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return user progress", fakeAsync(() => {
            let progress: IProgressData;
            userService.getProgress(userName, startDate, endDate)
                .then((r: IProgressData) => progress = r);

            // Tick the getAuthHeaders call
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

        it("should handle http errors", fakeAsync(() => {
            let progress: IProgressData;
            let error: string;
            userService.getProgress(userName, startDate, endDate)
                .then((r: IProgressData) => progress = r)
                .catch((e: string) => error = e);

            // Tick the getAuthHeaders call
            tick();

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(progress).toBeUndefined();
            expect(error).toEqual("someError");
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });

    describe("getFollows", () => {
        it("should make the correct api call", fakeAsync(() => {
            userService.getFollows(userName);

            // Tick the getAuthHeaders call
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

            // Tick the getAuthHeaders call
            tick();

            let expectedResponse: IFollowsData = {
                follows: [],
            };
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(expectedResponse) })));
            tick();

            expect(follows).toEqual(expectedResponse, "should return the expected response");
        }));

        it("should handle http errors", fakeAsync(() => {
            let follows: IFollowsData;
            let error: string;
            userService.getFollows(userName)
                .then((r: IFollowsData) => follows = r)
                .catch((e: string) => error = e);

            // Tick the getAuthHeaders call
            tick();

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(follows).toBeUndefined();
            expect(error).toEqual("someError");
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });

    describe("getLogins", () => {
        it("should make the correct api call", fakeAsync(() => {
            userService.getLogins(userName);

            // Tick the getAuthHeaders call
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
            expect(lastConnection.request.url).toEqual(`/api/users/${userName}/logins`, "url invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return data", fakeAsync(() => {
            let data: IUserLogins;
            userService.getLogins(userName)
                .then((d: IUserLogins) => data = d);

            // Tick the getAuthHeaders call
            tick();

            let expectedResponse: IUserLogins = {
                hasPassword: true,
                externalLogins: [
                    { providerName: "someProviderName0", externalUserId: "someExternalUserId0" },
                    { providerName: "someProviderName1", externalUserId: "someExternalUserId1" },
                    { providerName: "someProviderName2", externalUserId: "someExternalUserId2" },
                ],
            };
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(expectedResponse) })));
            tick();

            expect(data).toEqual(expectedResponse, "should return the expected response");
        }));

        it("should handle http errors", fakeAsync(() => {
            let follows: IUserLogins;
            let error: string;
            userService.getLogins(userName)
                .then((r: IUserLogins) => follows = r)
                .catch((e: string) => error = e);

            // Tick the getAuthHeaders call
            tick();

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(follows).toBeUndefined();
            expect(error).toEqual("someError");
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });

    describe("removeLogin", () => {
        const externalLogin = { providerName: "someProviderName", externalUserId: "someExternalUserId" };

        it("should make the correct api call", fakeAsync(() => {
            userService.removeLogin(userName, externalLogin);

            // Tick the getAuthHeaders call
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Delete, "method invalid");
            expect(lastConnection.request.url).toEqual(`/api/users/${userName}/logins`, "url invalid");
            expect(lastConnection.request.json()).toEqual(externalLogin, "request body invalid");
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let error: string;
            userService.removeLogin(userName, externalLogin)
                .then(() => succeeded = true)
                .catch((e: string) => error = e);

            // Tick the getAuthHeaders call
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
            userService.removeLogin(userName, externalLogin)
                .then(() => succeeded = true)
                .catch((e: string) => error = e);

            // Tick the getAuthHeaders call
            tick();

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(succeeded).toEqual(false);
            expect(error).toEqual("someError");
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });

    describe("setPassword", () => {
        const newPassword = "someNewPassword";

        it("should make the correct api call", fakeAsync(() => {
            userService.setPassword(userName, newPassword);

            // Tick the getAuthHeaders call
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual(`/api/users/${userName}/setpassword`, "url invalid");
            expect(lastConnection.request.json()).toEqual({ newPassword }, "request body invalid");
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let error: string;
            userService.setPassword(userName, newPassword)
                .then(() => succeeded = true)
                .catch((e: string) => error = e);

            // Tick the getAuthHeaders call
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
            userService.setPassword(userName, newPassword)
                .then(() => succeeded = true)
                .catch((e: string) => error = e);

            // Tick the getAuthHeaders call
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
            userService.setPassword(userName, newPassword)
                .then(() => succeeded = true)
                .catch((e: string[]) => errors = e);

            // Tick the getAuthHeaders call
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

    describe("changePassword", () => {
        const currentPassword = "someCurrentPassword";
        const newPassword = "someNewPassword";

        it("should make the correct api call", fakeAsync(() => {
            userService.changePassword(userName, currentPassword, newPassword);

            // Tick the getAuthHeaders call
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual(`/api/users/${userName}/changepassword`, "url invalid");
            expect(lastConnection.request.json()).toEqual({ currentPassword, newPassword }, "request body invalid");
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let error: string;
            userService.changePassword(userName, currentPassword, newPassword)
                .then(() => succeeded = true)
                .catch((e: string) => error = e);

            // Tick the getAuthHeaders call
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
            userService.changePassword(userName, currentPassword, newPassword)
                .then(() => succeeded = true)
                .catch((e: string) => error = e);

            // Tick the getAuthHeaders call
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
            userService.changePassword(userName, currentPassword, newPassword)
                .then(() => succeeded = true)
                .catch((e: string[]) => errors = e);

            // Tick the getAuthHeaders call
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

    describe("resetPassword", () => {
        it("should make the correct api call", fakeAsync(() => {
            userService.resetPassword(email);

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/users/resetpassword", "url invalid");
            expect(lastConnection.request.json()).toEqual({ email }, "request body invalid");
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let error: string;
            userService.resetPassword(email)
                .then(() => succeeded = true)
                .catch((e: string) => error = e);

            lastConnection.mockRespond(new Response(new ResponseOptions()));
            tick();

            expect(succeeded).toEqual(true);
            expect(error).toBeUndefined();
            expect(appInsights.trackEvent).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let succeeded = false;
            let error: string;
            userService.resetPassword(email)
                .then(() => succeeded = true)
                .catch((e: string) => error = e);

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(succeeded).toEqual(false);
            expect(error).toEqual(["someError"]);
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));

        it("should handle validation errors", fakeAsync(() => {
            let succeeded = false;
            let errors: string[];
            userService.resetPassword(email)
                .then(() => succeeded = true)
                .catch((e: string[]) => errors = e);

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

    describe("resetPasswordConfirmation", () => {
        it("should make the correct api call", fakeAsync(() => {
            userService.resetPasswordConfirmation(email, password, code);

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/users/resetpasswordconfirmation", "url invalid");
            expect(lastConnection.request.json()).toEqual({ email, password, code }, "request body invalid");
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let error: string;
            userService.resetPasswordConfirmation(email, password, code)
                .then(() => succeeded = true)
                .catch((e: string) => error = e);

            lastConnection.mockRespond(new Response(new ResponseOptions()));
            tick();

            expect(succeeded).toEqual(true);
            expect(error).toBeUndefined();
            expect(appInsights.trackEvent).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let succeeded = false;
            let error: string;
            userService.resetPasswordConfirmation(email, password, code)
                .then(() => succeeded = true)
                .catch((e: string) => error = e);

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(succeeded).toEqual(false);
            expect(error).toEqual(["someError"]);
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));

        it("should handle validation errors", fakeAsync(() => {
            let succeeded = false;
            let errors: string[];
            userService.resetPasswordConfirmation(email, password, code)
                .then(() => succeeded = true)
                .catch((e: string[]) => errors = e);

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
});
