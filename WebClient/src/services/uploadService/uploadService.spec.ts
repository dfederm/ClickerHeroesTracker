import { TestBed, fakeAsync, tick } from "@angular/core/testing";
import { HttpClientTestingModule, HttpTestingController } from "@angular/common/http/testing";
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";
import { HttpHeaders, HttpErrorResponse } from "@angular/common/http";

import { UploadService, IUpload } from "./uploadService";
import { AuthenticationService, IUserInfo } from "../authenticationService/authenticationService";

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
                    ],
            });

        uploadService = TestBed.get(UploadService) as UploadService;
        httpMock = TestBed.get(HttpTestingController) as HttpTestingController;
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

        it("should make the correct api call when the use is not logged in", fakeAsync(() => {
            userInfo.next(notLoggedInUser);

            uploadService.create("someEncodedSaveData", true, "somePlayStyle");

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            expect(request.request.body).toEqual("encodedSaveData=someEncodedSaveData&addToProgress=false&playStyle=somePlayStyle");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should make the correct api call when the use is logged in", fakeAsync(() => {
            userInfo.next(loggedInUser);

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
});
