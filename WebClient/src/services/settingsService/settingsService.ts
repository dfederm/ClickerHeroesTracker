import { Injectable } from "@angular/core";
import { Http, RequestOptions } from "@angular/http";
import { Observable } from "rxjs/Observable";
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import { Subscription } from "rxjs/Subscription";

import "rxjs/add/operator/toPromise";
import "rxjs/add/operator/map";
import "rxjs/add/observable/interval";

import { AuthenticationService } from "../authenticationService/authenticationService";

export type PlayStyle = "idle" | "hybrid" | "active";

export type Theme = "light" | "dark";

export interface IUserSettings {
    areUploadsPublic: boolean;
    playStyle: PlayStyle;
    useScientificNotation: boolean;
    scientificNotationThreshold: number;
    useEffectiveLevelForSuggestions: boolean;
    useLogarithmicGraphScale: boolean;
    logarithmicGraphScaleThreshold: number;
    hybridRatio: number;
    theme: Theme;
}

@Injectable()
export class SettingsService {
    // Sync settings every 5 minutes
    public static syncInterval = 5 * 60 * 1000;

    public static retryDelay = 1000;

    public static readonly defaultSettings: IUserSettings = {
        areUploadsPublic: true,
        playStyle: "hybrid",
        useScientificNotation: true,
        scientificNotationThreshold: 1000000,
        useEffectiveLevelForSuggestions: false,
        useLogarithmicGraphScale: true,
        logarithmicGraphScaleThreshold: 1000000,
        hybridRatio: 2,
        theme: "light",
    };

    public static readonly settingsKey: string = "settings";

    private settingsSubject: BehaviorSubject<IUserSettings>;

    private userName: string;

    private refreshSubscription: Subscription;

    constructor(
        private authenticationService: AuthenticationService,
        private http: Http,
    ) {
        let settingsString = localStorage.getItem(SettingsService.settingsKey);
        let currentSettings = settingsString == null ? null : JSON.parse(settingsString);
        this.settingsSubject = new BehaviorSubject(this.normalizeSettings(currentSettings));

        this.authenticationService
            .userInfo()
            .subscribe(userInfo => {
                // Reset the subscription
                if (this.refreshSubscription) {
                    this.refreshSubscription.unsubscribe();
                    this.refreshSubscription = null;
                }

                if (userInfo.isLoggedIn) {
                    this.userName = userInfo.username;
                    this.fetchSettingsInitial();
                } else {
                    this.userName = null;
                    localStorage.removeItem(SettingsService.settingsKey);
                    this.settingsSubject.next(this.normalizeSettings(null));
                }
            });
    }

    public settings(): Observable<IUserSettings> {
        return this.settingsSubject
            .distinctUntilChanged((x, y) => JSON.stringify(x) === JSON.stringify(y));
    }

    private fetchSettingsInitial(retryDelay: number = SettingsService.retryDelay): void {
        this.fetchSettings()
            .then(() => this.scheduleRefresh())
            .catch(() => {
                // If the initial fetch fails, retry after a delay
                setTimeout(
                    (newDelay: number) => this.fetchSettingsInitial(newDelay),
                    retryDelay,
                    // Exponential backoff, max out at the sync interval
                    Math.min(2 * retryDelay, SettingsService.syncInterval),
                );
            });
    }

    private fetchSettings(): Promise<void> {
        let headers = this.authenticationService.getAuthHeaders();
        let options = new RequestOptions({ headers });
        return this.http.get(`/api/users/${this.userName}/settings`, options)
            .toPromise()
            .then(response => {
                let newSettings: IUserSettings = response.json();
                if (!newSettings) {
                    return Promise.reject("Invalid settings response");
                }

                // Only store exactly what was sent back in case the client's defaults change.
                localStorage.setItem(SettingsService.settingsKey, JSON.stringify(newSettings));

                this.settingsSubject.next(this.normalizeSettings(newSettings));
                return Promise.resolve();
            });
    }

    // In case the settings are missing some values, fill in the defaults
    private normalizeSettings(settings: IUserSettings): IUserSettings {
        return Object.assign({}, SettingsService.defaultSettings, settings);
    }

    private scheduleRefresh(): void {
        this.refreshSubscription = Observable.interval(SettingsService.syncInterval)
            .map(() => {
                this.fetchSettings()
                    // Just swallow errors from polling
                    .catch(() => void 0);
            })
            .subscribe();
    }
}
