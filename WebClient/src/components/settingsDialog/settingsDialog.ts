import { Component, OnInit } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

import { SettingsService, PlayStyle, Theme, IUserSettings, GraphSpacingType } from "../../services/settingsService/settingsService";
import { ChangePasswordDialogComponent } from "../changePasswordDialog/changePasswordDialog";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";
import { OpenDialogDirective } from "src/directives/openDialog/openDialog";
import { FormsModule } from "@angular/forms";
import { TitleCasePipe } from "@angular/common";

@Component({
    selector: "settingsDialog",
    templateUrl: "./settingsDialog.html",
    imports: [
        FormsModule,
        NgxSpinnerModule,
        OpenDialogDirective,
        TitleCasePipe,
    ]
})
export class SettingsDialogComponent implements OnInit {
    public error: string;

    public settings: IUserSettings;

    public isSettingPending: { [setting: string]: boolean } = {};

    public playStyles: PlayStyle[] = ["idle", "hybrid", "active"];

    public themes: Theme[] = ["light", "dark"];

    public graphSpacingTypes: GraphSpacingType[] = ["time", "ascension"];

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
        private readonly settingsService: SettingsService,
        public activeModal: NgbActiveModal,
        private readonly spinnerService: NgxSpinnerService,
    ) { }

    public ngOnInit(): void {
        this.spinnerService.show("settingsDialog");
        this.settingsService
            .settings()
            .subscribe(settings => {
                this.spinnerService.hide("settingsDialog");

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
