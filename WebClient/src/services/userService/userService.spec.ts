import { TestBed, fakeAsync, tick } from "@angular/core/testing";
import { HttpClientTestingModule, HttpTestingController } from "@angular/common/http/testing";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";
import { HttpHeaders, HttpErrorResponse } from "@angular/common/http";

import { UserService, IProgressData, IFollowsData, IUserLogins, IUploadSummaryListResponse } from "./userService";
import { AuthenticationService } from "../authenticationService/authenticationService";
import { IUser } from "../../models";

describe("UserService", () => {
    let userService: UserService;
    let authenticationService: AuthenticationService;
    let httpErrorHandlerService: HttpErrorHandlerService;
    let httpMock: HttpTestingController;

    const userName = "someUserName";
    const email = "someEmail";
    const password = "somePassword";
    const code = "someCode";
    const expectedValidationErrors = ["error0", "error1", "error2"];

    beforeEach(() => {
        authenticationService = jasmine.createSpyObj("authenticationService", ["getAuthHeaders"]);
        (authenticationService.getAuthHeaders as jasmine.Spy).and.returnValue(Promise.resolve(new HttpHeaders()));

        httpErrorHandlerService = jasmine.createSpyObj("httpErrorHandlerService", ["logError", "getValidationErrors"]);
        (httpErrorHandlerService.getValidationErrors as jasmine.Spy).and.returnValue(expectedValidationErrors);

        TestBed.configureTestingModule(
            {
                imports: [
                    HttpClientTestingModule,
                ],
                providers:
                    [
                        UserService,
                        { provide: AuthenticationService, useValue: authenticationService },
                        { provide: HttpErrorHandlerService, useValue: httpErrorHandlerService },
                    ],
            });

        userService = TestBed.get(UserService) as UserService;
        httpMock = TestBed.get(HttpTestingController) as HttpTestingController;
    });

    afterEach(() => {
        httpMock.verify();
    });

    describe("create", () => {
        const apiRequest = { method: "post", url: "/api/users" };

        it("should make the correct api call", fakeAsync(() => {
            userService.create(userName, email, password);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            expect(request.request.body).toEqual({ userName, email, password });
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let errors: string[];
            userService.create(userName, email, password)
                .then(() => succeeded = true)
                .catch((e: string[]) => errors = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null);
            tick();

            expect(succeeded).toEqual(true);
            expect(errors).toBeUndefined();
            expect(httpErrorHandlerService.logError).not.toHaveBeenCalled();
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let succeeded = false;
            let errors: string[];
            userService.create(userName, email, password)
                .then(() => succeeded = true)
                .catch((e: string[]) => errors = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(succeeded).toEqual(false);
            expect(errors).toEqual(expectedValidationErrors);
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UserService.create.error", jasmine.any(HttpErrorResponse));
            expect(httpErrorHandlerService.getValidationErrors).toHaveBeenCalledWith(jasmine.any(HttpErrorResponse));
        }));
    });

    describe("getUser", () => {
        const apiRequest = { method: "get", url: `/api/users/${userName}` };

        it("should make an api call", fakeAsync(() => {
            userService.getUser(userName);

            // Tick the getAuthHeaders call
            tick();

            httpMock.expectOne(apiRequest);

            expect(authenticationService.getAuthHeaders).not.toHaveBeenCalled();
        }));

        it("should return some uploads", fakeAsync(() => {
            let response: IUser;
            let error: HttpErrorResponse;
            userService.getUser(userName)
                .then((r: IUser) => response = r)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let expectedResponse: IUser = { name: "someName", clanName: "someClanName" };
            let request = httpMock.expectOne(apiRequest);
            request.flush(expectedResponse);
            tick();

            expect(response).toEqual(expectedResponse, "should return the expected response");
            expect(error).toBeUndefined();
            expect(httpErrorHandlerService.logError).not.toHaveBeenCalled();
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
            expect(authenticationService.getAuthHeaders).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let response: IUser;
            let error: HttpErrorResponse;
            userService.getUser(userName)
                .then((r: IUser) => response = r)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(response).toBeUndefined();
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UserService.getUser.error", error);
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
            expect(authenticationService.getAuthHeaders).not.toHaveBeenCalled();
        }));
    });

    describe("getUploads", () => {
        const page = 1;
        const count = 2;
        const apiRequest = { method: "get", url: `/api/users/${userName}/uploads?page=${page}&count=${count}` };

        it("should make an api call", fakeAsync(() => {
            userService.getUploads(userName, page, count);

            // Tick the getAuthHeaders call
            tick();

            httpMock.expectOne(apiRequest);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return some uploads", fakeAsync(() => {
            let response: IUploadSummaryListResponse;
            let error: HttpErrorResponse;
            userService.getUploads(userName, page, count)
                .then((r: IUploadSummaryListResponse) => response = r)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let expectedResponse: IUploadSummaryListResponse = { pagination: { count: 0, next: "", previous: "" }, uploads: [] };
            let request = httpMock.expectOne(apiRequest);
            request.flush(expectedResponse);
            tick();

            expect(response).toEqual(expectedResponse, "should return the expected response");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(error).toBeUndefined();
            expect(httpErrorHandlerService.logError).not.toHaveBeenCalled();
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let response: IUploadSummaryListResponse;
            let error: HttpErrorResponse;
            userService.getUploads(userName, page, count)
                .then((r: IUploadSummaryListResponse) => response = r)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(response).toBeUndefined();
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UserService.getUploads.error", error);
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));
    });

    describe("getProgress", () => {
        const startString = "2017-01-01T00:00:00.000Z";
        const startDate = new Date(startString);
        const endString = "2017-01-02T00:00:00.000Z";
        const endDate = new Date(endString);
        const apiRequest = { method: "get", url: `/api/users/${userName}/progress?start=${startString}&end=${endString}` };

        it("should make the correct api call", fakeAsync(() => {
            userService.getProgress(userName, startDate, endDate);

            // Tick the getAuthHeaders call
            tick();

            httpMock.expectOne(apiRequest);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return user progress", fakeAsync(() => {
            let progress: IProgressData;
            let error: HttpErrorResponse;
            userService.getProgress(userName, startDate, endDate)
                .then((r: IProgressData) => progress = r)
                .catch((e: HttpErrorResponse) => error = e);

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
            let request = httpMock.expectOne(apiRequest);
            request.flush(expectedResponse);
            tick();

            expect(progress).toEqual(expectedResponse, "should return the expected response");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(error).toBeUndefined();
            expect(httpErrorHandlerService.logError).not.toHaveBeenCalled();
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let progress: IProgressData;
            let error: HttpErrorResponse;
            userService.getProgress(userName, startDate, endDate)
                .then((r: IProgressData) => progress = r)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(progress).toBeUndefined();
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UserService.getProgress.error", error);
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));
    });

    describe("getFollows", () => {
        const apiRequest = { method: "get", url: `/api/users/${userName}/follows` };

        it("should make the correct api call", fakeAsync(() => {
            userService.getFollows(userName);

            // Tick the getAuthHeaders call
            tick();

            httpMock.expectOne(apiRequest);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return follow data", fakeAsync(() => {
            let follows: IFollowsData;
            let error: HttpErrorResponse;
            userService.getFollows(userName)
                .then((r: IFollowsData) => follows = r)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let expectedResponse: IFollowsData = {
                follows: [],
            };
            let request = httpMock.expectOne(apiRequest);
            request.flush(expectedResponse);
            tick();

            expect(follows).toEqual(expectedResponse, "should return the expected response");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(error).toBeUndefined();
            expect(httpErrorHandlerService.logError).not.toHaveBeenCalled();
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let follows: IFollowsData;
            let error: HttpErrorResponse;
            userService.getFollows(userName)
                .then((r: IFollowsData) => follows = r)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(follows).toBeUndefined();
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UserService.getFollows.error", error);
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));
    });

    describe("addFollow", () => {
        const followUserName = "someFollowUserName";
        const apiRequest = { method: "post", url: `/api/users/${userName}/follows` };

        it("should make the correct api call", fakeAsync(() => {
            userService.addFollow(userName, followUserName);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            expect(request.request.body).toEqual({ followUserName });
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let error: HttpErrorResponse;
            userService.addFollow(userName, followUserName)
                .then(() => succeeded = true)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null);
            tick();

            expect(succeeded).toEqual(true);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(error).toBeUndefined();
            expect(httpErrorHandlerService.logError).not.toHaveBeenCalled();
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let succeeded = false;
            let error: HttpErrorResponse;
            userService.addFollow(userName, followUserName)
                .then(() => succeeded = true)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(succeeded).toEqual(false);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UserService.addFollow.error", error);
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));
    });

    describe("removeFollow", () => {
        const followUserName = "someFollowUserName";
        const apiRequest = { method: "delete", url: `/api/users/${userName}/follows/${followUserName}` };

        it("should make the correct api call", fakeAsync(() => {
            userService.removeFollow(userName, followUserName);

            // Tick the getAuthHeaders call
            tick();

            httpMock.expectOne(apiRequest);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let error: HttpErrorResponse;
            userService.removeFollow(userName, followUserName)
                .then(() => succeeded = true)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null);
            tick();

            expect(succeeded).toEqual(true);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(error).toBeUndefined();
            expect(httpErrorHandlerService.logError).not.toHaveBeenCalled();
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let succeeded = false;
            let error: HttpErrorResponse;
            userService.removeFollow(userName, followUserName)
                .then(() => succeeded = true)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(succeeded).toEqual(false);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UserService.removeFollow.error", error);
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));
    });

    describe("getLogins", () => {
        const apiRequest = { method: "get", url: `/api/users/${userName}/logins` };

        it("should make the correct api call", fakeAsync(() => {
            userService.getLogins(userName);

            // Tick the getAuthHeaders call
            tick();

            httpMock.expectOne(apiRequest);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return data", fakeAsync(() => {
            let data: IUserLogins;
            let error: HttpErrorResponse;
            userService.getLogins(userName)
                .then((d: IUserLogins) => data = d)
                .catch((e: HttpErrorResponse) => error = e);

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
            let request = httpMock.expectOne(apiRequest);
            request.flush(expectedResponse);
            tick();

            expect(data).toEqual(expectedResponse, "should return the expected response");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(error).toBeUndefined();
            expect(httpErrorHandlerService.logError).not.toHaveBeenCalled();
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let follows: IUserLogins;
            let error: HttpErrorResponse;
            userService.getLogins(userName)
                .then((r: IUserLogins) => follows = r)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(follows).toBeUndefined();
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UserService.getLogins.error", error);
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));
    });

    describe("removeLogin", () => {
        const externalLogin = { providerName: "someProviderName", externalUserId: "someExternalUserId" };
        const apiRequest = { method: "delete", url: `/api/users/${userName}/logins` };

        it("should make the correct api call", fakeAsync(() => {
            userService.removeLogin(userName, externalLogin);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            expect(request.request.body).toEqual(externalLogin);
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let error: HttpErrorResponse;
            userService.removeLogin(userName, externalLogin)
                .then(() => succeeded = true)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null);
            tick();

            expect(succeeded).toEqual(true);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(error).toBeUndefined();
            expect(httpErrorHandlerService.logError).not.toHaveBeenCalled();
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let succeeded = false;
            let error: HttpErrorResponse;
            userService.removeLogin(userName, externalLogin)
                .then(() => succeeded = true)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(succeeded).toEqual(false);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UserService.removeLogin.error", error);
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));
    });

    describe("setPassword", () => {
        const newPassword = "someNewPassword";
        const apiRequest = { method: "post", url: `/api/users/${userName}/setpassword` };

        it("should make the correct api call", fakeAsync(() => {
            userService.setPassword(userName, newPassword);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            expect(request.request.body).toEqual({ newPassword });
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let errors: string[];
            userService.setPassword(userName, newPassword)
                .then(() => succeeded = true)
                .catch((e: string[]) => errors = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null);
            tick();

            expect(succeeded).toEqual(true);
            expect(errors).toBeUndefined();
            expect(httpErrorHandlerService.logError).not.toHaveBeenCalled();
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let succeeded = false;
            let errors: string[];
            userService.setPassword(userName, newPassword)
                .then(() => succeeded = true)
                .catch((e: string[]) => errors = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(succeeded).toEqual(false);
            expect(errors).toEqual(expectedValidationErrors);
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UserService.setPassword.error", jasmine.any(HttpErrorResponse));
            expect(httpErrorHandlerService.getValidationErrors).toHaveBeenCalledWith(jasmine.any(HttpErrorResponse));
        }));
    });

    describe("changePassword", () => {
        const currentPassword = "someCurrentPassword";
        const newPassword = "someNewPassword";
        const apiRequest = { method: "post", url: `/api/users/${userName}/changepassword` };

        it("should make the correct api call", fakeAsync(() => {
            userService.changePassword(userName, currentPassword, newPassword);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            expect(request.request.body).toEqual({ currentPassword, newPassword });
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let errors: string[];
            userService.changePassword(userName, currentPassword, newPassword)
                .then(() => succeeded = true)
                .catch((e: string[]) => errors = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null);
            tick();

            expect(succeeded).toEqual(true);
            expect(errors).toBeUndefined();
            expect(httpErrorHandlerService.logError).not.toHaveBeenCalled();
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let succeeded = false;
            let errors: string[];
            userService.changePassword(userName, currentPassword, newPassword)
                .then(() => succeeded = true)
                .catch((e: string[]) => errors = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(succeeded).toEqual(false);
            expect(errors).toEqual(expectedValidationErrors);
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UserService.changePassword.error", jasmine.any(HttpErrorResponse));
            expect(httpErrorHandlerService.getValidationErrors).toHaveBeenCalledWith(jasmine.any(HttpErrorResponse));
        }));
    });

    describe("resetPassword", () => {
        const apiRequest = { method: "post", url: "/api/users/resetpassword" };

        it("should make the correct api call", fakeAsync(() => {
            userService.resetPassword(email);

            let request = httpMock.expectOne(apiRequest);
            expect(request.request.body).toEqual({ email });
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let errors: string[];
            userService.resetPassword(email)
                .then(() => succeeded = true)
                .catch((e: string[]) => errors = e);

            let request = httpMock.expectOne(apiRequest);
            request.flush(null);
            tick();

            expect(succeeded).toEqual(true);
            expect(errors).toBeUndefined();
            expect(httpErrorHandlerService.logError).not.toHaveBeenCalled();
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let succeeded = false;
            let errors: string[];
            userService.resetPassword(email)
                .then(() => succeeded = true)
                .catch((e: string[]) => errors = e);

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(succeeded).toEqual(false);
            expect(errors).toEqual(expectedValidationErrors);
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UserService.resetPassword.error", jasmine.any(HttpErrorResponse));
            expect(httpErrorHandlerService.getValidationErrors).toHaveBeenCalledWith(jasmine.any(HttpErrorResponse));
        }));
    });

    describe("resetPasswordConfirmation", () => {
        const apiRequest = { method: "post", url: "/api/users/resetpasswordconfirmation" };

        it("should make the correct api call", fakeAsync(() => {
            userService.resetPasswordConfirmation(email, password, code);

            let request = httpMock.expectOne(apiRequest);
            expect(request.request.body).toEqual({ email, password, code });
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let errors: string[];
            userService.resetPasswordConfirmation(email, password, code)
                .then(() => succeeded = true)
                .catch((e: string[]) => errors = e);

            let request = httpMock.expectOne(apiRequest);
            request.flush(null);
            tick();

            expect(succeeded).toEqual(true);
            expect(errors).toBeUndefined();
            expect(httpErrorHandlerService.logError).not.toHaveBeenCalled();
            expect(httpErrorHandlerService.getValidationErrors).not.toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let succeeded = false;
            let errors: string[];
            userService.resetPasswordConfirmation(email, password, code)
                .then(() => succeeded = true)
                .catch((e: string[]) => errors = e);

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(succeeded).toEqual(false);
            expect(errors).toEqual(expectedValidationErrors);
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UserService.resetPasswordConfirmation.error", jasmine.any(HttpErrorResponse));
            expect(httpErrorHandlerService.getValidationErrors).toHaveBeenCalledWith(jasmine.any(HttpErrorResponse));
        }));
    });
});
