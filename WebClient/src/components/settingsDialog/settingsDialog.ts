import { Component, OnInit } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

import { SettingsService, PlayStyle, Theme, IUserSettings } from "../../services/settingsService/settingsService";

@Component({
    selector: "settingsDialog",
    templateUrl: "./settingsDialog.html",
})
export class SettingsDialogComponent implements OnInit {
    public error: string;

    public settings: IUserSettings;

    public isSettingPending: { [setting: string]: boolean } = {};

    public playStyles: PlayStyle[] = ["idle", "hybrid", "active"];

    public themes: Theme[] = ["light", "dark"];

    constructor(
        private settingsService: SettingsService,
        public activeModal: NgbActiveModal,
    ) { }

    public ngOnInit(): void {
        this.settingsService
            .settings()
            .subscribe(settings => {
                // Make a copy so the 2-way bindings don't mutate the actual settings object.
                this.settings = JSON.parse(JSON.stringify(settings));
            });
    }

    public setSetting(setting: keyof IUserSettings, value: {}): void {
        this.error = null;
        this.isSettingPending[setting] = true;
        this.settingsService.setSetting(setting, value)
            .then(() => {
                this.isSettingPending[setting] = false;
            }).catch(() => {
                this.error = "We ran into a problem and some of your settings may have reset. Please try again.";
                this.isSettingPending[setting] = false;
            });
    }
}
