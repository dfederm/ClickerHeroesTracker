import { ReflectiveInjector } from "@angular/core";
import { fakeAsync, tick } from "@angular/core/testing";
import { BaseRequestOptions, ConnectionBackend, Http, Headers, RequestOptions, Response, ResponseOptions, RequestMethod } from "@angular/http";
import { MockBackend, MockConnection } from "@angular/http/testing";
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import { AppInsightsService } from "@markpieszak/ng-application-insights";

import { UploadService } from "./uploadService";
import { AuthenticationService, IUserInfo } from "../authenticationService/authenticationService";
import { IUpload } from "../../models";

describe("UploadService", () => {
    let uploadService: UploadService;
    let authenticationService: AuthenticationService;
    let appInsights: AppInsightsService;
    let backend: MockBackend;
    let lastConnection: MockConnection;
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
        (authenticationService.getAuthHeaders as jasmine.Spy).and.returnValue(Promise.resolve(new Headers()));

        appInsights = jasmine.createSpyObj("appInsights", ["trackEvent"]);

        let injector = ReflectiveInjector.resolveAndCreate(
            [
                UploadService,
                { provide: ConnectionBackend, useClass: MockBackend },
                { provide: RequestOptions, useClass: BaseRequestOptions },
                Http,
                { provide: AuthenticationService, useValue: authenticationService },
                { provide: AppInsightsService, useValue: appInsights },
            ]);

        uploadService = injector.get(UploadService) as UploadService;
        backend = injector.get(ConnectionBackend) as MockBackend;
        backend.connections.subscribe((connection: MockConnection) => lastConnection = connection);
    });

    afterEach(() => {
        lastConnection = null;
        backend.verifyNoPendingRequests();
    });

    describe("get", () => {
        it("should make the correct api call", fakeAsync(() => {
            uploadService.get(123);

            // Tick the getAuthHeaders call
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/uploads/123", "url invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return an upload", fakeAsync(() => {
            let upload: IUpload;
            uploadService.get(123)
                .then((r: IUpload) => upload = r);

            // Tick the getAuthHeaders call
            tick();

            let expectedUpload: IUpload = { id: 123, timeSubmitted: "someTimeSubmitted", playStyle: "somePlayStyle" };
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(expectedUpload) })));
            tick();

            expect(upload).toEqual(expectedUpload, "should return the expected upload");
        }));

        it("should handle http errors", fakeAsync(() => {
            let upload: IUpload;
            let error: string;
            uploadService.get(123)
                .then((r: IUpload) => upload = r)
                .catch((e: string) => error = e);

            // Tick the getAuthHeaders call
            tick();

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(upload).toBeUndefined();
            expect(error).toEqual("someError");
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });

    describe("create", () => {
        it("should make the correct api call when the use is not logged in", fakeAsync(() => {
            userInfo.next(notLoggedInUser);

            uploadService.create("someEncodedSaveData", true, "somePlayStyle");

            // Tick the getAuthHeaders call
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/uploads", "url invalid");
            expect(lastConnection.request.text()).toEqual("encodedSaveData=someEncodedSaveData&addToProgress=false&playStyle=somePlayStyle", "request body invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should make the correct api call when the use is logged in", fakeAsync(() => {
            userInfo.next(loggedInUser);

            uploadService.create("someEncodedSaveData", true, "somePlayStyle");

            // Tick the getAuthHeaders call
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/uploads", "url invalid");
            expect(lastConnection.request.text()).toEqual("encodedSaveData=someEncodedSaveData&addToProgress=true&playStyle=somePlayStyle", "request body invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let uploadId: number;
            let error: Error;
            uploadService.create("someEncodedSaveData", true, "somePlayStyle")
                .then((id: number) => uploadId = id)
                .catch((e: Error) => error = e);

            // Tick the getAuthHeaders call
            tick();

            let expectedError = new Error("someError");
            lastConnection.mockError(expectedError);
            tick();

            expect(uploadId).toBeUndefined();
            expect(error).toEqual(expectedError);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });

    describe("delete", () => {
        it("should make an api call", fakeAsync(() => {
            uploadService.delete(123);

            // Tick the getAuthHeaders call
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Delete, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/uploads/123", "url invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() => {
            let succeeded = false;
            let error: string;
            uploadService.delete(123)
                .then(() => succeeded = true)
                .catch((e: string) => error = e);

            // Tick the getAuthHeaders call
            tick();

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(succeeded).toEqual(false);
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });
});
