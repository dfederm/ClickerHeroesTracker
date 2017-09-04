import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import Decimal from "decimal.js";

import { DashboardComponent } from "./dashboard";
import { UploadService, IUploadSummaryListResponse, IUpload } from "../../services/uploadService/uploadService";
import { UserService, IProgressData } from "../../services/userService/userService";
import { ChartDataSets, ChartOptions, ChartPoint } from "chart.js";

describe("DashboardComponent", () => {
    let component: DashboardComponent;
    let fixture: ComponentFixture<DashboardComponent>;

    let uploadsResponse: IUploadSummaryListResponse = {
        uploads: [{
            id: 1234,
            timeSubmitted: undefined,
            playStyle: undefined,
        }],
        pagination: undefined,
    };

    let upload: IUpload = {
        user: {
            name: "someUserName",
            id: undefined,
        },
        id: undefined,
        timeSubmitted: undefined,
        playStyle: undefined,
    };

    let progress: IProgressData = {
        soulsSpentData: {
            "2017-01-01T00:00:00Z": "0",
            "2017-01-02T00:00:00Z": "1",
            "2017-01-03T00:00:00Z": "2",
            "2017-01-04T00:00:00Z": "3",
            "2017-01-05T00:00:00Z": "4",
        },
        titanDamageData: undefined,
        heroSoulsSacrificedData: undefined,
        totalAncientSoulsData: undefined,
        transcendentPowerData: undefined,
        rubiesData: undefined,
        highestZoneThisTranscensionData: undefined,
        highestZoneLifetimeData: undefined,
        ascensionsThisTranscensionData: undefined,
        ascensionsLifetimeData: undefined,
        ancientLevelData: undefined,
        outsiderLevelData: undefined,
    };

    beforeEach(done => {
        let uploadService = {
            getUploads(): Promise<IUploadSummaryListResponse> {
                return Promise.resolve(uploadsResponse);
            },
            get(): Promise<IUpload> {
                return Promise.resolve(upload);
            },
        };

        let userService = {
            getProgress(): Promise<IProgressData> {
                return Promise.resolve(progress);
            },
        };

        TestBed.configureTestingModule(
            {
                declarations: [DashboardComponent],
                providers: [
                    { provide: UploadService, useValue: uploadService },
                    { provide: UserService, useValue: userService },
                ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(DashboardComponent);
                component = fixture.componentInstance;
            })
            .then(done)
            .catch(done.fail);
    });

    describe("Upload Table", () => {
        it("should display without pagination", () => {
            fixture.detectChanges();

            let uploadsTable = fixture.debugElement.query(By.css("uploadsTable"));
            expect(uploadsTable).not.toBeNull();
            expect(uploadsTable.properties.count).toEqual(10);
            expect(uploadsTable.properties.paginate).toBeFalsy();
        });
    });

    describe("Progress Summary", () => {
        it("should display a chart with linear scale", done => {
            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let error = fixture.debugElement.query(By.css(".text-danger"));
                    expect(error).toBeNull();

                    let warning = fixture.debugElement.query(By.css(".text-warning"));
                    expect(warning).toBeNull();

                    let chart = fixture.debugElement.query(By.css("canvas"));
                    expect(chart).not.toBeNull();
                    expect(chart.attributes.baseChart).toBeDefined();
                    expect(chart.attributes.height).toEqual("235");
                    expect(chart.properties.chartType).toEqual("line");

                    let datasets: ChartDataSets[] = chart.properties.datasets;
                    expect(datasets).toBeTruthy();
                    expect(datasets.length).toEqual(1);

                    let data = datasets[0].data as ChartPoint[];
                    let dataKeys = Object.keys(progress.soulsSpentData);
                    expect(data.length).toEqual(dataKeys.length);
                    for (let i = 0; i < data.length; i++) {
                        expect(data[i].x).toEqual(new Date(dataKeys[i]).getTime());
                        expect(data[i].y).toEqual(Number(progress.soulsSpentData[dataKeys[i]]));
                    }

                    let colors = chart.properties.colors;
                    expect(colors).toBeTruthy();

                    let options: ChartOptions = chart.properties.options;
                    expect(options).toBeTruthy();
                    expect(options.title.text).toEqual("Souls Spent");
                    expect(options.scales.yAxes[0].type).toEqual("linear");
                })
                .then(done)
                .catch(done.fail);
        });

        it("should display a chart with logarithmic scale", done => {
            // Using values greater than normal numbers can handle
            let soulsSpentData: { [date: string]: string } = {
                "2017-01-01T00:00:00Z": "1e1000",
                "2017-01-02T00:00:00Z": "1e1001",
                "2017-01-03T00:00:00Z": "1e1002",
                "2017-01-04T00:00:00Z": "1e1003",
                "2017-01-05T00:00:00Z": "1e1004",
            };
            let userService = TestBed.get(UserService);
            spyOn(userService, "getProgress").and.returnValue(Promise.resolve({ soulsSpentData }));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let error = fixture.debugElement.query(By.css(".text-danger"));
                    expect(error).toBeNull();

                    let warning = fixture.debugElement.query(By.css(".text-warning"));
                    expect(warning).toBeNull();

                    let chart = fixture.debugElement.query(By.css("canvas"));
                    expect(chart).not.toBeNull();
                    expect(chart.attributes.baseChart).toBeDefined();
                    expect(chart.attributes.height).toEqual("235");
                    expect(chart.properties.chartType).toEqual("line");

                    let datasets: ChartDataSets[] = chart.properties.datasets;
                    expect(datasets).toBeTruthy();
                    expect(datasets.length).toEqual(1);

                    let data = datasets[0].data as ChartPoint[];
                    let dataKeys = Object.keys(soulsSpentData);
                    expect(data.length).toEqual(dataKeys.length);
                    for (let i = 0; i < data.length; i++) {
                        expect(data[i].x).toEqual(new Date(dataKeys[i]).getTime());

                        // The value we plot is actually the log of the value to fake log scale
                        expect(data[i].y).toEqual(new Decimal(soulsSpentData[dataKeys[i]]).log().toNumber());
                    }

                    let colors = chart.properties.colors;
                    expect(colors).toBeTruthy();

                    let options: ChartOptions = chart.properties.options;
                    expect(options).toBeTruthy();
                    expect(options.title.text).toEqual("Souls Spent");

                    // Linear since we're manually managing log scale
                    expect(options.scales.yAxes[0].type).toEqual("linear");
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when uploadService.getUploads fails", done => {
            let uploadService = TestBed.get(UploadService);
            spyOn(uploadService, "getUploads").and.returnValue(Promise.reject("someReason"));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let chart = fixture.debugElement.query(By.css("canvas"));
                    expect(chart).toBeNull();

                    let error = fixture.debugElement.query(By.css(".text-danger"));
                    expect(error).not.toBeNull();

                    let warning = fixture.debugElement.query(By.css(".text-warning"));
                    expect(warning).toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when uploadService.get fails", done => {
            let uploadService = TestBed.get(UploadService);
            spyOn(uploadService, "get").and.returnValue(Promise.reject("someReason"));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let chart = fixture.debugElement.query(By.css("canvas"));
                    expect(chart).toBeNull();

                    let error = fixture.debugElement.query(By.css(".text-danger"));
                    expect(error).not.toBeNull();

                    let warning = fixture.debugElement.query(By.css(".text-warning"));
                    expect(warning).toBeNull();
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

                    let chart = fixture.debugElement.query(By.css("canvas"));
                    expect(chart).toBeNull();

                    let error = fixture.debugElement.query(By.css(".text-danger"));
                    expect(error).not.toBeNull();

                    let warning = fixture.debugElement.query(By.css(".text-warning"));
                    expect(warning).toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show a warning when there is no data", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getProgress").and.returnValue(Promise.resolve({ soulsSpentData: {} }));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let chart = fixture.debugElement.query(By.css("canvas"));
                    expect(chart).toBeNull();

                    let error = fixture.debugElement.query(By.css(".text-danger"));
                    expect(error).toBeNull();

                    let warning = fixture.debugElement.query(By.css(".text-warning"));
                    expect(warning).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });
    });
});
