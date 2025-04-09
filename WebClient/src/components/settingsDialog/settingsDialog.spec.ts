import { ComponentFixture, TestBed } from "@angular/core/testing";
import { FormsModule } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { BehaviorSubject } from "rxjs";
import { By } from "@angular/platform-browser";

import { SettingsDialogComponent } from "./settingsDialog";
import { SettingsService, IUserSettings } from "../../services/settingsService/settingsService";
import { NgxSpinnerModule } from "ngx-spinner";
import { Component, Input } from "@angular/core";

describe("SettingsDialogComponent", () => {
    let component: SettingsDialogComponent;
    let fixture: ComponentFixture<SettingsDialogComponent>;

    @Component({ selector: "ngx-spinner", template: "", standalone: true })
    class MockNgxSpinnerComponent {
        @Input()
        public fullScreen: boolean;
    }

    // "Fully expanded" settings, ie, show all possible inputs
    const settings: IUserSettings = {
        playStyle: "hybrid",
        useScientificNotation: true,
        scientificNotationThreshold: 1000000,
        useLogarithmicGraphScale: true,
        logarithmicGraphScaleThreshold: 1000000,
        hybridRatio: 2,
        theme: "light",
        shouldLevelSkillAncients: true,
        skillAncientBaseAncient: 17,
        skillAncientLevelDiff: 0,
        graphSpacingType: "time",
    };

    beforeEach(async () => {
        let settingsSubject = new BehaviorSubject(settings);
        let settingsService = {
            settings: () => settingsSubject,
            setSetting: (): void => void 0,
        };
        let activeModal = { close: (): void => void 0 };

        await TestBed.configureTestingModule(
            {
                imports: [
                    SettingsDialogComponent,
                    FormsModule,
                ],
                providers: [
                    { provide: SettingsService, useValue: settingsService },
                    { provide: NgbActiveModal, useValue: activeModal },
                ],
            })
            .compileComponents();
        TestBed.overrideComponent(SettingsDialogComponent, {
            remove: { imports: [ NgxSpinnerModule ]},
            add: { imports: [ MockNgxSpinnerComponent ] },
        });

        fixture = TestBed.createComponent(SettingsDialogComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    describe("Play Style", () => {
        let input: HTMLSelectElement;

        beforeEach(() => {
            input = getInput(0, "select");
        });

        it("should be set to the inital value", () => {
            verifyInitialValue(input, `${input.selectedIndex}: ${settings.playStyle}`);
        });

        it("should patch settings when the value changes", () => {
            verifyServiceCalledWhenSettingChanges(
                "playStyle",
                () => {
                    setSelectValue(input, input.selectedIndex + 1);
                    return component.playStyles[input.selectedIndex];
                },
            );
        });

        it("should disable the setting until the patch is complete", () => {
            return verifyDisabledUntilPromiseResolves(input, () => setSelectValue(input, input.selectedIndex + 1));
        });

        it("should show an error when the patch fails", () => {
            return verifyErrorShowWhenPromiseRejects(input, () => setSelectValue(input, input.selectedIndex + 1));
        });
    });

    describe("Hybrid Ratio", () => {
        let input: HTMLInputElement;

        beforeEach(() => {
            input = getInput(1, "input");
        });

        it("should be set to the inital value", () => {
            verifyInitialValue(input, settings.hybridRatio.toString());
        });

        it("should hide when playStyle is not hybrid", () => {
            let settingsService = TestBed.inject(SettingsService);
            spyOn(settingsService, "setSetting").and.returnValue(Promise.resolve());

            let playStyleInput = getInput<HTMLSelectElement>(0, "select");
            setSelectValue(playStyleInput, playStyleInput.selectedIndex + 1);

            expect(isInDom(input)).toEqual(false);
        });

        it("should patch settings when the value changes", () => {
            verifyServiceCalledWhenSettingChanges(
                "hybridRatio",
                () => {
                    let newValue = settings.hybridRatio + 1;
                    setInputValue(input, newValue.toString());
                    return newValue;
                },
            );
        });

        it("should disable the setting until the patch is complete", () => {
            return verifyDisabledUntilPromiseResolves(input, () => setInputValue(input, (settings.hybridRatio + 1).toString()));
        });

        it("should show an error when the patch fails", () => {
            return verifyErrorShowWhenPromiseRejects(input, () => setInputValue(input, (settings.hybridRatio + 1).toString()));
        });
    });

    describe("Should Level Skill Ancients", () => {
        let input: HTMLInputElement;

        beforeEach(() => {
            input = getInput(2, "input");
        });

        it("should be set to the inital value", () => {
            verifyInitialValue(input, settings.shouldLevelSkillAncients);
        });

        it("should hide when playStyle is idle", () => {
            let settingsService = TestBed.inject(SettingsService);
            spyOn(settingsService, "setSetting").and.returnValue(Promise.resolve());

            let playStyleInput = getInput<HTMLSelectElement>(0, "select");
            setSelectValue(playStyleInput, 0);

            expect(isInDom(input)).toEqual(false);
        });

        it("should patch settings when the value changes", () => {
            verifyServiceCalledWhenSettingChanges(
                "shouldLevelSkillAncients",
                () => {
                    let newValue = !settings.shouldLevelSkillAncients;
                    setCheckboxValue(input, newValue);
                    return newValue;
                },
            );
        });

        it("should disable the setting until the patch is complete", () => {
            return verifyDisabledUntilPromiseResolves(input, () => setCheckboxValue(input, !settings.shouldLevelSkillAncients));
        });

        it("should show an error when the patch fails", () => {
            return verifyErrorShowWhenPromiseRejects(input, () => setCheckboxValue(input, !settings.shouldLevelSkillAncients));
        });
    });

    describe("Skill Ancient Base Ancient", () => {
        let input: HTMLSelectElement;

        beforeEach(() => {
            input = getInput(3, "select");
        });

        it("should be set to the inital value", () => {
            verifyInitialValue(input, `${input.selectedIndex}: ${settings.skillAncientBaseAncient}`);
        });

        it("should hide when playStyle is idle", () => {
            let settingsService = TestBed.inject(SettingsService);
            spyOn(settingsService, "setSetting").and.returnValue(Promise.resolve());

            let playStyleInput = getInput<HTMLSelectElement>(0, "select");
            setSelectValue(playStyleInput, 0);

            expect(isInDom(input)).toEqual(false);
        });

        it("should hide when shouldLevelSkillAncients is off", () => {
            let settingsService = TestBed.inject(SettingsService);
            spyOn(settingsService, "setSetting").and.returnValue(Promise.resolve());

            let shouldLevelSkillAncientsInput = getInput<HTMLInputElement>(2, "input");
            setCheckboxValue(shouldLevelSkillAncientsInput, false);

            expect(isInDom(input)).toEqual(false);
        });

        it("should patch settings when the value changes", () => {
            verifyServiceCalledWhenSettingChanges(
                "skillAncientBaseAncient",
                () => {
                    setSelectValue(input, input.selectedIndex + 1);
                    return component.skillAncientBaseAncients[input.selectedIndex].id;
                },
            );
        });

        it("should disable the setting until the patch is complete", () => {
            return verifyDisabledUntilPromiseResolves(input, () => setSelectValue(input, input.selectedIndex + 1));
        });

        it("should show an error when the patch fails", () => {
            return verifyErrorShowWhenPromiseRejects(input, () => setSelectValue(input, input.selectedIndex + 1));
        });
    });

    describe("Skill Ancient Level Diff", () => {
        let input: HTMLInputElement;

        beforeEach(() => {
            input = getInput(4, "input");
        });

        it("should be set to the inital value", () => {
            verifyInitialValue(input, settings.skillAncientLevelDiff.toString());
        });

        it("should hide when playStyle is idle", () => {
            let settingsService = TestBed.inject(SettingsService);
            spyOn(settingsService, "setSetting").and.returnValue(Promise.resolve());

            let playStyleInput = getInput<HTMLSelectElement>(0, "select");
            setSelectValue(playStyleInput, 0);

            expect(isInDom(input)).toEqual(false);
        });

        it("should hide when shouldLevelSkillAncients is off", () => {
            let settingsService = TestBed.inject(SettingsService);
            spyOn(settingsService, "setSetting").and.returnValue(Promise.resolve());

            let shouldLevelSkillAncientsInput = getInput<HTMLInputElement>(2, "input");
            setCheckboxValue(shouldLevelSkillAncientsInput, false);

            expect(isInDom(input)).toEqual(false);
        });

        it("should patch settings when the value changes", () => {
            verifyServiceCalledWhenSettingChanges(
                "skillAncientLevelDiff",
                () => {
                    let newValue = settings.skillAncientLevelDiff + 1;
                    setInputValue(input, newValue.toString());
                    return newValue;
                },
            );
        });

        it("should disable the setting until the patch is complete", () => {
            return verifyDisabledUntilPromiseResolves(input, () => setInputValue(input, (settings.skillAncientLevelDiff + 1).toString()));
        });

        it("should show an error when the patch fails", () => {
            return verifyErrorShowWhenPromiseRejects(input, () => setInputValue(input, (settings.skillAncientLevelDiff + 1).toString()));
        });
    });

    describe("Use scientific notation", () => {
        let input: HTMLInputElement;

        beforeEach(() => {
            input = getInput(5, "input");
        });

        it("should be set to the inital value", () => {
            verifyInitialValue(input, settings.useScientificNotation);
        });

        it("should patch settings when the value changes", () => {
            verifyServiceCalledWhenSettingChanges(
                "useScientificNotation",
                () => {
                    let newValue = !settings.useScientificNotation;
                    setCheckboxValue(input, newValue);
                    return newValue;
                },
            );
        });

        it("should disable the setting until the patch is complete", () => {
            return verifyDisabledUntilPromiseResolves(input, () => setCheckboxValue(input, !settings.useScientificNotation));
        });

        it("should show an error when the patch fails", () => {
            return verifyErrorShowWhenPromiseRejects(input, () => setCheckboxValue(input, !settings.useScientificNotation));
        });
    });

    describe("Scientific notation threshold", () => {
        let input: HTMLInputElement;

        beforeEach(() => {
            input = getInput(6, "input");
        });

        it("should be set to the inital value", () => {
            verifyInitialValue(input, settings.scientificNotationThreshold.toString());
        });

        it("should hide when useScientificNotation is off", () => {
            let settingsService = TestBed.inject(SettingsService);
            spyOn(settingsService, "setSetting").and.returnValue(Promise.resolve());

            let useScientificNotationInput = getInput<HTMLInputElement>(5, "input");
            setCheckboxValue(useScientificNotationInput, false);

            expect(isInDom(input)).toEqual(false);
        });

        it("should patch settings when the value changes", () => {
            verifyServiceCalledWhenSettingChanges(
                "scientificNotationThreshold",
                () => {
                    let newValue = settings.scientificNotationThreshold + 1;
                    setInputValue(input, newValue.toString());
                    return newValue;
                },
            );
        });

        it("should disable the setting until the patch is complete", () => {
            return verifyDisabledUntilPromiseResolves(input, () => setInputValue(input, (settings.scientificNotationThreshold + 1).toString()));
        });

        it("should show an error when the patch fails", () => {
            return verifyErrorShowWhenPromiseRejects(input, () => setInputValue(input, (settings.scientificNotationThreshold + 1).toString()));
        });
    });

    describe("Use logarithmic scale for graphs", () => {
        let input: HTMLInputElement;

        beforeEach(() => {
            input = getInput(7, "input");
        });

        it("should be set to the inital value", () => {
            verifyInitialValue(input, settings.useLogarithmicGraphScale);
        });

        it("should patch settings when the value changes", () => {
            verifyServiceCalledWhenSettingChanges(
                "useLogarithmicGraphScale",
                () => {
                    let newValue = !settings.useLogarithmicGraphScale;
                    setCheckboxValue(input, newValue);
                    return newValue;
                },
            );
        });

        it("should disable the setting until the patch is complete", () => {
            return verifyDisabledUntilPromiseResolves(input, () => setCheckboxValue(input, !settings.useLogarithmicGraphScale));
        });

        it("should show an error when the patch fails", () => {
            return verifyErrorShowWhenPromiseRejects(input, () => setCheckboxValue(input, !settings.useLogarithmicGraphScale));
        });
    });

    describe("Logarithmic scale threshold", () => {
        let input: HTMLInputElement;

        beforeEach(() => {
            input = getInput(8, "input");
        });

        it("should be set to the inital value", () => {
            verifyInitialValue(input, settings.logarithmicGraphScaleThreshold.toString());
        });

        it("should hide when useLogarithmicGraphScale is off", () => {
            let settingsService = TestBed.inject(SettingsService);
            spyOn(settingsService, "setSetting").and.returnValue(Promise.resolve());

            let useLogarithmicGraphScaleInput = getInput<HTMLInputElement>(7, "input");
            setCheckboxValue(useLogarithmicGraphScaleInput, false);

            expect(isInDom(input)).toEqual(false);
        });

        it("should patch settings when the value changes", () => {
            verifyServiceCalledWhenSettingChanges(
                "logarithmicGraphScaleThreshold",
                () => {
                    let newValue = settings.logarithmicGraphScaleThreshold + 1;
                    setInputValue(input, newValue.toString());
                    return newValue;
                },
            );
        });

        it("should disable the setting until the patch is complete", () => {
            return verifyDisabledUntilPromiseResolves(input, () => setInputValue(input, (settings.logarithmicGraphScaleThreshold + 1).toString()));
        });

        it("should show an error when the patch fails", () => {
            return verifyErrorShowWhenPromiseRejects(input, () => setInputValue(input, (settings.logarithmicGraphScaleThreshold + 1).toString()));
        });
    });

    describe("Graph Spacing Type", () => {
        let input: HTMLSelectElement;

        beforeEach(() => {
            input = getInput(9, "select");
        });

        it("should be set to the inital value", () => {
            verifyInitialValue(input, `${input.selectedIndex}: ${settings.graphSpacingType}`);
        });

        it("should patch settings when the value changes", () => {
            verifyServiceCalledWhenSettingChanges(
                "graphSpacingType",
                () => {
                    setSelectValue(input, input.selectedIndex + 1);
                    return component.graphSpacingTypes[input.selectedIndex];
                },
            );
        });

        it("should disable the setting until the patch is complete", () => {
            return verifyDisabledUntilPromiseResolves(input, () => setSelectValue(input, input.selectedIndex + 1));
        });

        it("should show an error when the patch fails", () => {
            return verifyErrorShowWhenPromiseRejects(input, () => setSelectValue(input, input.selectedIndex + 1));
        });
    });

    describe("Site theme", () => {
        let input: HTMLSelectElement;

        beforeEach(() => {
            input = getInput(10, "select");
        });

        it("should be set to the inital value", () => {
            verifyInitialValue(input, `${input.selectedIndex}: ${settings.theme}`);
        });

        it("should patch settings when the value changes", () => {
            verifyServiceCalledWhenSettingChanges(
                "theme",
                () => {
                    setSelectValue(input, input.selectedIndex + 1);
                    return component.themes[input.selectedIndex];
                },
            );
        });

        it("should disable the setting until the patch is complete", () => {
            return verifyDisabledUntilPromiseResolves(input, () => setSelectValue(input, input.selectedIndex + 1));
        });

        it("should show an error when the patch fails", () => {
            return verifyErrorShowWhenPromiseRejects(input, () => setSelectValue(input, input.selectedIndex + 1));
        });
    });

    function getInput<T extends HTMLElement>(i: number, selector: string): T {
        let body = fixture.debugElement.query(By.css(".modal-body"));
        expect(body).not.toBeNull();

        let formGroups = body.queryAll(By.css(".mb-3"));
        expect(formGroups.length).toEqual(11);

        let formGroup = formGroups[i];
        let inputElement = formGroup.query(By.css(selector));
        expect(inputElement).not.toBeNull();
        return inputElement.nativeElement as T;
    }

    function verifyInitialValue(
        input: HTMLSelectElement | HTMLInputElement,
        expectedValue: string | boolean,
    ): void {
        if (typeof expectedValue === "boolean") {
            // Ideally this would check if it's checked, but bSwitch doesn't seem to use that.
            fixture.detectChanges();
            expect(input.getAttribute("ng-reflect-model")).toEqual(expectedValue.toString());
        } else {
            expect(input.value).toEqual(expectedValue);
        }

        expect(input.disabled).toEqual(false);

        let error = fixture.debugElement.query(By.css(".alert-danger"));
        expect(error).toBeNull();
    }

    function isInDom(element: HTMLElement): boolean {
        // Check if the input is still in the DOM by just walking up it.
        while (element !== null) {
            element = element.parentElement;
            if (element === document.body) {
                return true;
            }
        }

        return false;
    }

    function verifyServiceCalledWhenSettingChanges(
        setting: keyof IUserSettings,
        setValue: () => {},
    ): void {
        let settingsService = TestBed.inject(SettingsService);
        spyOn(settingsService, "setSetting").and.returnValue(Promise.resolve());

        let newValue = setValue();
        expect(settingsService.setSetting).toHaveBeenCalledWith(setting, newValue);

        let error = fixture.debugElement.query(By.css(".alert-danger"));
        expect(error).toBeNull();
    }

    async function verifyDisabledUntilPromiseResolves(
        input: HTMLSelectElement | HTMLInputElement,
        setValue: () => void,
    ): Promise<void> {
        let resolvePromise: () => void;
        let settingsService = TestBed.inject(SettingsService);
        // eslint-disable-next-line , , , , , , , , , , ,
        spyOn(settingsService, "setSetting").and.returnValue(new Promise(resolve => {
            resolvePromise = resolve;
        }));

        setValue();

        // Not sure why this is needed to update the disable property
        await fixture.whenStable();

        expect(input.disabled).toEqual(true);

        // Resolve the settingsService promise
        resolvePromise();
        fixture.detectChanges();
        await fixture.whenStable();

        // Not sure why this is needed to update the disable property
        fixture.detectChanges();
        await fixture.whenStable();
        fixture.detectChanges();

        let error = fixture.debugElement.query(By.css(".alert-danger"));
        expect(error).toBeNull();
        expect(input.disabled).toEqual(false);
    }

    async function verifyErrorShowWhenPromiseRejects(
        input: HTMLSelectElement | HTMLInputElement,
        setValue: () => void,
    ): Promise<void> {
        let rejectPromise: () => void;
        let settingsService = TestBed.inject(SettingsService);
        // eslint-disable-next-line
        spyOn(settingsService, "setSetting").and.returnValue(new Promise((_, reject) => {
            rejectPromise = reject;
        }));

        setValue();

        // Not sure why this is needed to update the disable property
        await fixture.whenStable();

        expect(input.disabled).toEqual(true);

        // Resolve the settingsService promise
        rejectPromise();
        fixture.detectChanges();
        await fixture.whenStable();

        // Not sure why this is needed to update the disable property
        fixture.detectChanges();
        await fixture.whenStable();
        fixture.detectChanges();

        let error = fixture.debugElement.query(By.css(".alert-danger"));
        expect(error).not.toBeNull();
        expect(input.disabled).toEqual(false);
    }

    function setSelectValue(select: HTMLSelectElement, selectedIndex: number): void {
        select.selectedIndex = selectedIndex;

        // Tell Angular
        let evt = document.createEvent("CustomEvent");
        evt.initCustomEvent("change", false, false, null);
        select.dispatchEvent(evt);

        fixture.detectChanges();
    }

    function setInputValue(input: HTMLInputElement, value: string): void {
        input.value = value;

        // Tell Angular
        let inputEvt = document.createEvent("CustomEvent");
        inputEvt.initCustomEvent("input", false, false, null);
        input.dispatchEvent(inputEvt);

        // We only make the call on blur
        let blurEvt = document.createEvent("CustomEvent");
        blurEvt.initCustomEvent("blur", false, false, null);
        input.dispatchEvent(blurEvt);

        fixture.detectChanges();
    }

    function setCheckboxValue(input: HTMLInputElement, checked: boolean): void {
        input.checked = checked;

        // Tell Angular
        let evt = document.createEvent("CustomEvent");
        evt.initCustomEvent("change", false, false, null);
        input.dispatchEvent(evt);

        fixture.detectChanges();
    }
});
