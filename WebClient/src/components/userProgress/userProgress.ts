import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Params } from "@angular/router";
import { UserService, IProgressData } from "../../services/userService/userService";

import Decimal from "decimal.js";
import { ChartDataSets, ChartOptions, ChartTooltipItem } from "chart.js";

interface IChartViewModel {
    isProminent: boolean;
    datasets: ChartDataSets[];
    options: ChartOptions;
    colors: {
        backgroundColor: string,
        borderColor: string,
        pointBackgroundColor: string,
        pointBorderColor: string,
        pointHoverBackgroundColor: string,
        pointHoverBorderColor: string,
    }[];
}

@Component({
    selector: "userProgress",
    templateUrl: "./userProgress.html",
})
export class UserProgressComponent implements OnInit {
    public isError: boolean;
    public userName: string;
    public _currentDateRange: string;
    public dateRanges: string[];
    public charts: IChartViewModel[];

    // TODO get the user's real settings
    private userSettings =
    {
        areUploadsPublic: true,
        hybridRatio: 1,
        logarithmicGraphScaleThreshold: 1000000,
        playStyle: "hybrid",
        scientificNotationThreshold: 100000,
        useEffectiveLevelForSuggestions: false,
        useLogarithmicGraphScale: true,
        useScientificNotation: true,
    };

    constructor(
        private route: ActivatedRoute,
        private userService: UserService,
    ) { }

    public get currentDateRange(): string {
        return this._currentDateRange;
    }
    public set currentDateRange(value: string) {
        if (this._currentDateRange !== value) {
            this._currentDateRange = value;
            this.fetchData();
        }
    }

    public ngOnInit(): void {
        this.dateRanges = [
            "1d",
            "3d",
            "1w",
            "1m",
            "3m",
            "1y",
        ];
        this._currentDateRange = "1w";

        this.route.params.subscribe(
            (params: Params) => {
                this.userName = params.userName;
                this.fetchData();
            },
            () => this.isError = true);
    }

    private fetchData(): void {
        if (!this.userName) {
            return;
        }

        let now = Date.now();
        let end = new Date(now);
        let start = new Date(now);

        switch (this.currentDateRange) {
            case "1d":
                start.setDate(start.getDate() - 1);
                break;
            case "3d":
                start.setDate(start.getDate() - 3);
                break;
            case "1m":
                start.setMonth(start.getMonth() - 1);
                break;
            case "3m":
                start.setMonth(start.getMonth() - 3);
                break;
            case "1y":
                start.setFullYear(start.getFullYear() - 1);
                break;
            case "1w":
            default:
                start.setDate(start.getDate() - 7);
                break;
        }

        this.userService.getProgress(this.userName, start, end)
            .then(progress => this.handleData(progress))
            .catch(() => this.isError = true);
    }

    private handleData(progress: IProgressData): void {
        if (!progress) {
            this.charts = [];
            return;
        }

        let charts: IChartViewModel[] = [];

        charts.push(this.createChart("Souls Spent", true, progress.soulsSpentData));
        charts.push(this.createChart("Titan Damage", true, progress.titanDamageData));
        charts.push(this.createChart("Hero Souls Sacrificed", true, progress.heroSoulsSacrificedData));
        charts.push(this.createChart("Total Ancient Souls", true, progress.totalAncientSoulsData));
        charts.push(this.createChart("Transcendent Power", true, progress.transcendentPowerData));
        charts.push(this.createChart("Rubies", true, progress.rubiesData));
        charts.push(this.createChart("Highest Zone This Transcension", true, progress.highestZoneThisTranscensionData));
        charts.push(this.createChart("Highest Zone Lifetime", true, progress.highestZoneLifetimeData));
        charts.push(this.createChart("Ascensions This Transcension", true, progress.ascensionsThisTranscensionData));
        charts.push(this.createChart("Ascensions Lifetime", true, progress.ascensionsLifetimeData));

        for (let outsider in progress.outsiderLevelData) {
            charts.push(this.createChart(this.toTitleCase(outsider), false, progress.outsiderLevelData[outsider]));
        }

        for (let ancient in progress.ancientLevelData) {
            charts.push(this.createChart(this.toTitleCase(ancient), false, progress.ancientLevelData[ancient]));
        }

        // Only valid charts
        charts = charts.filter(chart => chart != null);

        this.charts = charts;
    }

    private createChart(
        title: string,
        isProminent: boolean,
        data: { [time: string]: string },
    ): IChartViewModel {
        if (!data) {
            return null;
        }

        let min = new Decimal(Infinity);
        let max = new Decimal(0);
        let requiresDecimal = false;

        let decimalData: { x: number, y: decimal.Decimal }[] = [];
        for (let i in data) {
            let time = Date.parse(i);
            let value = new Decimal(data[i]);

            if (min.greaterThan(value)) {
                min = value;
            }

            if (max.lessThan(value)) {
                max = value;
            }

            requiresDecimal = requiresDecimal || !isFinite(value.toNumber());

            decimalData.push({ x: time, y: value });
        }

        let isLogarithmic = (this.userSettings.useLogarithmicGraphScale && max.minus(min).greaterThan(this.userSettings.logarithmicGraphScaleThreshold)) || requiresDecimal;

        let seriesData: { x: number, y: number }[] = [];
        for (let i = 0; i < decimalData.length; i++) {
            let time = decimalData[i].x;
            let value = decimalData[i].y;

            seriesData.push({
                x: time,
                y: isLogarithmic
                    ? value.log().toNumber()
                    : value.toNumber(),
            });
        }

        return {
            isProminent,
            datasets: [{
                data: seriesData,
            }],
            colors: [{
                backgroundColor: "rgba(151,187,205,0.4)",
                borderColor: "rgba(151,187,205,1)",
                pointBackgroundColor: "rgba(151,187,205,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "rgba(151,187,205,0.8)",
                pointHoverBorderColor: "#fff",
            }],
            options: {
                title: {
                    display: true,
                    fontSize: 18,
                    text: title,
                },
                legend: {
                    display: false,
                },
                tooltips: {
                    callbacks: {
                        title: (tooltipItems: ChartTooltipItem[]) => {
                            return new Date(tooltipItems[0].xLabel).toLocaleString();
                        },
                        label: (tooltipItem: ChartTooltipItem) => {
                            return isLogarithmic
                                ? Decimal.pow(10, tooltipItem.yLabel).toExponential(3)
                                : Number(tooltipItem.yLabel).toExponential(3);
                        },
                    },
                },
                elements: {
                    line: {
                        // Disables bezier curves
                        tension: 0,
                    },
                },
                scales: {
                    xAxes: [
                        {
                            type: "time",
                        },
                    ],
                    yAxes: [
                        {
                            type: "linear",
                            ticks: {
                                callback: (value: number): string => {
                                    return isLogarithmic
                                        ? Decimal.pow(10, value).toExponential(3)
                                        : value.toExponential(3);
                                },
                            },
                        },
                    ],
                },
            },
        };
    }

    private toTitleCase(srt: string): string {
        return srt[0].toUpperCase() + srt.substring(1);
    }
}
