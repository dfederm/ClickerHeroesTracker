import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { ActivatedRoute, Params } from "@angular/router";
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import Decimal from "decimal.js";

import { UserCompareComponent } from "./userCompare";
import { UserService, IProgressData } from "../../services/userService/userService";
import { ChartDataSets, ChartPoint, ChartOptions } from "chart.js";
import { SettingsService } from "../../services/settingsService/settingsService";
import { ExponentialPipe } from "../../pipes/exponentialPipe";

describe("UserCompareComponent", () => {
    let component: UserCompareComponent;
    let fixture: ComponentFixture<UserCompareComponent>;
    let routeParams: BehaviorSubject<Params>;

    let userProgress: IProgressData = {
        soulsSpentData: createData(100),
        titanDamageData: createData(101),
        heroSoulsSacrificedData: createData(102),
        totalAncientSoulsData: createData(103),
        transcendentPowerData: createData(104),
        rubiesData: createData(105),
        highestZoneThisTranscensionData: createData(106),
        highestZoneLifetimeData: createData(107),
        ascensionsThisTranscensionData: createData(108),
        ascensionsLifetimeData: createData(109),
        outsiderLevelData: {
            outsider0: createData(110),
            outsider1: createData(111),
            outsider2: createData(112),
        },
        ancientLevelData: {
            ancient0: createData(113),
            ancient1: createData(114),
            ancient2: createData(115),
        },
    };

    let compareProgress: IProgressData = {
        soulsSpentData: createData(200),
        titanDamageData: createData(201),
        heroSoulsSacrificedData: createData(202),
        totalAncientSoulsData: createData(203),
        transcendentPowerData: createData(204),
        rubiesData: createData(205),
        highestZoneThisTranscensionData: createData(206),
        highestZoneLifetimeData: createData(207),
        ascensionsThisTranscensionData: createData(208),
        ascensionsLifetimeData: createData(209),
        outsiderLevelData: {
            outsider0: createData(210),
            outsider1: createData(211),
            outsider2: createData(212),
        },
        ancientLevelData: {
            ancient0: createData(213),
            ancient1: createData(214),
            ancient2: createData(215),
        },
    };

    let expectedChartOrder = [
        { title: "Souls Spent", isLogarithmic: true, data1: userProgress.soulsSpentData, data2: compareProgress.soulsSpentData },
        { title: "Titan Damage", isLogarithmic: false, data1: userProgress.titanDamageData, data2: compareProgress.titanDamageData },
        { title: "Hero Souls Sacrificed", isLogarithmic: true, data1: userProgress.heroSoulsSacrificedData, data2: compareProgress.heroSoulsSacrificedData },
        { title: "Total Ancient Souls", isLogarithmic: false, data1: userProgress.totalAncientSoulsData, data2: compareProgress.totalAncientSoulsData },
        { title: "Transcendent Power", isLogarithmic: true, data1: userProgress.transcendentPowerData, data2: compareProgress.transcendentPowerData },
        { title: "Rubies", isLogarithmic: false, data1: userProgress.rubiesData, data2: compareProgress.rubiesData },
        { title: "Highest Zone This Transcension", isLogarithmic: true, data1: userProgress.highestZoneThisTranscensionData, data2: compareProgress.highestZoneThisTranscensionData },
        { title: "Highest Zone Lifetime", isLogarithmic: false, data1: userProgress.highestZoneLifetimeData, data2: compareProgress.highestZoneLifetimeData },
        { title: "Ascensions This Transcension", isLogarithmic: true, data1: userProgress.ascensionsThisTranscensionData, data2: compareProgress.ascensionsThisTranscensionData },
        { title: "Ascensions Lifetime", isLogarithmic: false, data1: userProgress.ascensionsLifetimeData, data2: compareProgress.ascensionsLifetimeData },
        { title: "Outsider0", isLogarithmic: true, data1: userProgress.outsiderLevelData.outsider0, data2: compareProgress.outsiderLevelData.outsider0 },
        { title: "Outsider1", isLogarithmic: false, data1: userProgress.outsiderLevelData.outsider1, data2: compareProgress.outsiderLevelData.outsider1 },
        { title: "Outsider2", isLogarithmic: true, data1: userProgress.outsiderLevelData.outsider2, data2: compareProgress.outsiderLevelData.outsider2 },
        { title: "Ancient0", isLogarithmic: false, data1: userProgress.ancientLevelData.ancient0, data2: compareProgress.ancientLevelData.ancient0 },
        { title: "Ancient1", isLogarithmic: true, data1: userProgress.ancientLevelData.ancient1, data2: compareProgress.ancientLevelData.ancient1 },
        { title: "Ancient2", isLogarithmic: false, data1: userProgress.ancientLevelData.ancient2, data2: compareProgress.ancientLevelData.ancient2 },
    ];

    const settings = SettingsService.defaultSettings;

    let settingsSubject = new BehaviorSubject(settings);

    beforeEach(async(() => {
        let userService = {
            getProgress(userName: string): Promise<IProgressData> {
                if (userName === "someUserName") {
                    return Promise.resolve(userProgress);
                } else if (userName === "someOtherUserName") {
                    return Promise.resolve(compareProgress);
                } else {
                    return Promise.reject("No user: " + userName);
                }
            },
        };

        let settingsService = { settings: () => settingsSubject };

        routeParams = new BehaviorSubject({ userName: "someUserName", compareUserName: "someOtherUserName" });
        let route = { params: routeParams };
        TestBed.configureTestingModule(
            {
                declarations: [UserCompareComponent],
                providers: [
                    { provide: ActivatedRoute, useValue: route },
                    { provide: UserService, useValue: userService },
                    { provide: SettingsService, useValue: settingsService },
                ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(UserCompareComponent);
                component = fixture.componentInstance;
            });
    }));

    describe("Range Selector", () => {
        it("should display", () => {
            fixture.detectChanges();

            expect(component.currentDateRange).toEqual("1w");

            let dateRangeSelector = fixture.debugElement.query(By.css(".btn-group"));
            expect(dateRangeSelector).not.toBeNull();

            let dateRanges = dateRangeSelector.queryAll(By.css("button"));
            expect(dateRanges.length).toEqual(component.dateRanges.length);
            for (let i = 0; i < dateRanges.length; i++) {
                expect(dateRanges[i].nativeElement.textContent.trim()).toEqual(component.dateRanges[i]);
            }

            let disabledDateRanges = dateRanges.filter(dateRange => dateRange.classes.disabled);
            expect(disabledDateRanges.length).toEqual(1);
            expect(disabledDateRanges[0].nativeElement.textContent.trim()).toEqual(component.currentDateRange);
        });

        it("should change the current date range when clicked", () => {
            fixture.detectChanges();

            expect(component.currentDateRange).toEqual("1w");

            let dateRangeSelector = fixture.debugElement.query(By.css(".btn-group"));
            expect(dateRangeSelector).not.toBeNull();

            let dateRanges = dateRangeSelector.queryAll(By.css("button"));
            expect(dateRanges.length).toEqual(component.dateRanges.length);

            dateRanges[dateRanges.length - 1].nativeElement.click();

            fixture.detectChanges();

            expect(component.currentDateRange).toEqual(component.dateRanges[dateRanges.length - 1]);

            let disabledDateRanges = dateRanges.filter(dateRange => dateRange.classes.disabled);
            expect(disabledDateRanges.length).toEqual(1);
            expect(disabledDateRanges[0].nativeElement.textContent.trim()).toEqual(component.currentDateRange);
        });

        it("should request the proper date range", () => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getProgress").and.callThrough();

            fixture.detectChanges();

            let now = Date.now();
            spyOn(Date, "now").and.returnValue(now);

            let expectedEnd = new Date(now);
            let expectedStarts: { [dateRange: string]: Date } = {
                "1d": new Date(new Date(now).setDate(expectedEnd.getDate() - 1)),
                "3d": new Date(new Date(now).setDate(expectedEnd.getDate() - 3)),
                "1w": new Date(new Date(now).setDate(expectedEnd.getDate() - 7)),
                "1m": new Date(new Date(now).setMonth(expectedEnd.getMonth() - 1)),
                "3m": new Date(new Date(now).setMonth(expectedEnd.getMonth() - 3)),
                "1y": new Date(new Date(now).setFullYear(expectedEnd.getFullYear() - 1)),
            };

            expect(component.dateRanges.length).toEqual(Object.keys(expectedStarts).length);

            for (let i = 0; i < component.dateRanges.length; i++) {
                let dateRange = component.dateRanges[i];
                (userService.getProgress as jasmine.Spy).calls.reset();

                component.currentDateRange = dateRange;

                expect(userService.getProgress).toHaveBeenCalledWith("someUserName", expectedStarts[dateRange], expectedEnd);
            }
        });
    });

    describe("Charts", () => {
        it("should display", done => {
            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).toBeNull();

                    let warning = fixture.debugElement.query(By.css(".alert-warning"));
                    expect(warning).toBeNull();

                    let charts = fixture.debugElement.queryAll(By.css("canvas"));
                    expect(charts.length).toEqual(16);

                    for (let i = 0; i < charts.length; i++) {
                        let chart = charts[i];

                        expect(chart).not.toBeNull();
                        expect(chart.attributes.baseChart).toBeDefined();
                        expect(chart.attributes.height).toEqual("235");
                        expect(chart.properties.chartType).toEqual("line");

                        let colors = chart.properties.colors;
                        expect(colors).toBeTruthy();

                        let options: ChartOptions = chart.properties.options;
                        expect(options).toBeTruthy();
                        expect(options.title.text).toEqual(expectedChartOrder[i].title);

                        let isLogarithmic = expectedChartOrder[i].isLogarithmic;
                        let datasets: ChartDataSets[] = chart.properties.datasets;
                        expect(datasets).toBeTruthy();
                        expect(datasets.length).toEqual(2);

                        let data1 = datasets[0].data as ChartPoint[];
                        let expectedData1 = expectedChartOrder[i].data1;
                        let dataKeys1 = Object.keys(expectedData1);
                        expect(data1.length).toEqual(dataKeys1.length);
                        for (let j = 0; j < data1.length; j++) {
                            let expectedDate = new Date(dataKeys1[j]);
                            expect(data1[j].x).toEqual(expectedDate.getTime());
                            expect((options.tooltips.callbacks.title as Function)([{ xLabel: dataKeys1[j] }])).toEqual(expectedDate.toLocaleString());

                            // When logarithmic, the value we plot is actually the log of the value to fake log scale
                            let rawExpectedValue = expectedData1[dataKeys1[j]];
                            let expectedValue = isLogarithmic
                                ? new Decimal(rawExpectedValue).log().toNumber()
                                : Number(rawExpectedValue);
                            expect(data1[j].y).toEqual(expectedValue);

                            let expectedLabelNum = isLogarithmic
                                ? Decimal.pow(10, expectedValue)
                                : Number(expectedValue);
                            let expectedLabel = ExponentialPipe.formatNumber(expectedLabelNum, settings);
                            expect((options.tooltips.callbacks.label as Function)({ yLabel: expectedValue, datasetIndex: 0 })).toEqual("someUserName: " + expectedLabel);
                            expect(options.scales.yAxes[0].ticks.callback(expectedValue, null, null)).toEqual(expectedLabel);
                        }

                        let data2 = datasets[1].data as ChartPoint[];
                        let expectedData2 = expectedChartOrder[i].data2;
                        let dataKeys2 = Object.keys(expectedData2);
                        expect(data2.length).toEqual(dataKeys2.length);
                        for (let j = 0; j < data2.length; j++) {
                            let expectedDate = new Date(dataKeys2[j]);
                            expect(data2[j].x).toEqual(expectedDate.getTime());
                            expect((options.tooltips.callbacks.title as Function)([{ xLabel: dataKeys2[j] }])).toEqual(expectedDate.toLocaleString());

                            // When logarithmic, the value we plot is actually the log of the value to fake log scale
                            let rawExpectedValue = expectedData2[dataKeys2[j]];
                            let expectedValue = isLogarithmic
                                ? new Decimal(rawExpectedValue).log().toNumber()
                                : Number(rawExpectedValue);
                            expect(data2[j].y).toEqual(expectedValue);

                            let expectedLabelNum = isLogarithmic
                                ? Decimal.pow(10, expectedValue)
                                : Number(expectedValue);
                            let expectedLabel = ExponentialPipe.formatNumber(expectedLabelNum, settings);
                            expect((options.tooltips.callbacks.label as Function)({ yLabel: expectedValue, datasetIndex: 1 })).toEqual("someOtherUserName: " + expectedLabel);
                            expect(options.scales.yAxes[0].ticks.callback(expectedValue, null, null)).toEqual(expectedLabel);
                        }

                        // Linear even when logarithmic since we're manually managing log scale
                        expect(options.scales.yAxes[0].type).toEqual("linear");
                    }
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when userService.getProgress fails", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getProgress").and.returnValue(Promise.reject("someReason"));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).not.toBeNull();

                    let warning = fixture.debugElement.query(By.css(".alert-warning"));
                    expect(warning).toBeNull();

                    let charts = fixture.debugElement.queryAll(By.css("canvas"));
                    expect(charts.length).toEqual(0);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show a warning when there is no data", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getProgress").and.returnValue(Promise.resolve({}));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).toBeNull();

                    let warning = fixture.debugElement.query(By.css(".alert-warning"));
                    expect(warning).not.toBeNull();

                    let charts = fixture.debugElement.queryAll(By.css("canvas"));
                    expect(charts.length).toEqual(0);
                })
                .then(done)
                .catch(done.fail);
        });
    });

    function createData(index: number): { [date: string]: string } {
        // Mix it up to let some charts be logarithmic
        let prefix = index % 2
            ? index.toString()
            : "1e10" + index;
        return {
            "2017-01-01T00:00:00Z": prefix + "1",
            "2017-01-02T00:00:00Z": prefix + "2",
            "2017-01-03T00:00:00Z": prefix + "3",
            "2017-01-04T00:00:00Z": prefix + "4",
            "2017-01-05T00:00:00Z": prefix + "5",
        };
    }
});
