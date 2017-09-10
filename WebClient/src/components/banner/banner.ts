import { Component, OnInit, OnDestroy } from "@angular/core";
import { Subscription } from "rxjs/Subscription";
import { VersionService } from "../../services/versionService/versionService";

@Component({
    selector: "banner",
    templateUrl: "./banner.html",
})
export class BannerComponent implements OnInit, OnDestroy {
    public showReloadBanner = false;

    private subscription: Subscription;

    private initialVersion: { [bundle: string]: string };

    constructor(
        private versionService: VersionService,
    ) { }

    public ngOnInit(): void {
        this.subscription = this.versionService.getVersion()
            .subscribe(version => {
                // We only care about webclient versions
                let newVersion = version.webclient;

                if (this.initialVersion) {
                    if (JSON.stringify(this.initialVersion) !== JSON.stringify(newVersion)) {
                        this.showReloadBanner = true;
                    }
                } else {
                    this.initialVersion = newVersion;
                }
            });
    }

    public ngOnDestroy(): void {
        if (this.subscription) {
            this.subscription.unsubscribe();
            this.subscription = null;
        }
    }

    public reload($event: MouseEvent): void {
        $event.preventDefault();
        location.reload();
    }
}
