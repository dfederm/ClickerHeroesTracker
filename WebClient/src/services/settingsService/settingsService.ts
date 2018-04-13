import { Injectable } from "@angular/core";
import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { Observable } from "rxjs/Observable";
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import { Subscription } from "rxjs/Subscription";
import { map } from "rxjs/operators/map";
import { distinctUntilChanged } from "rxjs/operators/distinctUntilChanged";
import { interval } from "rxjs/observable/interval";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";

import "rxjs/add/operator/toPromise";

import { AuthenticationService } from "../authenticationService/authenticationService";

export type PlayStyle = "idle" | "hybrid" | "active";

export type Theme = "light" | "dark";

export interface IUserSettings {
    playStyle: PlayStyle;
    useScientificNotation: boolean;
    scientificNotationThreshold: number;
    useLogarithmicGraphScale: boolean;
    logarithmicGraphScaleThreshold: number;
    hybridRatio: number;
    theme: Theme;
    shouldLevelSkillAncients: boolean;
    skillAncientBaseAncient: number;
    skillAncientLevelDiff: number;
}

@Injectable()
export class SettingsService {
    // Sync settings every hour
    public static syncInterval = 60 * 60 * 1000;

    public static retryDelay = 1000;

    public static readonly defaultSettings: IUserSettings = {
        playStyle: "hybrid",
        useScientificNotation: true,
        scientificNotationThreshold: 1000000,
        useLogarithmicGraphScale: true,
        logarithmicGraphScaleThreshold: 1000000,
        hybridRatio: 2,
        theme: "light",
        shouldLevelSkillAncients: false,
        skillAncientBaseAncient: 17, // Chronos
        skillAncientLevelDiff: 0,
    };

    public static readonly settingsKey: string = "settings";

    private readonly settingsSubject: BehaviorSubject<IUserSettings>;

    private userName: string;

    private refreshSubscription: Subscription;

    private numPendingPatches = 0;

    constructor(
        private readonly authenticationService: AuthenticationService,
        private readonly http: HttpClient,
        private readonly httpErrorHandlerService: HttpErrorHandlerService,
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
                    // Only if the user was previously logged in do we want to clear the settings. Otherwise, just leave the ones read from local storage.
                    if (this.userName) {
                        this.userName = null;
                        localStorage.removeItem(SettingsService.settingsKey);
                        this.settingsSubject.next(this.normalizeSettings(null));
                    }
                }
            });
    }

    public settings(): Observable<IUserSettings> {
        return this.settingsSubject.pipe(
            distinctUntilChanged((x, y) => JSON.stringify(x) === JSON.stringify(y)),
        );
    }

    public setSetting(setting: keyof IUserSettings, value: {}): Promise<void> {
        // While the user is updating settings, cancel any refreshes for now
        if (this.refreshSubscription) {
            this.refreshSubscription.unsubscribe();
            this.refreshSubscription = null;
        }

        let patch = { [setting]: value };
        if (this.userName) {
            this.numPendingPatches++;

            return this.authenticationService.getAuthHeaders()
                .then(headers => {
                    return this.http.patch(`/api/users/${this.userName}/settings`, patch, { headers, responseType: "text" })
                        .toPromise();
                })
                .then(() => {
                    this.handlePatchCompleted();
                })
                .catch((err: HttpErrorResponse) => {
                    this.httpErrorHandlerService.logError("SettingsService.setSetting.error", err);
                    this.handlePatchCompleted();
                    return Promise.reject(err);
                });
        } else {
            let newSettings = Object.assign({}, this.settingsSubject.getValue(), patch);
            this.handleNewSettings(newSettings);
            return Promise.resolve();
        }
    }

    private fetchSettingsInitial(retryDelay: number = SettingsService.retryDelay): void {
        this.fetchSettings()
            .then(() => this.scheduleRefresh())
            .catch(() => {
                // If the user is no longer logged in, just bail
                if (!this.userName) {
                    return;
                }

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
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                if (!this.userName) {
                    return Promise.reject("Not logged in");
                }

                return this.http.get<IUserSettings>(`/api/users/${this.userName}/settings`, { headers })
                    .toPromise();
            })
            .then(newSettings => {
                // If the user is in the process of updating their settings, just ignore this response so it doesn't plow over the newly updated settings.
                // Once the patch finishes, it will refresh again.
                if (this.numPendingPatches !== 0) {
                    return Promise.resolve();
                }

                this.handleNewSettings(newSettings);

                return Promise.resolve();
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("SettingsService.fetchSettings.error", err);
                return Promise.reject(err);
            });
    }

    private handlePatchCompleted(): void {
        this.numPendingPatches--;

        // In case there are multiple patches at once, only refresh the settings from the server and start refreshing again once they're all finished
        if (this.numPendingPatches === 0) {
            this.fetchSettings()
                .catch(() => void 0) // Just swallow errors from the refresh
                .then(() => this.scheduleRefresh());
        }
    }

    private handleNewSettings(newSettings: IUserSettings): void {
        // Store exactly what was given (before normalization) in case the client's defaults change.
        localStorage.setItem(SettingsService.settingsKey, JSON.stringify(newSettings));

        this.settingsSubject.next(this.normalizeSettings(newSettings));
    }

    // In case the settings are missing some values, fill in the defaults
    private normalizeSettings(settings: IUserSettings): IUserSettings {
        return Object.assign({}, SettingsService.defaultSettings, settings);
    }

    private scheduleRefresh(): void {
        this.refreshSubscription = interval(SettingsService.syncInterval).pipe(
            map(() => {
                this.fetchSettings()
                    // Just swallow errors from polling
                    .catch(() => void 0);
            }),
        ).subscribe();
    }
}
