import { TestBed, fakeAsync, tick } from "@angular/core/testing";
import { HttpClientTestingModule, HttpTestingController } from "@angular/common/http/testing";
import { HttpErrorResponse } from "@angular/common/http";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";

import { NewsService, ISiteNewsEntryListResponse } from "./newsService";

describe("NewsService", () => {
    let newsService: NewsService;
    let httpErrorHandlerService: HttpErrorHandlerService;
    let httpMock: HttpTestingController;

    describe("getNews", () => {
        const apiRequest = { method: "get", url: "/api/news" };

        beforeEach(() => {
            httpErrorHandlerService = jasmine.createSpyObj("httpErrorHandlerService", ["logError"]);

            TestBed.configureTestingModule(
                {
                    imports: [
                        HttpClientTestingModule,
                    ],
                    providers:
                        [
                            NewsService,
                            { provide: HttpErrorHandlerService, useValue: httpErrorHandlerService },
                        ],
                });

            newsService = TestBed.get(NewsService) as NewsService;
            httpMock = TestBed.get(HttpTestingController) as HttpTestingController;
        });

        afterAll(() => {
            httpMock.verify();
        });

        it("should make an api call", () => {
            newsService.getNews();
            httpMock.expectOne(apiRequest);
        });

        it("should return some news", fakeAsync(() => {
            let response: ISiteNewsEntryListResponse;
            newsService.getNews()
                .then((r: ISiteNewsEntryListResponse) => response = r);

            let expectedResponse: ISiteNewsEntryListResponse = { entries: { someEntry: ["someEntryValue1", "someEntryValue2"] } };
            let request = httpMock.expectOne(apiRequest);
            request.flush(expectedResponse);
            tick();

            expect(response).toEqual(expectedResponse, "should return the expected response");
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
            expect(error).toBeDefined();
            expect(error.status).toEqual(500);
            expect(error.statusText).toEqual("someStatus");
            expect(httpErrorHandlerService.logError).toHaveBeenCalledWith("NewsService.getNews.error", error);
        }));
    });
});
