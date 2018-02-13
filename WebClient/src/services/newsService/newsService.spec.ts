import { TestBed, fakeAsync, tick } from "@angular/core/testing";
import { HttpClientTestingModule, HttpTestingController } from "@angular/common/http/testing";
import { HttpErrorResponse, HttpHeaders } from "@angular/common/http";

import { NewsService, ISiteNewsEntryListResponse, ISiteNewsEntry } from "./newsService";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";
import { AuthenticationService } from "../authenticationService/authenticationService";

describe("NewsService", () => {
    let newsService: NewsService;
    let httpErrorHandlerService: HttpErrorHandlerService;
    let httpMock: HttpTestingController;
    let authenticationService: AuthenticationService;

    beforeEach(() => {
        httpErrorHandlerService = jasmine.createSpyObj("httpErrorHandlerService", ["logError"]);

        authenticationService = jasmine.createSpyObj("authenticationService", ["getAuthHeaders"]);
        (authenticationService.getAuthHeaders as jasmine.Spy).and.returnValue(Promise.resolve(new HttpHeaders()));

        TestBed.configureTestingModule(
            {
                imports: [
                    HttpClientTestingModule,
                ],
                providers:
                    [
                        NewsService,
                        { provide: HttpErrorHandlerService, useValue: httpErrorHandlerService },
                        { provide: AuthenticationService, useValue: authenticationService },
                    ],
            });

        newsService = TestBed.get(NewsService) as NewsService;
        httpMock = TestBed.get(HttpTestingController) as HttpTestingController;
    });

    afterAll(() => {
        httpMock.verify();
    });

    describe("getNews", () => {
        const apiRequest = { method: "get", url: "/api/news" };

        it("should make an api call", fakeAsync(() => {
            newsService.getNews();

            httpMock.expectOne(apiRequest);

            expect(authenticationService.getAuthHeaders).not.toHaveBeenCalled();
        }));

        it("should return some news", fakeAsync(() => {
            let response: ISiteNewsEntryListResponse;
            newsService.getNews()
                .then((r: ISiteNewsEntryListResponse) => response = r);

            let expectedResponse: ISiteNewsEntryListResponse = { entries: { someEntry: ["someEntryValue1", "someEntryValue2"] } };
            let request = httpMock.expectOne(apiRequest);
            request.flush(expectedResponse);
            tick();

            expect(response).toEqual(expectedResponse, "should return the expected response");
            expect(authenticationService.getAuthHeaders).not.toHaveBeenCalled();
        }));

        it("should handle errors", fakeAsync(() => {
            let response: ISiteNewsEntryListResponse;
            let error: HttpErrorResponse;
            newsService.getNews()
                .then((r: ISiteNewsEntryListResponse) => response = r)
                .catch((e: HttpErrorResponse) => error = e);

            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });
            tick();

            expect(response).toBeUndefined();
            expect(authenticationService.getAuthHeaders).not.toHaveBeenCalled();
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("NewsService.getNews.error", error);
        }));
    });

    describe("addNews", () => {
        const apiRequest = { method: "post", url: "/api/news" };
        const newsEntry: ISiteNewsEntry = {
            date: "someDate",
            messages: ["someMessage0", "someMessage1", "someMessage2"],
        };

        it("should make an api call", fakeAsync(() => {
            newsService.addNews(newsEntry);

            // Tick the getAuthHeaders call
            tick();

            let request = httpMock.expectOne(apiRequest);
            expect(request.request.body).toEqual(newsEntry);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let error: HttpErrorResponse;
            newsService.addNews(newsEntry)
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
        }));

        it("should handle errors", fakeAsync(() => {
            let succeeded = false;
            let error: HttpErrorResponse;
            newsService.addNews(newsEntry)
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
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("NewsService.addNews.error", error);
        }));
    });

    describe("deleteNews", () => {
        const date = "someDate";
        const apiRequest = { method: "delete", url: `/api/news/${date}` };

        it("should make an api call", fakeAsync(() => {
            newsService.deleteNews(date);

            // Tick the getAuthHeaders call
            tick();

            httpMock.expectOne(apiRequest);
            expect(authenticationService.getAuthHeaders).toHaveBeenCalled();
        }));

        it("should handle when the api returns a success response", fakeAsync(() => {
            let succeeded = false;
            let error: HttpErrorResponse;
            newsService.deleteNews(date)
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
        }));

        it("should handle errors", fakeAsync(() => {
            let succeeded = false;
            let error: HttpErrorResponse;
            newsService.deleteNews(date)
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
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("NewsService.deleteNews.error", error);
        }));
    });
});
