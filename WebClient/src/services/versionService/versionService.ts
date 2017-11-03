import { Injectable } from "@angular/core";
import { Http } from "@angular/http";
import { Observable } from "rxjs/Observable";
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import { filter, distinctUntilChanged } from "rxjs/operators";
import { interval } from "rxjs/observable/interval";

import "rxjs/add/operator/toPromise";

export interface IVersion {
    environment: string;
    changelist: string;
    buildId: string;
    webclient: { [bundle: string]: string };
}

@Injectable()
export class VersionService {
    // Poll the version every 5 minutes
    public static pollingInterval = 5 * 60 * 1000;

    public static retryDelay = 1000;

    private version: BehaviorSubject<IVersion>;

    constructor(private http: Http) {
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
        return this.http.get("/version")
            .toPromise()
            .then(response => {
                let version: IVersion = response.json();
                if (!version) {
                    return Promise.reject("Invalid version response");
                }

                this.version.next(version);
                return Promise.resolve();
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
