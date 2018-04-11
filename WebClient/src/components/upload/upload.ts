import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { UploadService, IUpload } from "../../services/uploadService/uploadService";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { Decimal } from "decimal.js";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { switchMap } from "rxjs/operators/switchMap";
import { SavedGame } from "../../models/savedGame";

@Component({
    selector: "upload",
    templateUrl: "./upload.html",
})
export class UploadComponent implements OnInit {
    public userInfo: IUserInfo;
    public errorMessage: string;
    public isLoading: boolean;

    public userName: string;
    public clanName: string;
    public uploadTime: string;
    public playStyle: string;
    public savedGame: SavedGame;

    // Miscellaneous stats
    public heroSoulsSpent: Decimal = new Decimal(0);
    public heroSoulsSacrificed: Decimal = new Decimal(0);
    public totalAncientSouls: Decimal = new Decimal(0);
    public transcendentPower: Decimal = new Decimal(0);
    public titanDamage: Decimal = new Decimal(0);
    public highestZoneThisTranscension: Decimal = new Decimal(0);
    public highestZoneLifetime: Decimal = new Decimal(0);
    public ascensionsThisTranscension: Decimal = new Decimal(0);
    public ascensionsLifetime: Decimal = new Decimal(0);
    public rubies: Decimal = new Decimal(0);
    public autoclickers: Decimal = new Decimal(0);

    private uploadId: number;
    private upload: IUpload;

    constructor(
        private readonly authenticationService: AuthenticationService,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly uploadService: UploadService,
        private readonly modalService: NgbModal,
    ) {
    }

    public ngOnInit(): void {
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => this.userInfo = userInfo);

        this.route.params.pipe(
            switchMap(params => {
                this.isLoading = true;
                return this.uploadService.get(+params.id);
            }),
        ).subscribe(upload => this.handleUpload(upload), () => this.handleError("There was a problem getting that upload"));
    }

    public openModal(modal: {}): void {
        this.errorMessage = null;
        this.modalService
            .open(modal)
            .result
            .then(() => {
                // Noop on close as the modal is expected to handle its own stuff.
            })
            .catch(() => {
                // Noop on dismissal
            });
    }

    public deleteUpload(closeModal: () => void): void {
        this.isLoading = true;
        this.uploadService.delete(this.uploadId)
            .then(() => {
                this.isLoading = false;
                this.router.navigate([`/users/${this.userName}`]);
            })
            .catch(() => this.handleError("There was a problem deleting that upload"))
            .then(closeModal);
    }

    private handleUpload(upload: IUpload): void {
        this.upload = upload;
        this.refresh();
        this.isLoading = false;
    }

    // tslint:disable-next-line:cyclomatic-complexity
    private refresh(): void {
        if (!this.upload) {
            return;
        }

        this.errorMessage = null;
        this.uploadId = this.upload.id;

        if (this.upload.user) {
            this.userName = this.upload.user.name;
            this.clanName = this.upload.user.clanName;
        } else {
            this.userName = null;
            this.clanName = null;
        }

        this.uploadTime = this.upload.timeSubmitted;
        this.playStyle = this.upload.playStyle;
        this.savedGame = new SavedGame(this.upload.content, this.upload.isScrubbed);

        let heroSoulsSpent = new Decimal(0);
        if (this.savedGame.data.ancients && this.savedGame.data.ancients.ancients) {
            for (let ancientId in this.savedGame.data.ancients.ancients) {
                heroSoulsSpent = heroSoulsSpent.plus(this.savedGame.data.ancients.ancients[ancientId].spentHeroSouls);
            }
        }

        this.heroSoulsSpent = heroSoulsSpent;
        this.heroSoulsSacrificed = new Decimal(this.savedGame.data.heroSoulsSacrificed || 0);
        this.totalAncientSouls = new Decimal(this.savedGame.data.ancientSoulsTotal || 0);
        this.transcendentPower = this.savedGame.data.transcendent
            ? new Decimal((2 + (23 * (1 - Math.pow(Math.E, -0.0003 * this.totalAncientSouls.toNumber())))) / 100)
            : new Decimal(0);
        this.titanDamage = new Decimal(this.savedGame.data.titanDamage || 0);
        this.highestZoneThisTranscension = new Decimal(this.savedGame.data.highestFinishedZonePersist || 0);
        this.highestZoneLifetime = Decimal.max(this.savedGame.data.pretranscendentHighestFinishedZone || 0, this.savedGame.data.transcendentHighestFinishedZone || 0, this.highestZoneThisTranscension);
        this.ascensionsLifetime = new Decimal(this.savedGame.data.numWorldResets || 0);
        this.ascensionsThisTranscension = this.savedGame.data.transcendent
            ? new Decimal(this.savedGame.data.numAscensionsThisTranscension || 0)
            : this.ascensionsLifetime;
        this.rubies = new Decimal(this.savedGame.data.rubies || 0);
        this.autoclickers = new Decimal(this.savedGame.data.autoclickers || 0)
            .plus(this.savedGame.data.dlcAutoclickers || 0);
    }

    private handleError(errorMessage: string): void {
        this.isLoading = false;
        this.errorMessage = errorMessage;
    }
}
