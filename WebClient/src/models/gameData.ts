import gameDataJson from "../../../GameData.json";
export const gameData: IGameData = gameDataJson;

export interface IGameData {
    itemBonusTypes: { [id: string]: IItemBonusType };
    heroes: { [id: string]: IHeroData };
    upgrades: { [id: string]: IUpgradeData };
    ancients: { [id: string]: IAncientData };
    outsiders: { [id: string]: IOutsiderData };
}

export interface IItemBonusType {
    ancientId: number;
}

export interface IHeroData {
    id: number;
    name: string;
    clickDamageFormula: string;
    attackFormula: string;
    costFormula: string;
    baseCost: number | string;
    baseAttack: number | string;
    baseClickDamage: number;
    _live: string;
}

export interface IUpgradeData {
    id: number;
    heroLevelRequired: number;
    isPercentage: number;
    heroId: number;
    upgradeFunction: string;
    upgradeParams: string;
    _live: string;
    attribute: number;
}

export interface IAncientData {
    name: string;
    nonTranscendent?: boolean;
    levelCostFormula: string;
    levelAmountFormula: string;
    levelAmountParams?: string;
}

export interface IOutsiderData {
    id: number;
    name: string;
    levelAmountFormula: string;
}
