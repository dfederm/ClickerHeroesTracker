import { TestBed, fakeAsync, tick, discardPeriodicTasks } from "@angular/core/testing";
import { HttpClientTestingModule, HttpTestingController } from "@angular/common/http/testing";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";

import { VersionService, IVersion } from "./versionService";

describe("VersionService", () => {
    let versionService: VersionService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        let httpErrorHandlerService = {
            logError: (): void => void 0,
        };

        TestBed.configureTestingModule(
            {
                imports: [
                    HttpClientTestingModule,
                ],
                providers: [
                    VersionService,
                    { provide: HttpErrorHandlerService, useValue: httpErrorHandlerService },
                ],
            });

        httpMock = TestBed.get(HttpTestingController) as HttpTestingController;
        versionService = TestBed.get(VersionService) as VersionService;
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

            // An interval was started, so abandon it.
            discardPeriodicTasks();
        }));

        it("should update the version when it changes", fakeAsync(() => {
            let expectedVersion0 = respondToLastConnection(0);

            // Let the polling interval tick
            tick(VersionService.pollingInterval);

            let expectedVersion1 = respondToLastConnection(1);

            expect(versionLog.length).toEqual(2);
            expect(versionLog[0]).toEqual(expectedVersion0);
            expect(versionLog[1]).toEqual(expectedVersion1);

            // An interval was started, so abandon it.
            discardPeriodicTasks();
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

            // An interval was started, so abandon it.
            discardPeriodicTasks();
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

            // Let the polling interval lapse a whole bunch more times with an empty version
            for (let i = 0; i < 100; i++) {
                tick(VersionService.pollingInterval);
                respondEmptyToLastConnection();
            }

            // Let the polling interval tick again
            tick(VersionService.pollingInterval);

            let expectedVersion2 = respondToLastConnection(2);

            expect(versionLog.length).toEqual(3);
            expect(versionLog[0]).toEqual(expectedVersion0);
            expect(versionLog[1]).toEqual(expectedVersion1);
            expect(versionLog[2]).toEqual(expectedVersion2);

            // An interval was started, so abandon it.
            discardPeriodicTasks();
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

            // An interval was started, so abandon it.
            discardPeriodicTasks();
        }));

        function respondToLastConnection(index: number): IVersion {
            let version = {
                environment: "environment_" + index,
                changelist: "changelist_" + index,
                buildId: "buildId_" + index,
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

        function respondEmptyToLastConnection(): void {
            let request = httpMock.expectOne(apiRequest);
            request.flush("");

            // Don't tick longer than the refresh interval
            tick(1);
        }

        function errorToLastConnection(): void {
            let request = httpMock.expectOne(apiRequest);
            request.flush(null, { status: 500, statusText: "someStatus" });

            // Don't tick longer than the refresh interval
            tick(1);
        }
    });
});
