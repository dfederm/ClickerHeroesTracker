import * as pako from "pako";
import * as CryptoJS from "crypto-js";

export interface IItemData {
    bonusType1: number;
    bonus1Level: string;
    bonusType2: number;
    bonus2Level: string;
    bonusType3: number;
    bonus3Level: string;
    bonusType4: number;
    bonus4Level: string;
}

export interface IItemsData {
    slots: { [id: string]: number };
    items: { [id: string]: IItemData };
}

export interface IAncientData {
    id: number;
    level: number | string;
    spentHeroSouls: number | string;
    purchaseTime: number;
}

export interface IAncientsData {
    ancients: { [id: string]: IAncientData };
}

export interface IOutsiderData {
    id: number;
    level: number;
}

export interface IOutsidersData {
    outsiders: { [id: string]: IOutsiderData };
}

export interface ISavedGameData {
    transcendent: boolean;
    paidForRubyMultiplier: boolean;
    saveOrigin: string;
    unixTimestamp: number;

    primalSouls: number | string;
    heroSouls: number | string;
    heroSoulsSacrificed: number | string;
    ancientSoulsTotal: number | string;
    titanDamage: number | string;
    highestFinishedZonePersist: number | string;
    pretranscendentHighestFinishedZone: number | string;
    transcendentHighestFinishedZone: number | string;
    numAscensionsThisTranscension: number | string;
    numWorldResets: number | string;
    rubies: number | string;
    autoclickers: number | string;
    dlcAutoclickers: number | string;

    items: IItemsData;
    ancients: IAncientsData;
    outsiders: IOutsidersData;
}

type EncodingAlgorithm = "unknown" | "sprinkle" | "android" | "zlib";

// Ensure the logic here stays in sync with SavedGame.cs on the server
export class SavedGame {
    private static readonly sprinkleAntiCheatCode = "Fe12NAfA3R6z4k0z";

    private static readonly sprinkleSalt = "af0ik392jrmt0nsfdghy0";

    private static readonly androidPrefix = "ClickerHeroesAccountSO";

    private static readonly hashLength = 32;

    private static readonly zlibHash = "7a990d405d2c6fb93aa8fbb0ec1a3b23";

    private static readonly encodingAlgorithmHashes: { [hash: string]: EncodingAlgorithm } = {
        [SavedGame.zlibHash]: "zlib",
    };

    private static readonly decodeFuncs: { [encodingAlgorithm: string]: (content: string) => ISavedGameData } = {
        sprinkle: SavedGame.decodeSprinkle,
        android: SavedGame.decodeAndroid,
        zlib: SavedGame.decodeZlib,
    };

    private static readonly encodeFuncs: { [encodingAlgorithm: string]: (data: ISavedGameData) => string } = {
        sprinkle: SavedGame.encodeSprinkle,
        android: SavedGame.encodeAndroid,
        zlib: SavedGame.encodeZlib,
    };

    public data: ISavedGameData;

    private encoding: EncodingAlgorithm;

    private _scrubbedContent: string;

    constructor(
        public content: string,
        public isScrubbed: boolean,
    ) {
        this.encoding = SavedGame.determineEncodingAlgorithm(this.content);
        this.data = SavedGame.decode(this.content, this.encoding);

        if (this.isScrubbed) {
            this._scrubbedContent = content;
        }
    }

    public get scrubbedContent(): string {
        if (!this._scrubbedContent) {
            // Create a copy of the data before altering it
            let data = JSON.parse(JSON.stringify(this.data));

            // Based on https://github.com/Legocro/Clan-stripper/blob/master/script.js
            delete data.type;
            data.email = "";
            data.passwordHash = "";
            data.prevLoginTimestamp = 0;
            data.account = null;
            data.accountId = 0;
            data.loginValidated = false;
            data.uniqueId = "";
            data.subscribeEmail = "";

            this._scrubbedContent = SavedGame.encode(data, this.encoding);
        }

        return this._scrubbedContent;
    }

