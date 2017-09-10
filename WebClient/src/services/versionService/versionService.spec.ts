import { ReflectiveInjector } from "@angular/core";
import { fakeAsync, tick, discardPeriodicTasks } from "@angular/core/testing";
import { BaseRequestOptions, ConnectionBackend, Http, RequestOptions } from "@angular/http";
import { Response, ResponseOptions, RequestMethod } from "@angular/http";
import { MockBackend, MockConnection } from "@angular/http/testing";

import { VersionService, IVersion } from "./versionService";

describe("VersionService", () => {
    let versionService: VersionService;
    let backend: MockBackend;
    let lastConnection: MockConnection = null;

    beforeEach(() => {
        let injector = ReflectiveInjector.resolveAndCreate(
            [
                { provide: ConnectionBackend, useClass: MockBackend },
                { provide: RequestOptions, useClass: BaseRequestOptions },
                Http,
                VersionService,
            ]);

        backend = injector.get(ConnectionBackend) as MockBackend;
        backend.connections.subscribe((connection: MockConnection) => {
            if (lastConnection != null) {
                fail("Previous connection not handled");
            }

            lastConnection = connection;
        });

        versionService = injector.get(VersionService) as VersionService;
    });

    afterEach(() => {
        lastConnection = null;
        backend.verifyNoPendingRequests();
    });

    describe("getVersion", () => {
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

        function verifyLastConnection(): void {
            expect(lastConnection).not.toBeNull("no http service connection made");
            expect(lastConnection.request.method).toEqual(RequestMethod.Get, "method invalid");
            expect(lastConnection.request.url).toEqual("/version", "url invalid");
        }

        function respondToLastConnection(index: number): IVersion {
            verifyLastConnection();

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

            lastConnection.mockRespond(new Response(new ResponseOptions({ body: JSON.stringify(version) })));
            lastConnection = null;

            // Don't tick longer than the refresh interval
            tick(1);

            return version;
        }

        function respondEmptyToLastConnection(): void {
            verifyLastConnection();

            lastConnection.mockRespond(new Response(new ResponseOptions({ body: "" })));
            lastConnection = null;

            // Don't tick longer than the refresh interval
            tick(1);
        }

        function errorToLastConnection(): void {
            verifyLastConnection();

            lastConnection.mockError();
            lastConnection = null;

            // Don't tick longer than the refresh interval
            tick(1);
        }
    });
});
