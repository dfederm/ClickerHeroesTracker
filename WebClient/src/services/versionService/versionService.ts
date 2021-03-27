import { Injectable } from "@angular/core";
import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { Observable, BehaviorSubject, interval } from "rxjs";
import { filter, distinctUntilChanged } from "rxjs/operators";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";

export interface IVersion {
    environment: string;
    changelist: string;
    buildUrl: string;
    webclient: { [bundle: string]: string };
}

@Injectable({
    providedIn: "root",
})
export class VersionService {
    // Poll the version every hour
    public static pollingInterval = 60 * 60 * 1000;

    public static retryDelay = 1000;

    private readonly version: BehaviorSubject<IVersion>;

    constructor(
        private readonly http: HttpClient,
        private readonly httpErrorHandlerService: HttpErrorHandlerService,
    ) {
        this.version = new BehaviorSubject(null);

        /*
            Note that these is a (fairly rare) race condition here. If the inital fetch of
            index.html referenced one version of the app, but then a deployment happened before
            this fetch to the version endpoint, this will report a version that is newer than
            what's running. Ideally we'd figure out the iniital version from index.html in some way.
        */
        this.fetchVersionInitial();
    }

    public getVersion(): Observable<IVersion> {
        return this.version.pipe(
            filter(version => version != null),
            distinctUntilChanged((x, y) => JSON.stringify(x) === JSON.stringify(y)),
        );
    }

    private fetchVersionInitial(retryDelay: number = VersionService.retryDelay): void {
        this.fetchVersion()
            .then(() => this.scheduleRefresh())
            .catch(() => {
                // If the initial fetch fails, retry after a delay
                setTimeout(
                    (newDelay: number) => this.fetchVersionInitial(newDelay),
                    retryDelay,
                    // Exponential backoff, max out at the polling interval
                    Math.min(2 * retryDelay, VersionService.pollingInterval),
                );
            });
    }

    private fetchVersion(): Promise<void> {
        return this.http.get<IVersion>("/version")
            .toPromise()
            .then(version => {
                this.version.next(version);
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("VersionService.fetchVersion.error", err);
                return Promise.reject(err);
            });
    }

    private scheduleRefresh(): void {
        interval(VersionService.pollingInterval)
            .forEach(() => {
                this.fetchVersion()
                    // Just swallow errors from polling
                    .catch(() => void 0);
            });
    }
}
