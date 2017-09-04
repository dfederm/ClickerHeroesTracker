import { Component, OnInit } from "@angular/core";

import { UploadService } from "../../services/uploadService/uploadService";
import { UserService, IProgressData } from "../../services/userService/userService";

import Decimal from "decimal.js";
import { ChartDataSets, ChartOptions, ChartTooltipItem } from "chart.js";

interface IProgressViewModel {
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
  selector: "dashboard",
  templateUrl: "./dashboard.html",
})
export class DashboardComponent implements OnInit {
  public isProgressError: boolean;
  public userName: string;
  public progress: IProgressViewModel;

  // TODO get the user's real settings
  private userSettings =
  {
    areUploadsPublic: true,
    hybridRatio: 1,
    logarithmicGraphScaleThreshold: 1000000,
    playStyle: "hybrid",
    scientificNotationThreshold: 100000,
    useEffectiveLevelForSuggestions: false,
    useExperimentalStats: true,
    useLogarithmicGraphScale: true,
    useReducedSolomonFormula: false,
    useScientificNotation: true,
  };

  constructor(
    private uploadService: UploadService,
    private userService: UserService,
  ) { }

  public ngOnInit(): void {
    // This is a pretty big hack. We don't have the right APIs to get the curret user info, so just get their first upload and get their user name from that.
    this.uploadService
      .getUploads(1, 1)
      .then(uploadsResponse => {
        return uploadsResponse
          && uploadsResponse.uploads
          && uploadsResponse.uploads.length
          && uploadsResponse.uploads[0]
          && uploadsResponse.uploads[0].id
          ? this.uploadService.get(uploadsResponse.uploads[0].id)
          : null;
      })
      .then(upload => {
        // Show the last week's worth
        let now = Date.now();
        let end = new Date(now);
        let start = new Date(now);
        start.setDate(start.getDate() - 7);

        if (upload
          && upload.user
          && upload.user.name) {
          this.userName = upload.user.name;
          return this.userService.getProgress(this.userName, start, end);
        }

        return null;
      })
      .then(progress => this.handleData(progress))
      .catch(() => this.isProgressError = true);
  }

  private handleData(progress: IProgressData): void {
    if (progress && Object.keys(progress.soulsSpentData).length === 0) {
      // No data
      return;
    }

    let data = progress.soulsSpentData;

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

    this.progress = {
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
          text: "Souls Spent",
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
}
