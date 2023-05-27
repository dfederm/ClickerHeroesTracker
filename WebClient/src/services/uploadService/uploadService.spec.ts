import { TestBed, fakeAsync, tick } from "@angular/core/testing";
import { HttpClientTestingModule, HttpTestingController } from "@angular/common/http/testing";
import { BehaviorSubject } from "rxjs";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";
import { HttpHeaders, HttpErrorResponse } from "@angular/common/http";

import { UploadService, IUpload } from "./uploadService";
import { AuthenticationService, IUserInfo } from "../authenticationService/authenticationService";
import { LoggingService } from "../loggingService/loggingService";
import { UserService } from "../userService/userService";
import { IUser } from "../../models";

describe("UploadService", () => {
    let uploadService: UploadService;
    let authenticationService: AuthenticationService;
    let httpErrorHandlerService: HttpErrorHandlerService;
    let httpMock: HttpTestingController;
    let userInfo: BehaviorSubject<IUserInfo>;

    const loggedInUser: IUserInfo = {
        isLoggedIn: true,
        id: "someId",
        username: "someUsername",
        email: "someEmail",
    };

    const notLoggedInUser: IUserInfo = {
        isLoggedIn: false,
    };

    beforeEach(() => {
        userInfo = new BehaviorSubject(notLoggedInUser);
        authenticationService = jasmine.createSpyObj("authenticationService", ["userInfo", "getAuthHeaders"]);
        (authenticationService.userInfo as jasmine.Spy).and.returnValue(userInfo);
        (authenticationService.getAuthHeaders as jasmine.Spy).and.returnValue(Promise.resolve(new HttpHeaders()));

        httpErrorHandlerService = jasmine.createSpyObj("httpErrorHandlerService", ["logError"]);

        let loggingService = { logEvent: (): void => void 0 };
        let userService = { getUser: (): void => void 0 };

        TestBed.configureTestingModule(
            {
                imports: [
                    HttpClientTestingModule,
                ],
                providers:
                    [
                        UploadService,
                        { provide: AuthenticationService, useValue: authenticationService },
                        { provide: HttpErrorHandlerService, useValue: httpErrorHandlerService },
                        { provide: LoggingService, useValue: loggingService },
                        { provide: UserService, useValue: userService },
                    ],
            });

        uploadService = TestBed.inject(UploadService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    describe("get", () => {
        const apiRequest = { method: "get", url: "/api/uploads/123" };

        it("should make the correct api call", fakeAsync(() => {
            uploadService.get(123);

            // Tick the getAuthHeaders call
            tick();

            httpMock.expectOne(apiRequest);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return an upload", fakeAsync(() => {
            let upload: IUpload;
            uploadService.get(123)
                .then((r: IUpload) => upload = r);

            // Tick the getAuthHeaders call
            tick();

            let expectedResponse: IUpload = { id: 123, timeSubmitted: "someTimeSubmitted", playStyle: "somePlayStyle", user: { name: "someUserName", clanName: "someClanName" }, content: "someContent", isScrubbed: false };
            let request = httpMock.expectOne(apiRequest);
            request.flush(expectedResponse);
            tick();

            expect(upload).toEqual(expectedResponse, "should return the expected upload");
        }));

        it("should handle http errors", fakeAsync(() => {
            let upload: IUpload;
            let error: HttpErrorResponse;
            uploadService.get(123)
                .then((r: IUpload) => upload = r)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(upload).toBeUndefined();
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UploadService.get.error", error);
        }));
    });

    describe("create", () => {
        const apiRequest = { method: "post", url: "/api/uploads" };

        it("should make the correct api call", fakeAsync(() => {
            uploadService.create("someEncodedSaveData", true, "somePlayStyle");

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            expect(request.request.body).toEqual("encodedSaveData=someEncodedSaveData&addToProgress=true&playStyle=somePlayStyle");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let uploadId: number;
            let error: HttpErrorResponse;
            uploadService.create("someEncodedSaveData", true, "somePlayStyle")
                .then((id: number) => uploadId = id)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(uploadId).toBeUndefined();
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UploadService.create.error", error);
        }));
    });

    describe("delete", () => {
        const apiRequest = { method: "delete", url: "/api/uploads/123" };

        it("should make an api call", fakeAsync(() => {
            uploadService.delete(123);

            // Tick the getAuthHeaders call
            tick();

            httpMock.expectOne(apiRequest);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let succeeded = false;
            let error: HttpErrorResponse;
            uploadService.delete(123)
                .then(() => succeeded = true)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(succeeded).toEqual(false);
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UploadService.delete.error", error);
        }));
    });

    describe("caching", () => {
        const getApiRequest = { method: "get", url: "/api/uploads/123" };
        const createApiRequest = { method: "post", url: "/api/uploads" };
        const deleteApiRequest = { method: "delete", url: "/api/uploads/123" };

        it("should cache uploads", fakeAsync(() => {
            let upload: IUpload;
            uploadService.get(123)
                .then((r: IUpload) => upload = r);

            // Tick the getAuthHeaders call
            tick();

            let expectedResponse: IUpload = { id: 123, timeSubmitted: "someTimeSubmitted", playStyle: "somePlayStyle", user: { name: "someUserName", clanName: "someClanName" }, content: "someContent", isScrubbed: false };
            let request = httpMock.expectOne(getApiRequest);
            request.flush(expectedResponse);
            tick();

            expect(upload).toEqual(expectedResponse, "should return the expected upload");

            // Get it again, but no request happens and the same response is returned.
            uploadService.get(123)
                .then((r: IUpload) => upload = r);
            tick();
            expect(upload).toEqual(expectedResponse, "should return the expected upload");
        }));

        it("should clear the cache on user change", fakeAsync(() => {
            let user: IUser = {
                name: "someName",
                clanName: "someClanName",
            };
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getUser").and.returnValue(Promise.resolve(user));

            // Originally logged in
            userInfo.next(loggedInUser);
            tick();
            expect(userService.getUser).toHaveBeenCalled();

            let upload: IUpload;
            uploadService.get(123)
                .then((r: IUpload) => upload = r);

            // Tick the getAuthHeaders call
            tick();

            let expectedResponse1: IUpload = { id: 1, timeSubmitted: "someTimeSubmitted1", playStyle: "somePlayStyle1", user: { name: "someUserName1", clanName: "someClanName1" }, content: "someContent1", isScrubbed: false };
            let request1 = httpMock.expectOne(getApiRequest);
            request1.flush(expectedResponse1);
            tick();

            expect(upload).toEqual(expectedResponse1, "should return the expected upload");

            (userService.getUser as jasmine.Spy).calls.reset();

            // User logs out
            userInfo.next(notLoggedInUser);
            tick();
            expect(userService.getUser).not.toHaveBeenCalled();

            // Get it again, and it's not cached
            uploadService.get(123)
                .then((r: IUpload) => upload = r);

            // Tick the getAuthHeaders call
            tick();

            let expectedResponse2: IUpload = { id: 2, timeSubmitted: "someTimeSubmitted2", playStyle: "somePlayStyle2", user: { name: "someUserName2", clanName: "someClanName2" }, content: "someContent2", isScrubbed: false };
            let request2 = httpMock.expectOne(getApiRequest);
            request2.flush(expectedResponse2);
            tick();

            expect(upload).toEqual(expectedResponse2, "should return the expected upload");
        }));

        it("should cache uploads on creation when logged out", fakeAsync(() => {
            userInfo.next(notLoggedInUser);
            tick();

            let uploadId: number;
            uploadService.create("someEncodedSaveData", false, "somePlayStyle")
                .then((i: number) => uploadId = i);

            // Tick the getAuthHeaders call
            tick();

            let expectedUploadId = 123;
            let request = httpMock.expectOne(createApiRequest);
            request.flush(expectedUploadId);
            tick();

            expect(uploadId).toEqual(expectedUploadId, "should return the expected upload");

            // Get the upload, but no request happens.
            let upload: IUpload;
            uploadService.get(uploadId)
                .then((r: IUpload) => upload = r);
            tick();

            expect(upload).toBeDefined();
            expect(upload.id).toEqual(uploadId);
            expect(upload.content).toEqual("someEncodedSaveData");
            expect(upload.playStyle).toEqual("somePlayStyle");
            expect(upload.isScrubbed).toEqual(false);
            expect(upload.timeSubmitted).toBeDefined();
            expect(upload.user).toBeNull();
        }));

        it("should cache uploads on creation when logged in", fakeAsync(() => {
            let user: IUser = {
                name: "someName",
                clanName: "someClanName",
            };
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getUser").and.returnValue(Promise.resolve(user));

            userInfo.next(loggedInUser);
            tick();
            expect(userService.getUser).toHaveBeenCalled();

            let uploadId: number;
            uploadService.create("someEncodedSaveData", true, "somePlayStyle")
                .then((i: number) => uploadId = i);

            // Tick the getAuthHeaders call
            tick();

            let expectedUploadId = 123;
            let request = httpMock.expectOne(createApiRequest);
            request.flush(expectedUploadId);
            tick();

            expect(uploadId).toEqual(expectedUploadId, "should return the expected upload");

            // Get the upload, but no request happens.
            let upload: IUpload;
            uploadService.get(uploadId)
                .then((r: IUpload) => upload = r);
            tick();

            expect(upload).toBeDefined();
            expect(upload.id).toEqual(uploadId);
            expect(upload.content).toEqual("someEncodedSaveData");
            expect(upload.playStyle).toEqual("somePlayStyle");
            expect(upload.isScrubbed).toEqual(false);
            expect(upload.timeSubmitted).toBeDefined();
            expect(upload.user).toEqual(user);
        }));

        it("should not cache uploads when we're mising current user data", fakeAsync(() => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getUser").and.returnValue(Promise.reject(null));

            userInfo.next(loggedInUser);
            tick();
            expect(userService.getUser).toHaveBeenCalled();

            let uploadId: number;
            uploadService.create("someEncodedSaveData", true, "somePlayStyle")
                .then((i: number) => uploadId = i);

            // Tick the getAuthHeaders call
            tick();

            let expectedUploadId = 123;
            let request1 = httpMock.expectOne(createApiRequest);
            request1.flush(expectedUploadId);
            tick();

            expect(uploadId).toEqual(expectedUploadId, "should return the expected upload");

            // Get the upload, and it's not cached
            let upload: IUpload;
            uploadService.get(uploadId)
                .then((r: IUpload) => upload = r);

            // Tick the getAuthHeaders call
            tick();

            let expectedUpload: IUpload = { id: uploadId, timeSubmitted: "someTimeSubmitted", playStyle: "somePlayStyle", user: { name: "someUserName", clanName: "someClanName" }, content: "someContent", isScrubbed: true };
            let request2 = httpMock.expectOne(getApiRequest);
            request2.flush(expectedUpload);
            tick();

            expect(upload).toEqual(expectedUpload, "should return the expected upload");
        }));

        it("should delete from the cache on upload deletion", fakeAsync(() => {
            let upload: IUpload = null;
            let error: HttpErrorResponse = null;

            uploadService.get(123)
                .then((r: IUpload) => upload = r)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let expectedResponse1: IUpload = { id: 1, timeSubmitted: "someTimeSubmitted1", playStyle: "somePlayStyle1", user: { name: "someUserName1", clanName: "someClanName1" }, content: "someContent1", isScrubbed: false };
            let request1 = httpMock.expectOne(getApiRequest);
            request1.flush(expectedResponse1);
            tick();

            expect(upload).toEqual(expectedResponse1, "should return the expected upload");
            expect(error).toBeNull();

            upload = null;
            error = null;

            // Delete the upload
            uploadService.delete(123);
            tick();
            httpMock.expectOne(deleteApiRequest);

            // Get it again, and it's not cached
            uploadService.get(123)
                .then((r: IUpload) => upload = r)
                .catch((e: HttpErrorResponse) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let request2 = httpMock.expectOne(getApiRequest);
            request2.flush(null, { status: 404, statusText: "someStatus" });
            tick();

            expect(upload).toBeNull();
            expect(error).toBeDefined();
            expect(error.status).toEqual(404);
            expect(error.statusText).toEqual("someStatus");
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("UploadService.get.error", error);
        }));
    });
});
