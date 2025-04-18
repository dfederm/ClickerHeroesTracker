import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { BehaviorSubject } from "rxjs";

import { BannerComponent } from "./banner";
import { VersionService, IVersion } from "../../services/versionService/versionService";

describe("BannerComponent", () => {
    let fixture: ComponentFixture<BannerComponent>;
    let version: BehaviorSubject<IVersion>;

    beforeEach(async () => {
        version = new BehaviorSubject(createVersion(0));
        let versionService = {
            getVersion: () => version,
        };

        await TestBed.configureTestingModule(
            {
                imports: [BannerComponent],
                providers: [
                    { provide: VersionService, useValue: versionService },
                ],
            })
            .compileComponents();

        fixture = TestBed.createComponent(BannerComponent);

        fixture.detectChanges();
    });

    it("should initially show nothing", () => {
        expect(fixture.debugElement.children.length).toEqual(0);
    });

    describe("Reload Banner", () => {
        it("should show when the webclient version changes", () => {
            let reloadBanner = fixture.debugElement.query(By.css("ngb-alert"));
            expect(reloadBanner).toBeNull();

            version.next(createVersion(1));
            fixture.detectChanges();

            reloadBanner = fixture.debugElement.query(By.css("ngb-alert"));
            expect(reloadBanner).not.toBeNull();

            let reloadLink = reloadBanner.query(By.css("a"));
            expect(reloadLink).not.toBeNull();
        });

        it("should not show when the webclient version doesn't change", () => {
            version.next(createVersion(0));
            fixture.detectChanges();

            let reloadBanner = fixture.debugElement.query(By.css("ngb-alert"));
            expect(reloadBanner).toBeNull();
        });

        it("should not show when the server version change but the webclient version doesn't", () => {
            let initialVersion = version.getValue();
            let newVersion = createVersion(1);
            newVersion.webclient = initialVersion.webclient;

            version.next(newVersion);
            fixture.detectChanges();

            let reloadBanner = fixture.debugElement.query(By.css("ngb-alert"));
            expect(reloadBanner).toBeNull();
        });
    });

    function createVersion(index: number): IVersion {
        return {
            environment: "environment_" + index,
            changelist: "changelist_" + index,
            buildUrl: "buildUrl_" + index,
            webclient: {
                bundle1: "bundle1_" + index,
                bundle2: "bundle2_" + index,
                bundle3: "bundle3_" + index,
            },
        };
    }
});
