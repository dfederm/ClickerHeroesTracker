import { TestBed, fakeAsync, tick } from "@angular/core/testing";
import { HttpTestingController, provideHttpClientTesting } from "@angular/common/http/testing";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";

import { VersionService, IVersion } from "./versionService";
import { provideHttpClient } from "@angular/common/http";

describe("VersionService", () => {
    let versionService: VersionService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        let httpErrorHandlerService = {
            logError: (): void => void 0,
        };

        TestBed.configureTestingModule({
            providers: [
                VersionService,
                provideHttpClient(),
                provideHttpClientTesting(),
                { provide: HttpErrorHandlerService, useValue: httpErrorHandlerService },
            ],
        });

        httpMock = TestBed.inject(HttpTestingController);
        versionService = TestBed.inject(VersionService);
    });

    afterEach(() => {
        httpMock.verify();
    });

    describe("getVersion", () => {
        const apiRequest = { method: "get", url: "/version" };
        let versionLog: IVersion[];

        beforeEach(() => {
            versionLog = [];
            versionService.getVersion().subscribe(version => {
                versionLog.push(version);
            });
        });

        it("should return initial version", fakeAsync(() => {
            let expectedVersion = respondToLastConnection(0);

            expect(versionLog.length).toEqual(1);
            expect(versionLog[0]).toEqual(expectedVersion);

            versionService.ngOnDestroy();
        }));

        it("should update the version when it changes", fakeAsync(() => {
            let expectedVersion0 = respondToLastConnection(0);

            // Let the polling interval tick
            tick(VersionService.pollingInterval);

            let expectedVersion1 = respondToLastConnection(1);

            expect(versionLog.length).toEqual(2);
            expect(versionLog[0]).toEqual(expectedVersion0);
            expect(versionLog[1]).toEqual(expectedVersion1);

            versionService.ngOnDestroy();
        }));

        it("should not update the version when it doesn't change", fakeAsync(() => {
            let expectedVersion0 = respondToLastConnection(0);

            // Let the polling interval tick
            tick(VersionService.pollingInterval);

            let expectedVersion1 = respondToLastConnection(1);

            // Let the polling interval lapse a whole bunch more times with the same version
            for (let i = 0; i < 100; i++) {
                tick(VersionService.pollingInterval);
                respondToLastConnection(1);
            }

            // Let the polling interval tick again
            tick(VersionService.pollingInterval);

            let expectedVersion2 = respondToLastConnection(2);

            expect(versionLog.length).toEqual(3);
            expect(versionLog[0]).toEqual(expectedVersion0);
            expect(versionLog[1]).toEqual(expectedVersion1);
            expect(versionLog[2]).toEqual(expectedVersion2);

            versionService.ngOnDestroy();
        }));

        it("should not update the version when it errors", fakeAsync(() => {
            let expectedVersion0 = respondToLastConnection(0);

            // Let the polling interval tick
            tick(VersionService.pollingInterval);

            let expectedVersion1 = respondToLastConnection(1);

            // Let the polling interval lapse a whole bunch more times with an error
            for (let i = 0; i < 100; i++) {
                tick(VersionService.pollingInterval);
                errorToLastConnection();
            }

            // Let the polling interval tick again
            tick(VersionService.pollingInterval);

            let expectedVersion2 = respondToLastConnection(2);

            expect(versionLog.length).toEqual(3);
            expect(versionLog[0]).toEqual(expectedVersion0);
            expect(versionLog[1]).toEqual(expectedVersion1);
            expect(versionLog[2]).toEqual(expectedVersion2);

            versionService.ngOnDestroy();
        }));

        it("should retry the initial fetch", fakeAsync(() => {
            let retryDelay = VersionService.retryDelay;

            // Retry and fail a bunch of times
            const numRetries = 100;
            for (let i = 0; i < numRetries; i++) {
                errorToLastConnection();
                tick(retryDelay);

                // Exponential backoff. This is tested since the last connection throws if the one before it isn't handled.
                retryDelay = Math.min(2 * retryDelay, VersionService.pollingInterval);
            }

            // Eventually succeed
            let expectedVersion = respondToLastConnection(0);

            expect(versionLog.length).toEqual(1);
            expect(versionLog[0]).toEqual(expectedVersion);

            versionService.ngOnDestroy();
        }));

        function respondToLastConnection(index: number): IVersion {
            let version = {
                environment: "environment_" + index,
                changelist: "changelist_" + index,
                buildUrl: "buildUrl_" + index,
                webclient: {
                    bundle1: "bundle1_" + index,
                    bundle2: "bundle2_" + index,
                    bundle3: "bundle3_" + index,
                },
            };

            let request = httpMock.expectOne(apiRequest);
            request.flush(version);

            // Don't tick longer than the refresh interval
            tick(1);

            return version;
        }

        function errorToLastConnection(): void {
            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });

            // Don't tick longer than the refresh interval
            tick(1);
        }
    });
});
