import { Injectable } from "@angular/core";
import { HttpClient, HttpParams, HttpErrorResponse } from "@angular/common/http";
import { HttpErrorHandlerService } from "../httpErrorHandlerService/httpErrorHandlerService";

import "rxjs/add/operator/toPromise";

import { AuthenticationService } from "../authenticationService/authenticationService";
import { IPaginationMetadata } from "../../models";

export interface IGuildMember {
    highestZone: number;

    nickname: string;

    uid: string;

    userName: string;
}

export interface IClanData {
    clanName: string;

    currentRaidLevel: number;

    guildMembers: IGuildMember[];

    rank: number;
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
        private readonly authenticationService: AuthenticationService,
        private readonly http: HttpClient,
        private readonly httpErrorHandlerService: HttpErrorHandlerService,
    ) { }

    public getClan(clanName: string): Promise<IClanData> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http
                    .get<IClanData>(`/api/clans/${clanName}`, { headers, observe: "response" })
                    .toPromise();
            })
            .then(response => {
                if (response.status === 204) {
                    // This means the user is not part of a clan.
                    return null;
                }

                return response.body;
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("ClanService.getClan.error", err);
                return Promise.reject(err);
            });
    }

    public getLeaderboard(page: number, count: number): Promise<ILeaderboardSummaryListResponse> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http
                    .get<ILeaderboardSummaryListResponse>("/api/clans?page=" + page + "&count=" + count, { headers })
                    .toPromise();
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("ClanService.getLeaderboard.error", err);
                return Promise.reject(err);
            });
    }

    public getMessages(): Promise<IMessage[]> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                return this.http
                    .get<IMessage[]>("/api/clans/messages", { headers })
                    .toPromise();
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("ClanService.getMessages.error", err);
                return Promise.reject(err);
            });
    }

    public sendMessage(message: string, clanName: string): Promise<void> {
        return this.authenticationService.getAuthHeaders()
            .then(headers => {
                headers = headers.set("Content-Type", "application/x-www-form-urlencoded");
                let params = new HttpParams()
                    .set("message", message)
                    .set("clanName", clanName);

                // Angular doesn't encode '+' correctly. See: https://github.com/angular/angular/issues/11058
                let body = params.toString().replace(/\+/gi, "%2B");

                return this.http
                    .post<ISendMessageResponse>("/api/clans/messages", body, { headers })
                    .toPromise();
            })
            .then(response => {
                if (!response.success) {
                    return Promise.reject(response.reason);
                }

                return Promise.resolve();
            })
            .catch((err: HttpErrorResponse) => {
                this.httpErrorHandlerService.logError("ClanService.sendMessage.error", err);
                return Promise.reject(err);
            });
    }
}
