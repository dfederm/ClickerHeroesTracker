import { Component, OnInit } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

import { SettingsService, PlayStyle, Theme, IUserSettings } from "../../services/settingsService/settingsService";
import { ChangePasswordDialogComponent } from "../changePasswordDialog/changePasswordDialog";

@Component({
    selector: "settingsDialog",
    templateUrl: "./settingsDialog.html",
})
export class SettingsDialogComponent implements OnInit {
    public error: string;

    public isLoading: boolean;

    public settings: IUserSettings;

    public isSettingPending: { [setting: string]: boolean } = {};

    public playStyles: PlayStyle[] = ["idle", "hybrid", "active"];

    public themes: Theme[] = ["light", "dark"];

    public skillAncientBaseAncients: { name: string, id: number }[] = [
        { name: "Atman", id: 13 },
        { name: "Bubos", id: 18 },
        { name: "Chronos", id: 17 },
        { name: "Dogcog", id: 11 },
        { name: "Dora", id: 14 },
        { name: "Fortuna", id: 12 },
        { name: "Kumawakamaru", id: 21 },
    ];

    public ChangePasswordDialogComponent = ChangePasswordDialogComponent;

    constructor(
        private settingsService: SettingsService,
        public activeModal: NgbActiveModal,
    ) { }

    public ngOnInit(): void {
        this.isLoading = true;
        this.settingsService
            .settings()
            .subscribe(settings => {
                this.isLoading = false;

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