    public clone(): SavedGame {
        // Passing null content since it's faster to copy the private state than to re-parse
        let savedGame = new SavedGame(null, this.isScrubbed);
        savedGame.content = this.content;
        savedGame.encoding = this.encoding;
        savedGame.data = JSON.parse(JSON.stringify(this.data));
        savedGame._scrubbedContent = this._scrubbedContent;
        return savedGame;
    }

    public updateContent(): void {
        this.content = SavedGame.encode(this.data, this.encoding);
        this._scrubbedContent = null;
    }

    private static determineEncodingAlgorithm(content: string): EncodingAlgorithm {
        if (content == null || content.length < SavedGame.hashLength) {
            return "unknown";
        }

        // Read the first 32 characters as they are the MD5 hash of the used algorithm
        let encodingAlgorithmHash = content.substring(0, SavedGame.hashLength);

        // Test if the MD5 hash header corresponds to a known encoding algorithm
        let encodingAlgorithm = SavedGame.encodingAlgorithmHashes[encodingAlgorithmHash];
        if (encodingAlgorithm) {
            return encodingAlgorithm;
        }

        // Legacy encodings
        return content.indexOf(SavedGame.androidPrefix) >= 0
            ? "android"
            : "sprinkle";
    }

    private static decode(content: string, encoding: EncodingAlgorithm): ISavedGameData {
        let decodeFunc = SavedGame.decodeFuncs[encoding];
        return decodeFunc ? decodeFunc(content) : null;
    }

    private static encode(data: ISavedGameData, encoding: EncodingAlgorithm): string {
        let encodeFunc = SavedGame.encodeFuncs[encoding];
        return encodeFunc ? encodeFunc(data) : null;
    }

    private static decodeSprinkle(content: string): ISavedGameData {
        let pieces = content.split(SavedGame.sprinkleAntiCheatCode);
        if (pieces.length !== 2) {
            // Couldn't find anti-cheat
            return null;
        }

        let data = pieces[0];
        let hash = pieces[1];

        // Remove every other character, AKA "unsprinkle".
        let unsprinkled = "";
        for (let i = 0; i < data.length; i += 2) {
            unsprinkled += data[i];
        }

        // Validation
        let expectedHash = CryptoJS.MD5(unsprinkled + SavedGame.sprinkleSalt).toString();
        // tslint:disable-next-line:possible-timing-attack This isn't a security issue
        if (hash !== expectedHash) {
            return null;
        }

        // Decode
        return JSON.parse(atob(unsprinkled));
    }

    private static encodeSprinkle(data: ISavedGameData): string {
        let json = JSON.stringify(data);
        let base64Data = btoa(json);

        // Inject an arbitrary character every other character, AKA "sprinkle".
        let sprinkled = "";
        for (let i = 0; i < base64Data.length; i++) {
            sprinkled += base64Data[i];
            sprinkled += "0";
        }

        let hash = CryptoJS.MD5(base64Data + SavedGame.sprinkleSalt).toString();
        return sprinkled + SavedGame.sprinkleAntiCheatCode + hash;
    }

    private static decodeAndroid(content: string): ISavedGameData {
        // Get the index of the first open brace
        let firstBrace = content.indexOf("{");
        if (firstBrace < 0) {
            return null;
        }

        let json = content
            .substring(firstBrace)
            .replace("\\\"", "\"")
            .replace("\\\\", "\\");

        return JSON.parse(json);
    }

    private static encodeAndroid(data: ISavedGameData): string {
        let json = JSON.stringify(data)
            .replace("\\", "\\\\")
            .replace("\"", "\\\"");

        // No idea how accurate these chracters are. Just using what was found in one particular save.
        return "?D?TCSO" + SavedGame.androidPrefix + "\tjson??%" + json;
    }

    private static decodeZlib(content: string): ISavedGameData {
        let result = content.slice(SavedGame.hashLength);
        let data = pako.inflate(atob(result), { to: "string" });
        return JSON.parse(data);

    }

    private static encodeZlib(data: ISavedGameData): string {
        let json = JSON.stringify(data);
        let encodedData = pako.deflate(json, { to: "string" });
        return SavedGame.zlibHash + btoa(encodedData);
    }
}
