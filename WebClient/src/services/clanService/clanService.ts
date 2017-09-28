import { Injectable } from "@angular/core";
import { Http, RequestOptions } from "@angular/http";

import "rxjs/add/operator/toPromise";

import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { IPaginationMetadata } from "../pagination";

export interface IGuildMember {
    highestZone: number;

    nickname: string;

    uid: string;
}

export interface IClanData {
    clanName: string;

    currentRaidLevel: number;

    guildMembers: IGuildMember[];

    messages: IMessage[];
}

export interface IMessage {
    date: string;

    username: string;

    content: string;
}

export interface ILeaderboardSummaryListResponse {
    pagination: IPaginationMetadata;

    leaderboardClans: ILeaderboardClan[];
}

export interface ILeaderboardClan {
    name: string;

    currentRaidLevel: number;

    memberCount: number;

    rank: number;

    isUserClan: boolean;
}

export interface ISendMessageResponse {
    success: boolean;
    reason?: string;
}

@Injectable()
export class ClanService {
    constructor(
        private authenticationService: AuthenticationService,
        private http: Http,
    ) { }

    // TODO: this is really the user's clan.
    public getClan(): Promise<IClanData> {
        let headers = this.authenticationService.getAuthHeaders();
        let options = new RequestOptions({ headers });
        return this.http
            .get("/api/clans", options)
            .toPromise()
            .then(response => {
                if (response.status === 204) {
                    // This means the user is not part of a clan.
                    return null;
                }

                return response.json() as IClanData;
            })
            .catch(error => {
                let errorMessage = error.message || error.toString();
                appInsights.trackEvent("ClanService.getClan.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }

    // TODO this should be combined with the call above.
    public getUserClan(): Promise<ILeaderboardClan> {
        let headers = this.authenticationService.getAuthHeaders();
        let options = new RequestOptions({ headers });
        return this.http
            .get("/api/clans/userClan", options)
            .toPromise()
            .then(response => {
                if (response.status === 204) {
                    // This means the user is not part of a clan.
                    return null;
                }

                return response.json() as ILeaderboardClan;
            })
            .catch(error => {
                let errorMessage = error.message || error.toString();
                appInsights.trackEvent("ClanService.getUserClan.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }

    public getLeaderboard(page: number, count: number): Promise<ILeaderboardSummaryListResponse> {
        let headers = this.authenticationService.getAuthHeaders();
        let options = new RequestOptions({ headers });
        return this.http
            .get("/api/clans/leaderboard?page=" + page + "&count=" + count, options)
            .toPromise()
            .then(response => {
                return response.json() as ILeaderboardSummaryListResponse;
            })
            .catch(error => {
                let errorMessage = error.message || error.toString();
                appInsights.trackEvent("ClanService.getLeaderboard.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }

    public sendMessage(message: string, clanName: string): Promise<void> {
        let headers = this.authenticationService.getAuthHeaders();
        headers.append("Content-Type", "application/x-www-form-urlencoded");
        let options = new RequestOptions({ headers });
        let params = new URLSearchParams();
        params.append("message", message);
        params.append("clanName", clanName);
        return this.http
            .post("/api/clans/messages", params.toString(), options)
            .toPromise()
            .then(response => {
                let sendMessageResponse = response.json() as ISendMessageResponse;
                if (!sendMessageResponse.success) {
                    return Promise.reject(sendMessageResponse.reason);
                }

                return Promise.resolve();
            })
            .catch(error => {
                let errorMessage = error.message || error.toString();
                appInsights.trackEvent("ClanService.getLeaderboard.error", { message: errorMessage });
                return Promise.reject(errorMessage);
            });
    }
}
