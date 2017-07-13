import { ReflectiveInjector } from "@angular/core";
import { fakeAsync, tick } from "@angular/core/testing";
import { BaseRequestOptions, ConnectionBackend, Http, RequestOptions } from "@angular/http";
import { Response, ResponseOptions, RequestMethod } from "@angular/http";
import { MockBackend, MockConnection } from "@angular/http/testing";
import { BehaviorSubject } from "rxjs/BehaviorSubject";

import { UploadService } from "./uploadService";
import { AuthenticationService } from "../authenticationService/authenticationService";

declare global
{
    // tslint:disable-next-line:interface-name - We don't own this interface name, just extending it
    interface Window
    {
        appInsights: Microsoft.ApplicationInsights.IAppInsights;
    }
}

describe("UploadService", () =>
{
    let uploadService: UploadService;
    let authenticationService: AuthenticationService;
    let backend: MockBackend;
    let lastConnection: MockConnection;
    let isLoggedIn: BehaviorSubject<boolean>;

    beforeEach(() =>
    {
        isLoggedIn = new BehaviorSubject(false);
        authenticationService = jasmine.createSpyObj("authenticationService", ["isLoggedIn", "getAuthHeaders"]);
        (authenticationService.isLoggedIn as jasmine.Spy).and.returnValue(isLoggedIn);
        (authenticationService.getAuthHeaders as jasmine.Spy).and.returnValue(Promise.resolve(new Headers()));

        let injector = ReflectiveInjector.resolveAndCreate(
            [
                UploadService,
                { provide: ConnectionBackend, useClass: MockBackend },
                { provide: RequestOptions, useClass: BaseRequestOptions },
                Http,
                { provide: AuthenticationService, useValue: authenticationService },
            ]);

        uploadService = injector.get(UploadService) as UploadService;
        backend = injector.get(ConnectionBackend) as MockBackend;
        backend.connections.subscribe((connection: MockConnection) => lastConnection = connection);

        // Mock the global variable. We should figure out a better way to both inject this in the product and mock this in tests.
        window.appInsights = jasmine.createSpyObj("appInsights", ["trackEvent"]);
    });

    afterEach(() =>
    {
        lastConnection = null;
        backend.verifyNoPendingRequests();
    });

    describe("getUploads", () =>
    {
        it("should make an api call", fakeAsync(() =>
        {
            uploadService.getUploads(1, 2);
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/uploads?page=1&count=2", "url invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return some uploads", fakeAsync(() =>
        {
            let response: IUploadSummaryListResponse;
            uploadService.getUploads(1, 2)
                .then((r: IUploadSummaryListResponse) => response = r);
            tick();

            let expectedResponse: IUploadSummaryListResponse = { pagination: { count: 0, next: "", previous: "" }, uploads: [] };
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(expectedResponse) })));
            tick();

            expect(response).toEqual(expectedResponse, "should return the expected response");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle errors from authenticationService.getAuthHeaders", fakeAsync(() =>
        {
            (authenticationService.getAuthHeaders as jasmine.Spy).and.callFake(() => Promise.reject("someError"));

            isLoggedIn.next(true);
            tick();

            let response: IUploadSummaryListResponse;
            let error: string;
            uploadService.getUploads(1, 1)
                .then((r: IUploadSummaryListResponse) => response = r)
                .catch((e: string) => error = e);
            tick();

            expect(response).toBeUndefined();
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() =>
        {
            let response: IUploadSummaryListResponse;
            let error: string;
            uploadService.getUploads(1, 1)
                .then((r: IUploadSummaryListResponse) => response = r)
                .catch((e: string) => error = e);
            tick();

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(response).toBeUndefined();
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });

    describe("get", () =>
    {
        it("should make the correct api call when the use is not logged in", fakeAsync(() =>
        {
            isLoggedIn.next(false);
            tick();

            uploadService.get(123);
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/uploads/123", "url invalid");
            expect(authenticationService.getAuthHeaders).not.toHaveBeenCalled();
        }));

        it("should make the correct api call when the use is logged in", fakeAsync(() =>
        {
            isLoggedIn.next(true);
            tick();

            uploadService.get(123);
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/uploads/123", "url invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should return an upload", fakeAsync(() =>
        {
            let upload: IUpload;
            uploadService.get(123)
                .then((r: IUpload) => upload = r);
            tick();

            let expectedUpload: IUpload = { id: 123, timeSubmitted: "someTimeSubmitted", playStyle: "somePlayStyle" };
            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(expectedUpload) })));
            tick();

            expect(upload).toEqual(expectedUpload, "should return the expected upload");
        }));

        it("should handle errors from authenticationService.getAuthHeaders", fakeAsync(() =>
        {
            (authenticationService.getAuthHeaders as jasmine.Spy).and.callFake(() => Promise.reject("someError"));

            isLoggedIn.next(true);
            tick();

            let upload: IUpload;
            let error: string;
            uploadService.get(123)
                .then((r: IUpload) => upload = r)
                .catch((e: string) => error = e);
            tick();

            expect(upload).toBeUndefined();
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() =>
        {
            let upload: IUpload;
            let error: string;
            uploadService.get(123)
                .then((r: IUpload) => upload = r)
                .catch((e: string) => error = e);
            tick();

            lastConnection.mockError(new Error("someError"));
            tick();

            expect(upload).toBeUndefined();
            expect(error).toEqual("someError");
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));
    });

    describe("create", () =>
    {
        it("should make the correct api call when the use is not logged in", fakeAsync(() =>
        {
            isLoggedIn.next(false);
            tick();

            uploadService.create("someEncodedSaveData", true, "somePlayStyle");
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/uploads", "url invalid");
            expect(lastConnection.request.text()).toEqual("encodedSaveData=someEncodedSaveData&addToProgress=false&playStyle=somePlayStyle", "request body invalid");
            expect(authenticationService.getAuthHeaders).not.toHaveBeenCalled();
        }));

        it("should make the correct api call when the use is logged in", fakeAsync(() =>
        {
            isLoggedIn.next(true);
            tick();

            uploadService.create("someEncodedSaveData", true, "somePlayStyle");
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Post, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/uploads", "url invalid");
            expect(lastConnection.request.text()).toEqual("encodedSaveData=someEncodedSaveData&addToProgress=true&playStyle=somePlayStyle", "request body invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle errors from authenticationService.getAuthHeaders", fakeAsync(() =>
        {
            (authenticationService.getAuthHeaders as jasmine.Spy).and.callFake(() => Promise.reject("someError"));

            isLoggedIn.next(true);
            tick();

            let uploadId: number;
            let error: string;
            uploadService.create("someEncodedSaveData", true, "somePlayStyle")
                .then((id: number) => uploadId = id)
                .catch((e: string) => error = e);
            tick();

            expect(uploadId).toBeUndefined();
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() =>
        {
            isLoggedIn.next(true);
            tick();

            let uploadId: number;
            let error: Error;
            uploadService.create("someEncodedSaveData", true, "somePlayStyle")
                .then((id: number) => uploadId = id)
                .catch((e: Error) => error = e);
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

    describe("delete", () =>
    {
        it("should make an api call", fakeAsync(() =>
        {
            uploadService.delete(123);
            tick();

            expect(lastConnection).toBeDefined("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Delete, "method invalid");
            expect(lastConnection.request.url).toEqual("/api/uploads/123", "url invalid");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle errors from authenticationService.getAuthHeaders", fakeAsync(() =>
        {
            (authenticationService.getAuthHeaders as jasmine.Spy).and.callFake(() => Promise.reject("someError"));

            isLoggedIn.next(true);
            tick();

            let succeeded = false;
            let error: string;
            uploadService.delete(123)
                .then(() => succeeded = true)
                .catch((e: string) => error = e);
            tick();

            expect(succeeded).toEqual(false);
            expect(error).toEqual("someError");
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
            expect(appInsights.trackEvent).toHaveBeenCalled();
        }));

        it("should handle http errors", fakeAsync(() =>
        {
            let succeeded = false;
            let error: string;
            uploadService.delete(123)
                .then(() => succeeded = true)
                .catch((e: string) => error = e);
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
